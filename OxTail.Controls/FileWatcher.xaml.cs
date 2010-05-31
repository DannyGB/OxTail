/*****************************************************************
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Threading;
    using OxTailLogic.PatternMatching;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    
    /// <summary>
    /// Interaction logic for FileWatcher.xaml
    /// </summary>
    public partial class FileWatcher : UserControl, IDisposable
    {
        private StreamReader _streamReader = null;
        private FileStream _fileStream = null;
        private BackgroundWorker _bw = null;
        private object _fileReadLock = new object();
        private long _startLine = 0;
        private long _numberOfLinesInFile = 0;
        private int _visibleLines = 20;
        private Encoding _tailEncoding = Encoding.Default;
        private DateTime _dateLastTime = DateTime.MinValue;
        private int _chunkSize = 16384;
        private bool _followTail = true;
        private FileInfo _fileInfo;
        private DoWorkEventArgs _doWorkEventArgs;
        private string _newlineCharacters = null;
        private long _currentLength = 0;
        private long _previousLength = 0;
        private long _offset = 0;
        private List<string> _readLines = new List<string>();
        private NewlineDetectionMode _newlineDetectionMode;
        private static IStringPatternMatching patternMatching = StringPatternMatching.CreatePatternMatching();
        private long _previousLastLineOffset = 0;

        public string NewlineCharacters
        {
            get
            {
                return this._newlineCharacters;
            }
            set
            {
                this._newlineCharacters = value;
            }
        }

        public NewlineDetectionMode NewlineDetectionMode 
        {
            get 
            { 
                return this._newlineDetectionMode; 
            }
            set 
            { 
                this._newlineDetectionMode = value;
                this.SetNewlineCharacters();
                this.Refresh();
            }
        }

        private void Refresh()
        {
            this.StartLine = this.CalculateStartLine();
            if (this.StartLine > -1)
            {
                ReadLines();
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(this.Update));
            }
        }

        public Encoding TailEncoding
        {
            get { return this._tailEncoding; }
            set { this._tailEncoding = value; }
        }

        private void SetNewlineCharacters()
        {
            switch (this.NewlineDetectionMode)
            {
                case NewlineDetectionMode.Auto:
                    this.AutoDetectNewLineCharacters();
                    break;
                case NewlineDetectionMode.Windows:
                    this.NewlineCharacters = "\r\n";
                    break;
                case NewlineDetectionMode.Unix:
                    this.NewlineCharacters = "\n";
                    break;
                case NewlineDetectionMode.Mac:
                    this.NewlineCharacters = "\r";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Detects the new line by scanning for first new line in the file 
        /// </summary>
        private void AutoDetectNewLineCharacters()
        {
            char[] chars = new char[2];
            this.NewlineCharacters = null;
            // read first line of file
            this.PositionFilePointerToOffset(0);
            string line = this._streamReader.ReadLine();
            if (line != null)
            {
                this.PositionFilePointerToOffset(line.Length);
                this._streamReader.Read(chars, 0, 2);
                this.NewlineCharacters = new string(chars);
                // if the second character is not line feed ("\n") then it must have been a single character newline - either unix ("\n") or mac ("\r")
                if (this.NewlineCharacters.Substring(1, 1) != "\n")
                {
                    this.NewlineCharacters = this.NewlineCharacters.Substring(0, 1);
                }
            }
        }

        public int VisibleLines
        {
            get { return this._visibleLines; }
            set { this._visibleLines = value; }
        }

        public bool FollowTail
        {
            get { return this._followTail; }
            set { this._followTail = value; }
        }

        public long StartLine
        {
            get { return this._startLine; }
            set { this._startLine = value; }
        }

        private void Update()
        {
            IStringPatternMatching highlighting = new StringPatternMatching();
            for (int i = 0; i < this._readLines.Count; i++)
            {
                //TextBlock textBlock = new TextBlock(new Run(this._readLines[i]));
                HighlightedItem item = this.Highlight(this._readLines[i]);
                if (this.Lines.Count <= i)
                {
                    this.Lines.Add(item);
                }
                else
                {
                    this.Lines[i] = item;
                }
            }
            // todo: add separate status bar item for start line, number of visible lines and total file lines. Probably file size too.
            this.ReportProgress(0, string.Format("Showing lines {0}-{1} of {2}", this._startLine, this._startLine + this._visibleLines, this._numberOfLinesInFile), false, System.Windows.Visibility.Hidden);
        }

        public static IEnumerable<HighlightItem> FindFirstHighlightByText(IEnumerable<HighlightItem> coll, string text)
        {
            foreach (HighlightItem item in coll)
            {
                // Empty pattern should not exist (a blank line should be a "special" Highlight item?
                if (!string.IsNullOrEmpty(item.Pattern) && !string.IsNullOrEmpty(text) )
                {
                    if (text == item.Pattern || patternMatching.MatchPattern(text, item.Pattern))
                    {
                        yield return item;
                    }
                }
            }
        }

        private HighlightedItem Highlight(string text)
        {
            HighlightedItem highlighted = new HighlightedItem();
            highlighted.Text = text;

            highlighted.ForeColour = Colors.Black;
            highlighted.BackColour = Colors.AliceBlue;

            foreach (HighlightItem highlight in FindFirstHighlightByText(this.Patterns, text))
            {
                highlighted.ForeColour = highlight.ForeColour;
                highlighted.BackColour = highlight.BackColour;
                break; // use the first one we come across in the list - items at the top are most important
            }

            return highlighted;
        }

        private SeekOrigin CalculateClosestSeekOrigin(long target, out long smallestOffset)
        {
            long beginOffset = target;
            // we want the following be negative for use in conjunction with SeekOrigin.Current or SeekOrigin.End
            long currentOffset = target - this._fileStream.Position;
            long endOffset = target - this._currentLength;

            smallestOffset = Math.Min(Math.Min(beginOffset, Math.Abs(currentOffset)), Math.Abs(endOffset));

            SeekOrigin closestSeekOrigin;

            if (smallestOffset == beginOffset)
            {
                closestSeekOrigin = SeekOrigin.Begin;
            }
            else if (smallestOffset == Math.Abs(currentOffset))
            {
                closestSeekOrigin = SeekOrigin.Current;
                smallestOffset = currentOffset;
            }
            else
            {
                closestSeekOrigin = SeekOrigin.End;
                smallestOffset = endOffset;
            }

            return closestSeekOrigin;
        }

        private void ReadLines()
        {
            this._readLines.Clear();
            // educated guess at quickest direction to go - position the cursor at the beginning if our first line is in the first "half" (variable line lengths!) of the file
            if (this._startLine < (this._numberOfLinesInFile / 2))
            {
                ReadLinesForwards();
            }
            else
            {
                ReadLinesBackwards();
            }
        }

        private void PositionFilePointerToOffset(long target)
        {
            this._streamReader.DiscardBufferedData();
            //long smallestOffset;
            //SeekOrigin seekOrigin = this.CalculateClosestSeekOrigin(target, out smallestOffset);
            //this._fileStream.Seek(smallestOffset, seekOrigin);
            this._fileStream.Seek(target, SeekOrigin.Begin);
        }

        private void ReadLinesBackwards()
        {
            int indexIncrement = -this._chunkSize;
            int chunksRead = 0;
            char[] chunk = new char[this._chunkSize];
            long linesToRead;
            StringBuilder chunkAsString = new StringBuilder();

            // calculate how many lines we need to read
            linesToRead = this._numberOfLinesInFile - this._startLine;

            // go directly to the end of the file
            this.PositionFilePointerToOffset(this._currentLength);
            this._offset = Math.Max(this._currentLength + indexIncrement, 0);

            do
            {
                // honour cancellation request
                if (this.CancellationPending())
                {
                    return;
                }

                // shorten chunk to remaining file if necessary
                if (this._offset + indexIncrement < 0)
                {
                    indexIncrement = -(int)this._fileStream.Position;
                }

                // set pointer to beginning of next chunk
                this.PositionFilePointerToOffset(this._offset);
                // read the chunk 
                this._streamReader.Read(chunk, 0, this._chunkSize);
                chunksRead++;
                chunkAsString.Clear();
                chunkAsString.Append(chunk);

                // rudimentary way to split the chunk into seperate lines
                string[] lineArray = chunkAsString.ToString().Substring(0, Math.Max(Math.Abs(indexIncrement), (int)(this._currentLength - this._offset))).Split(new string[] { this._newlineCharacters }, StringSplitOptions.None);

                // insert the last line of this chunk at the beginning of the first line of the previous chunk
                // this covers the usual scenario where a chunk disects a line
                if (lineArray.Length > 0)
                {
                    if (this._readLines.Count == 0)
                    {
                        this._readLines.Add(string.Empty);
                    }
                    this._readLines[0] = lineArray[lineArray.Length - 1] + this._readLines[0].Trim('\0');
                }

                // we will almost definitely have read more than we needed to unless by some miracle the nth newline is exactly in position 0 of the most recent chunk
                int linesToTrim = Math.Max((int)(this._readLines.Count - 1 + lineArray.Length - linesToRead), 0);

                // insert our lines at the beginning (except the last one because that has been inserted at the beginning of the first line of the previous chunk) 
                for (int i = lineArray.Length - 2; i >= linesToTrim; i--)
                {
                    this._readLines.Insert(0, lineArray[i].Trim('\0'));
                }

                // have we read enough lines?
                if (this._readLines.Count >= linesToRead)
                {
                    break;
                }

                ReportProgress((int)(Math.Min(this._readLines.Count, linesToRead) * 100 / linesToRead), string.Format("Skipped {0} of {1} lines backwards", this._readLines.Count, linesToRead), false, System.Windows.Visibility.Visible);
                
                // start of next chunk to read
                this._offset += indexIncrement;

                // rewind the pointer back to where it started on the most recent read operation
                this.PositionFilePointerToOffset(this._offset);

            } while (this._readLines.Count <= linesToRead && this._offset > 0);

            //// we will almost definitely have read more than we needed to unless by some miracle the nth newline is exactly in position 0 of the most recent chunk
            //int linesToTrim = Math.Max((int)(this._readLines.Count - linesToRead), 1);
            //if (linesToTrim > 0)
            //{
            //    this._readLines.RemoveRange(0, linesToTrim);
            //}
        }

        private void ReadLinesForwards()
        {
            this.PositionFilePointerToOffset(0);
            // skip to start line
            for (int i = 0; i < this._startLine; i++)
            {
                // honour cancellation request
                if (this.CancellationPending())
                {
                    return;
                }

                //this._streamReader.DiscardBufferedData();
                this._streamReader.ReadLine();
            }
            // load lines
            for (int i = 0; i < this._visibleLines; i++)
            {
                string line = this._streamReader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.TrimEnd('\0');
                }
                this._readLines.Add(line);
            }
        }

        private int _interval = 1000;
        int Interval 
        {
            get { return this._interval; }
            set { this._interval = value; }
        }

        public FileWatcher()
        {
            InitializeComponent();
            
            // setup the line ending detection combo box
            this.comboBoxNewlineDetection.ItemsSource = Enum.GetNames(typeof(NewlineDetectionMode));
            Binding bindingNewlineDetection = new Binding("NewlineDetectionMode");
            bindingNewlineDetection.Source = this;
            this.comboBoxNewlineDetection.SetBinding(ComboBox.TextProperty, bindingNewlineDetection);

            // setup the encoding combo box
            this.comboBoxEncoding.ItemsSource = Encoding.GetEncodings();
            Binding bindingEncoding = new Binding("TailEncoding");
            bindingEncoding.Source = this;
            this.comboBoxEncoding.SetBinding(ComboBox.SelectedItemProperty, bindingEncoding);
            comboBoxEncoding.DisplayMemberPath = "DisplayName";
            comboBoxEncoding.SelectedValuePath = "DisplayName";

            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bw_RunWorkerCompleted);
        }

        void Patterns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Update();
        }

        void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            return;
        }

        public void Start(string filename)
        {
            this._fileInfo = new FileInfo(filename);
            this._fileStream = new FileStream(filename,FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize);
            this._streamReader = new StreamReader(this._fileStream, this.TailEncoding, true, this._chunkSize);
            this.SetNewlineCharacters();    
            if (!this._bw.IsBusy)
            {
                this._bw.RunWorkerAsync();
            }
        }

        public ItemCollection Lines
        {
            get { return this.colourfulListView.Items; }
        }

        private delegate void StatusNotificationDelegate(int progressPercentage, FileWatcherProgressChangedUserState userState);

        private void ShowStatus(int progressPercentage, FileWatcherProgressChangedUserState userState)
        {
            this.textBlockMainStatus.Text = userState.MainStatusText;
            this.progressBarStatus.Visibility = userState.ProgressBarVisibility;
            this.progressBarStatus.IsIndeterminate = userState.ProgressBarIndeterminate;
            this.progressBarStatus.Value = progressPercentage;
        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            this._doWorkEventArgs = e;
            while (!this._bw.CancellationPending)
            {
                this.Tail(this._fileStream.Length);
                if (!this.CancellationPending())
                {
                    Thread.Sleep(this.Interval);
                }
            }
            e.Result = "Completed";
            return;
        }

        /// <summary>
        /// Reads only the latest additional text that has been appended to the file
        /// </summary>
        private void Tail(long length)
        {
            lock (this._fileReadLock)
            {
                this._fileInfo.Refresh(); // see if file has changed
                if (this.FollowTail && (this._currentLength != this._fileStream.Length || this._dateLastTime.Ticks != Math.Max(this._fileInfo.LastWriteTime.Ticks, this._fileInfo.CreationTime.Ticks)))
                {
                    this._previousLength = this._currentLength;
                    this._currentLength = length;
                    this._dateLastTime = new DateTime(Math.Max(this._fileInfo.CreationTime.Ticks, this._fileInfo.LastWriteTime.Ticks));
                    this.Refresh();
                }

            }
        }

        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileWatcher));

        // Provide CLR accessors for the event
        public event RoutedEventHandler FileChanged
        {
            add { AddHandler(FileChangedEvent, value); }
            remove { RemoveHandler(FileChangedEvent, value); }
        }

        private void RaiseFileChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(FileWatcher.FileChangedEvent);
            this.RaiseEvent(new RoutedEventArgs(FileWatcher.FileChangedEvent, this));
        }

        private long CountLinesInFile()
        {
            long pos = this._fileStream.Position; // save current position
            if (this._previousLength == 0)
            {
                this._numberOfLinesInFile = 0;
                this._previousLastLineOffset = 0;
            }
            if (this.NewlineCharacters != "\r\n" && this.NewlineCharacters != "\n" && this.NewlineCharacters != "\r")
            {
                this.SetNewlineCharacters();    
            }
            this.PositionFilePointerToOffset(this._previousLastLineOffset); // only count from previous length to save time
            long offset = this._fileStream.Position;
            while (offset < this._currentLength)
            {
                // honour cancellation request
                if (this.CancellationPending())
                {
                    return -1;
                }
                this._previousLastLineOffset = offset;
                // skip lines and keep track of the position of the end of the line by measuring the length of the line and adding on the length of our line ending
                // n.b. StreamReader.Readline reads a block of data from the underlying filestream so does not reflect the position of the end of each line
                string line = this._streamReader.ReadLine();
                if (line == null)
                {
                    break;
                }
                offset += line.Length + this._newlineCharacters.Length;
                if (this._previousLength == 0 ||  offset < this._currentLength)
                {
                    this._numberOfLinesInFile++;
                }
                if (this._numberOfLinesInFile % 1000 == 0)
                {
                    ReportProgress((int)(this._fileStream.Position * 100 / this._currentLength), string.Format("counting lines in file: {0}", this._numberOfLinesInFile), false, System.Windows.Visibility.Visible);
                }
            }
            this.PositionFilePointerToOffset(pos); // back to saved position
            ReportProgress(100, string.Format("counted {0} lines", this._numberOfLinesInFile), false, System.Windows.Visibility.Hidden);
            return this._numberOfLinesInFile;
        }

        private bool CancellationPending()
        {
            if (this._bw.CancellationPending)
            {
                this._doWorkEventArgs.Cancel = true;
            }
            return this._bw.CancellationPending;
        }

        private long CalculateStartLine()
        {
            long newStartLine;
            if (this.CountLinesInFile() < 0)
            {
                return -1;
            }

            if (this.FollowTail)
            {
                newStartLine = Math.Max(this._numberOfLinesInFile - this.VisibleLines, 0);
            }
            else
            {
                // return the current start line if we're not following the tail
                newStartLine = this.StartLine;
            }
            ReportProgress(0, "start line = " + newStartLine.ToString(), false, Visibility.Hidden);
            
            return newStartLine;
        }

        private void ReportProgress(int progressBarPercentage, string status, bool progressBarIndeterminate, System.Windows.Visibility progressBarVisibility)
        {
            this.Dispatcher.Invoke(new StatusNotificationDelegate(this.ShowStatus), DispatcherPriority.Render, progressBarPercentage, new FileWatcherProgressChangedUserState(status, progressBarIndeterminate, progressBarVisibility));
            //Thread.Sleep(100);
        }

        internal void Stop()
        {
            if (this._bw.IsBusy)
            {
                this._bw.CancelAsync();
            }
            // sleep until the worker has terminated
            while (this._bw.IsBusy)
            {
                ReportProgress(0, "Waiting for watcher to exit", true, System.Windows.Visibility.Visible);
                Thread.Sleep(100);
            }
            return;
        }

        public void Dispose()
        {
            if (this._streamReader != null)
            {
                this._streamReader.Close();
                this._streamReader.Dispose();
            }
            if (this._fileStream != null)
            {
                this._fileStream.Close();
                this._fileStream.Dispose();
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            lock (this._fileReadLock)
            {
                this._previousLength = 0;
                this.Refresh();
            }
        }

        private ObservableCollection<HighlightItem> _patterns;
        public ObservableCollection<HighlightItem> Patterns
        {
            get
            {
                return this._patterns;
            }
            set
            {
                this._patterns = value;
                this.Patterns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Patterns_CollectionChanged);
            }
        }
    }
}

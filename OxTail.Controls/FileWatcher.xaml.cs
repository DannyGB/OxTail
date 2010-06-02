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
    using OxTailHelpers;
    using OxTailLogic.PatternMatching;
    using System.Windows.Media;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Interaction logic for FileWatcher.xaml
    /// </summary>
    public partial class FileWatcher : UserControl, IDisposable
    {
        private BackgroundWorker _bw = null;
        private object _fileReadLock = new object();
        private long _startLine = 0;
        private int _visibleLines = 20;
        private Encoding _tailEncoding = Encoding.Default;
        private DateTime _dateLastTime = DateTime.MinValue;
        private int _chunkSize = 16384;
        private bool _followTail = true;
        private string _filename;
        private DoWorkEventArgs _doWorkEventArgs;
        private string _newlineCharacters = null;
        private long _currentLength = 0;
        private long _previousLength = 0;
        private long _offset = 0;
        private List<string> _readLines = new List<string>();
        private NewlineDetectionMode _newlineDetectionMode;
        private static IStringPatternMatching _patternMatching = StringPatternMatching.CreatePatternMatching();
        private long _previousLastLineOffset = 0;
        private ScrollContentPresenter _scrollContentPresenter = null;
        private ScrollViewer _scrollViewer = null;
        private int _interval = 1000;

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

        public int LinesInFile { get; set; }

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
                this.Refresh(false, true);
            }
        }

        private void Refresh(bool tail, bool calculateStartLine)
        {
            if (calculateStartLine)
            {
                this.StartLine = this.CalculateStartLine();
            }
            if (this.FollowTail || !tail)
            {
                if (this.StartLine > -1)
                {
                    ReadLines();
                }
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(this.Update));
            }
            this._previousLength = this._currentLength;
        }

        private StreamReader OpenFile()
        {
            return new StreamReader(new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize), this.TailEncoding, true, this._chunkSize);
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
            using (StreamReader streamReader = this.OpenFile())
            {
                // read first line of file
                this.PositionFilePointerToOffset(streamReader, 0);
                string line = streamReader.ReadLine();
                if (line != null)
                {
                    this.PositionFilePointerToOffset(streamReader, line.Length);
                    streamReader.Read(chars, 0, 2);
                    this.NewlineCharacters = new string(chars);
                    // if the second character is not line feed ("\n") then it must have been a single character newline - either unix ("\n") or mac ("\r")
                    if (this.NewlineCharacters.Substring(1, 1) != "\n")
                    {
                        this.NewlineCharacters = this.NewlineCharacters.Substring(0, 1);
                    }
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

        /// <summary>
        /// Updates the current display
        /// </summary>
        private void Update()
        {
            HighlightedItem blank = new HighlightedItem();
            // make the number of items match the number of lines
            if (this.LinesInFile > this.Lines.Count)
            {
                int extra = this.LinesInFile - this.Lines.Count;
                for (int i = 0; i < extra; i++)
                {
                    this.Lines.Add(blank);
                }
            }
            // remove excess lines 
            else if (this.LinesInFile < this.Lines.Count)
            {
                int linesToRemove = this.LinesInFile - this.Lines.Count;
                for (int i = 0; i < linesToRemove; i++)
                {
                    if (this.Lines.Count > 0)
                    {
                        this.Lines.RemoveAt(this.Lines.Count - 1);
                    }
                }
            }

            // highlight and add/update items to the listview
            for (int i = (int)this.StartLine; i < this.StartLine + this._readLines.Count; i++)
            {
                HighlightedItem item = null;
                //if (i >= this.StartLine && i < this.StartLine + this.VisibleLines && this._readLines.Count > i - (int)this.StartLine)
                {
                    item = this.Highlight(i.ToString() + ": " + this._readLines[i - (int)this.StartLine].Replace("\r", "^M").Replace("\n", "^J")); // in case of incorrectly selected line end
                }
                //else
                //{
                //    item = blank;
                //}
                if (this.Lines.Count <= i)
                {
                    this.Lines.Add(item);
                }
                else
                {
                    this.Lines[i] = item;
                }
            }
            if (this.FollowTail)
            {
                this._scrollViewer.ScrollToEnd();
            }
            // update the status bar
            this.ReportProgress(0, String.Empty, false, System.Windows.Visibility.Hidden);
            this.textBlockStartLine.Text = this.StartLine.ToString();
            this.textBlockLinesInFile.Text = this.LinesInFile.ToString();
            this.textBlockVisibleLines.Text = this.VisibleLines.ToString();
        }

        public static IEnumerable<HighlightItem> FindFirstHighlightByText(IEnumerable<HighlightItem> coll, string text)
        {
            foreach (HighlightItem item in coll)
            {
                // Empty pattern should not exist (a blank line should be a "special" Highlight item?
                if (!string.IsNullOrEmpty(item.Pattern) && !string.IsNullOrEmpty(text))
                {
                    if (text == item.Pattern || _patternMatching.MatchPattern(text, item.Pattern))
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

            // todo: make configurable?
            highlighted.ForeColour = Colors.Black;
            highlighted.BackColour = Colors.White;

            foreach (HighlightItem highlight in FindFirstHighlightByText(this.Patterns, text))
            {
                highlighted.ForeColour = highlight.ForeColour;
                highlighted.BackColour = highlight.BackColour;
                break; // use the first one we come across in the list - items at the top are most important
            }

            return highlighted;
        }

        private SeekOrigin CalculateClosestSeekOrigin(StreamReader streamReader, long target, out long smallestOffset)
        {
            long beginOffset = target;
            // we want the following to be negative for use in conjunction with SeekOrigin.Current or SeekOrigin.End
            long currentOffset = target - streamReader.BaseStream.Position;
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
            if (this._startLine < (this.LinesInFile / 2))
            {
                ReadLinesForwards();
            }
            else
            {
                ReadLinesBackwards();
            }
        }

        private void PositionFilePointerToOffset(StreamReader streamReader, long target)
        {
            streamReader.DiscardBufferedData();
            long smallestOffset;
            SeekOrigin seekOrigin = this.CalculateClosestSeekOrigin(streamReader, target, out smallestOffset);
            streamReader.BaseStream.Seek(smallestOffset, seekOrigin);
        }

        private void ReadLinesBackwards()
        {
            int indexIncrement = -this._chunkSize;
            int chunksRead = 0;
            char[] chunk = new char[this._chunkSize];
            long linesToRead;
            StringBuilder chunkAsString = new StringBuilder();

            // calculate how many lines we need to read
            linesToRead = this.LinesInFile - this._startLine;

            using (StreamReader streamReader = this.OpenFile())
            {
                // go directly to the end of the file
                this.PositionFilePointerToOffset(streamReader, this._currentLength);
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
                        indexIncrement = -(int)streamReader.BaseStream.Position;
                    }

                    // set pointer to beginning of next chunk
                    this.PositionFilePointerToOffset(streamReader, this._offset);
                    // read the chunk 
                    streamReader.Read(chunk, 0, this._chunkSize);
                    chunksRead++;
                    chunkAsString.Clear();
                    chunkAsString.Append(chunk);

                    // rudimentary way to split the chunk into seperate lines
                    string[] lineArray = chunkAsString.ToString().TrimEnd(new char[] { '\0' }).Split(new string[] { this._newlineCharacters }, StringSplitOptions.None);

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
                    this.PositionFilePointerToOffset(streamReader, this._offset);

                } while (this._readLines.Count <= linesToRead && this._offset > 0);

            }
        }

        private void ReadLinesForwards()
        {
            using (StreamReader streamReader = this.OpenFile())
            {
                this.PositionFilePointerToOffset(streamReader, 0);
                // skip to start line
                for (int i = 0; i < this._startLine; i++)
                {
                    // honour cancellation request
                    if (this.CancellationPending())
                    {
                        return;
                    }

                    streamReader.ReadLine();
                    if (i % 100 == 0)
                    {
                        ReportProgress((int)(i * 100 / this._startLine), string.Format("Skipped {0} of {1} lines forwards", this._readLines.Count, this._startLine), false, System.Windows.Visibility.Visible);
                    }

                }
                // load lines
                for (int i = 0; i < this.VisibleLines; i++)
                {
                    string line = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        line = line.TrimEnd('\0');
                    }
                    this._readLines.Add(line);
                }
            }
        }

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

            // bind the follow tail checkbox
            Binding bindingFollowTail = new Binding("FollowTail");
            bindingFollowTail.Source = this;
            this.checkBoxFollowTail.SetBinding(CheckBox.IsCheckedProperty, bindingFollowTail);

            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bw_RunWorkerCompleted);
        }

        void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            return;
        }

        public void Start(string filename)
        {
            this._filename = filename;
            this.SetNewlineCharacters();
            //this.CalculateVisibleLines();
            List<ScrollViewer> scrollviewers = this.GetVisualChildren<ScrollViewer>();
            this._scrollViewer = scrollviewers[0];
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
                this.Tail();
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
        private void Tail()
        {
            lock (this._fileReadLock)
            {
                // see if file has changed
                FileInfo fileInfo = new FileInfo(this._filename);
                if ((this._currentLength != fileInfo.Length || this._dateLastTime.Ticks != Math.Max(fileInfo.LastWriteTime.Ticks, fileInfo.CreationTime.Ticks)))
                {
                    this._currentLength = fileInfo.Length;
                    this._dateLastTime = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));
                    this.Refresh(true, true);
                    this.OnFileChanged();
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

        private void OnFileChanged()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(FileWatcher.FileChangedEvent);
                this.RaiseEvent(new RoutedEventArgs(FileWatcher.FileChangedEvent, this));
            }
            ));
        }

        private long CountLinesInFile()
        {
            using (StreamReader streamReader = this.OpenFile())
            {
                if (this._previousLength == 0)
                {
                    this.LinesInFile = 0;
                    this._previousLastLineOffset = 0;
                }
                if (this.NewlineCharacters != "\r\n" && this.NewlineCharacters != "\n" && this.NewlineCharacters != "\r")
                {
                    this.SetNewlineCharacters();
                }
                this.PositionFilePointerToOffset(streamReader, this._previousLastLineOffset); // only count from previous length to save time
                long offset = streamReader.BaseStream.Position;
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
                    string line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    offset += line.Length + this._newlineCharacters.Length;
                    if (this._previousLength == 0 || offset < this._currentLength)
                    {
                        this.LinesInFile++;
                    }

                    if (this.LinesInFile % 1000 == 0)
                    {
                        ReportProgress((int)(offset * 100 / this._currentLength), string.Format("counting lines in file: {0}", this.LinesInFile), false, System.Windows.Visibility.Visible);
                    }
                }
                ReportProgress(100, string.Format("counted {0} lines", this.LinesInFile), false, System.Windows.Visibility.Hidden);
                return this.LinesInFile;
            }
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
            // only count lines if the file length has changed
            if (this._previousLength != this._currentLength)
            {
                if (this.CountLinesInFile() < 0)
                {
                    return -1;
                }
            }

            if (this.FollowTail)
            {
                newStartLine = Math.Max(this.LinesInFile - this.VisibleLines, 0);
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
            //Thread.Sleep(20);
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
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            lock (this._fileReadLock)
            {
                this._previousLength = 0;
                this.Refresh(false, true);
            }
        }

        private BindingList<HighlightItem> _patterns;
        public BindingList<HighlightItem> Patterns
        {
            get
            {
                return this._patterns;
            }
            set
            {
                this._patterns = value;
                this.Patterns.ListChanged += new ListChangedEventHandler(Patterns_ListChanged);
            }
        }

        void Patterns_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.Update();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (this)
            {
                //int visible = this.VisibleLines;
                //this.CalculateVisibleLines();
                //if (visible != this.VisibleLines)
                //{
                //    this.Refresh(false, false);
                //}
            }
        }

        private void CalculateVisibleLines()
        {
            //if (!this.colourfulListView.HasItems)
            //{
            //    this.colourfulListView.Items.Add("");
            //}
            //object lvi = this.colourfulListView.ItemContainerGenerator.ContainerFromIndex(0);
            //double inner = GetScrollContentPresenterHeight();

            //if (lvi is ListViewItem)
            //{
            //    double lineHeight = ((ListViewItem)lvi).ActualHeight;
            //    this.VisibleLines = (int)((inner) / lineHeight);
            //}
            if (_scrollViewer != null)
            {
                this.VisibleLines = (int)this._scrollViewer.ViewportHeight;
            }
        }

        private double GetScrollContentPresenterHeight()
        {
            if (this._scrollContentPresenter == null)
            {
                List<ScrollContentPresenter> scrollers = this.colourfulListView.GetVisualChildren<ScrollContentPresenter>();
                if (scrollers != null && scrollers.Count > 0)
                {
                    foreach (ScrollContentPresenter scroller in scrollers)
                    {
                        if (scroller.IsVisible)
                        {
                            this._scrollContentPresenter = scroller;
                            break;
                        }
                    }
                }
            }
            return Math.Floor(this._scrollContentPresenter.ActualHeight);
        }

        private void colourfulListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.ViewportHeightChange != 0)
            {
                this.VisibleLines = (int)e.ViewportHeight;
                this.StartLine = (int)e.VerticalOffset;
                this.Refresh(false, false);
            }
        }
    }
}

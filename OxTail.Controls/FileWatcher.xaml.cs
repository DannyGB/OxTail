﻿/*****************************************************************
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
        //private object _lock = new object();
        private Encoding _tailEncoding = Encoding.Default;
        private DateTime _dateLastTime = DateTime.MinValue;
        private int _chunkSize = 16384;
        private bool _followTail = true;
        private string _filename;
        private DoWorkEventArgs _doWorkEventArgs;
        private string _newlineCharacters = null;
        private long _currentLength = 0;
        private long _previousLength = 0;
        private long _previousStartLine = 0;
        private List<string> _readLines = new List<string>();
        private NewlineDetectionMode _newlineDetectionMode;
        private static IStringPatternMatching _patternMatching = StringPatternMatching.CreatePatternMatching();
        private ScrollViewer _scrollViewer = null;
        private int _interval = 1000;
        private int _linesInFile = 0;
        public List<HighlightedItem> SelectedItem { get; private set; }
        private string SearchText { get; set; }
        private int LastSearchIndex { get; set; }

        private bool IsSeaching
        {
            get
            {
                if (!string.IsNullOrEmpty(this.SearchText))
                {
                    return true;
                }

                return false;
            }
        }


        public long CurrentLength
        {
            get
            {
                lock (this)
                {
                    return this._currentLength;
                }
            }
            set
            {
                lock (this)
                {
                    this._previousLength = this._currentLength;
                    this._currentLength = value;
                }
            }
        }

        public long PreviousLength
        {
            get {
                lock (this)
                {
                    return this._previousLength;  
                }
            }
            set {
                lock (this)
                {
                    this._previousLength = value;  
                }
            }
        }

        public int LinesInFile
        {
            get
            {
                lock (this)
                {
                    return this._linesInFile; 
                }
            }
            set
            {
                lock (this)
                {
                    this._linesInFile = value; 
                }
            }
        }

        public long StartLine
        {
            get;
            set;
            //get
            //{
            //    ScrollViewer vw = this.ScrollViewer;
            //    if (vw != null)
            //        return (long)this.ScrollViewer.VerticalOffset + this.HorizontalScrollbarVisibilityOffset;
            //    else
            //        return 0;
            //}
        }

        private int HorizontalScrollbarVisibilityOffset
        {
            get { return 0; } // (this.ScrollViewer.ScrollableWidth == 0 ? 0 : 1); }
        }

        public ScrollViewer ScrollViewer
        {
            get
            {
                // If the tab is not visible then the GetVisualChildren call returns 0
                // So whilst opening multiple tabs most tabs are not visible during the load.
                if (this._scrollViewer == null)
                {
                    List<ScrollViewer> scrollviewers = this.colourfulListView.GetVisualChildren<ScrollViewer>();
                    if (scrollviewers != null && scrollviewers.Count > 0)
                    {
                        foreach (ScrollViewer scrollviewer in scrollviewers)
                        {
                            if (scrollviewer.IsVisible)
                            {
                                this._scrollViewer = scrollviewer;
                                break;
                            }
                        }
                    }
                }
                return this._scrollViewer;
            }
        }

        public NewlineDetectionMode NewlineDetectionMode
        {
            get { return this._newlineDetectionMode; }
            set
            {
                this._newlineDetectionMode = value;
                this.SetNewlineCharacters();
                this.ScanLinesInFile();
                this.ShowLines();
            }
        }

        private void ShowLines()
        {
            ReadLines();
            Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(this.Update));
        }

        private StreamReader OpenFile()
        {
            return new StreamReader(new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize), this._tailEncoding, true, this._chunkSize);
        }

        private void SetNewlineCharacters()
        {
            switch (this._newlineDetectionMode)
            {
                case NewlineDetectionMode.Auto:
                    this.AutoDetectNewLineCharacters();
                    break;
                case NewlineDetectionMode.Windows:
                    this._newlineCharacters = Constants.WINDOWS_NEWLINE;
                    break;
                case NewlineDetectionMode.Unix:
                    this._newlineCharacters = Constants.UNIX_NEWLINE;
                    break;
                case NewlineDetectionMode.Mac:
                    this._newlineCharacters = Constants.MAC_NEWLINE;
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
            this._newlineCharacters = null;
            using (StreamReader streamReader = this.OpenFile())
            {
                // read first line of file
                this.PositionFilePointerToOffset(streamReader, 0);
                string line = streamReader.ReadLine();
                if (line != null)
                {
                    this.PositionFilePointerToOffset(streamReader, line.Length);
                    streamReader.Read(chars, 0, 2);
                    this._newlineCharacters = new string(chars);
                    // if the second character is not line feed ("\n") then it must have been a single character newline - either unix ("\n") or mac ("\r")
                    if (this._newlineCharacters.Substring(1, 1) != Constants.UNIX_NEWLINE)
                    {
                        this._newlineCharacters = this._newlineCharacters.Substring(0, 1);
                    }
                }
            }
        }

        // Gets a value that indicates the number of visible lines 
        public int VisibleLines
        {
            get
            {
                ScrollViewer vw = this.ScrollViewer;
                if (vw != null)
                    return (int)this.ScrollViewer.ViewportHeight - this.HorizontalScrollbarVisibilityOffset;
                else
                    return 0;
            }
        }

        public ItemCollection Lines
        {
            get { return this.colourfulListView.Items; }
        }

        // Determines whether this file watcher follows the tail
        public bool IsFollowTail
        {
            get { return this._followTail; }
            set { this._followTail = value; }
        }

        private bool CanHighlightLine(int lineIndex)
        {
            return lineIndex == this.LastSearchIndex && !string.IsNullOrEmpty(this.SearchText);
        }

        private string CreateHighlightItemString(int lineIndex, int index)
        {
            return lineIndex.ToString() + Constants.LINE_NUMBER_DIVIDER + this._readLines[index];
        }

        /// <summary>
        /// Updates the current display
        /// </summary>
        private void Update()
        {
            lock (this)
            {
                // highlight and add/update items to the listview
                for (int i = 0; i < this._readLines.Count; i++)
                {
                    HighlightedItem item = null;
                    int lineIndex = i + (int)this.StartLine;

                    if (this.IsSeaching)
                    {
                        if (this.CanHighlightLine(lineIndex))
                        {
                            item = this.Highlight(this.CreateHighlightItemString(lineIndex, i));
                        }
                        else
                        {
                            item = new HighlightedItem(this.CreateHighlightItemString(lineIndex, i), Constants.DEFAULT_FORECOLOUR, Constants.DEFAULT_BACKCOLOUR);
                        }
                    }
                    else
                    {
                        item = this.Highlight(this.CreateHighlightItemString(lineIndex, i));
                    }

                    if (this.Lines.Count <= i)
                    {
                        this.Lines.Add(item);
                    }
                    else
                    {
                        this.Lines[lineIndex] = item;
                    }
                }
            }

#if DEBUG
            if (this.Lines.Count > this.StartLine + this.VisibleLines)
            {

                this.Lines[(int)this.StartLine + this.VisibleLines] = new TextBlock(new Run(string.Format("{0}: visible lines = {1}", (int)this.StartLine + this.VisibleLines, this.VisibleLines)));
                Console.WriteLine("line {0} was the last visible line of {1} apparently ", (int)this.StartLine + this.VisibleLines, this.VisibleLines);
            }
#endif

            // update the status bar
            this.ReportProgress(0, String.Empty, false, System.Windows.Visibility.Hidden);
            this.textBlockStartLine.Text = this.StartLine.ToString();
            this.textBlockLinesInFile.Text = this.LinesInFile.ToString();
            this.textBlockVisibleLines.Text = this.VisibleLines.ToString();
        }

        private void PadOutListView()
        {
            // make the number of items match the number of lines
            if (this.LinesInFile > this.Lines.Count)
            {
                int extra = this.LinesInFile - this.Lines.Count;
                for (int i = 0; i < extra; i++)
                {
                    this.Lines.Add(string.Empty);
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
        }

        public IEnumerable<HighlightItem> FindFirstHighlightByText(IEnumerable<HighlightItem> coll, string text)
        {
            if (this.IsSeaching)
            {
                // "Special" Search item
                HighlightItem special = new HighlightItem(this.SearchText, Constants.DEFAULT_FORECOLOUR, Constants.DEFAULT_BACKCOLOUR);
                special.BorderColour = Constants.DEFAULT_BORDERCOLOUR;

                if (!string.IsNullOrEmpty(special.Pattern) && !string.IsNullOrEmpty(text))
                {
                    if (text == special.Pattern || _patternMatching.MatchPattern(text, special.Pattern))
                    {
                        yield return special;
                    }

                    // if a search is in progress, disable all other highlighting
                    yield break;
                }
            }
            else
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
        }

        private HighlightedItem Highlight(string text)
        {
            HighlightedItem highlighted = new HighlightedItem();
            highlighted.Text = text;

            highlighted.ForeColour = Constants.DEFAULT_FORECOLOUR;
            highlighted.BackColour = Constants.DEFAULT_BACKCOLOUR;

            foreach (HighlightItem highlight in FindFirstHighlightByText(this.Patterns, text))
            {
                if (highlight.BorderColour != Constants.DEFAULT_NULL_COLOUR)
                {
                    highlighted.ForeColour = Constants.DEFAULT_FORECOLOUR;
                    highlighted.BackColour = Constants.DEFAULT_BACKCOLOUR;
                    highlighted.BorderColour = highlight.BorderColour;
                }
                else
                {
                    highlighted.ForeColour = highlight.ForeColour;
                    highlighted.BackColour = highlight.BackColour;
                    highlighted.BorderColour = Constants.DEFAULT_NULL_COLOUR;
                }
                
                break; // use the first one we come across in the list - items at the top are most important
            }

            return highlighted;
        }

        private SeekOrigin CalculateClosestSeekOrigin(StreamReader streamReader, long target, out long smallestOffset)
        {
            long beginOffset = target;
            // we want the following to be negative for use in conjunction with SeekOrigin.Current or SeekOrigin.End
            long currentOffset = target - streamReader.BaseStream.Position;
            long endOffset = target - this.CurrentLength;

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
            lock (this)
            {
                this._readLines = new List<string>(this.VisibleLines);
                long offset = 0;
                if (!this._LineOffsets.TryGetValue(this.StartLine, out offset))
                {
                    ReportProgress(0, LanguageHelper.GetLocalisedText((Application.Current as IApplication), "lineNumberError") + this.StartLine.ToString(), true, System.Windows.Visibility.Hidden);
                    return;
                }

                using (StreamReader streamReader = this.OpenFile())
                {
                    this.PositionFilePointerToOffset(streamReader, offset);

                    for (int i = 0; i < this.VisibleLines; i++)
                    {
                        string line = streamReader.ReadLine();
                        offset += line.Length + this._newlineCharacters.Length;
                        if (offset >= this.CurrentLength)
                        {
                            break;
                        }
                        if (!string.IsNullOrEmpty(line))
                        {
                            line = line.TrimEnd(Constants.NULL_TERMINATOR).Replace(Constants.MAC_NEWLINE, Constants.CARRIAGE_RETURN).Replace(Constants.UNIX_NEWLINE, Constants.LINE_FEED); // in case of incorrectly selected line end;
                        }
                        this._readLines.Add(line);
                    }
                } 
            }

#if DEBUG
            Console.WriteLine("Read {2} of {3} Lines {0} - {1}", this.StartLine, this.StartLine + this.VisibleLines, this._readLines.Count, this.VisibleLines);
#endif
        }

        private void PositionFilePointerToOffset(StreamReader streamReader, long target)
        {
            streamReader.DiscardBufferedData();
            long smallestOffset;
            SeekOrigin seekOrigin = this.CalculateClosestSeekOrigin(streamReader, target, out smallestOffset);
            streamReader.BaseStream.Seek(smallestOffset, seekOrigin);
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
            Binding bindingFollowTail = new Binding("IsFollowTail");
            bindingFollowTail.Source = this;
            this.checkBoxFollowTail.SetBinding(CheckBox.IsCheckedProperty, bindingFollowTail);

            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bw_RunWorkerCompleted);

            this.SelectedItem = new List<HighlightedItem>();
        }

        void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            return;
        }

        public void Start(string filename)
        {
            this._filename = filename;
            if (!this._bw.IsBusy)
            {
                this._bw.RunWorkerAsync();
            }
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
            //lock (this._fileReadLock)
            {
                // see if file has changed
                FileInfo fileInfo = new FileInfo(this._filename);
                if ((this.CurrentLength != fileInfo.Length || this._dateLastTime.Ticks != Math.Max(fileInfo.LastWriteTime.Ticks, fileInfo.CreationTime.Ticks)))
                {
                    this.CurrentLength = fileInfo.Length;
                    this._dateLastTime = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));
                    this.ScanLinesInFile();
                    this.ShowLines();
                    if (this.IsFollowTail)
                    {
                        this.ScrollToEnd();
                    }
                    this.OnFileChanged();
                }
            }
        }

        private void ScrollToEnd()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Loaded, (Action)(() => 
            {
                lock (this)
                {
#if DEBUG
                    Console.WriteLine("Scroll to end");
#endif

                    if (this.ScrollViewer != null)
                    {
                        this.ScrollViewer.ScrollToVerticalOffset(this.LinesInFile + 1);
                    }
                }
            })); // this will change the vertical offset which will induce an update
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

        /// <summary>
        /// Counts the number of lines in the file and stores the line offsets 
        /// </summary>
        /// <returns>A boolean value that indicates whether the scan completed successfully.</returns>
        private bool ScanLinesInFile()
        {
            long start = 0;
            long offset = 0;
            lock (this)
            {
                using (StreamReader streamReader = this.OpenFile())
                {
                    if (this.PreviousLength == 0)
                    {
                        this._LineOffsets = new Dictionary<long, long>();
                        this.LinesInFile = 0;
                    }
                    if (this._newlineCharacters != Constants.WINDOWS_NEWLINE && this._newlineCharacters != Constants.UNIX_NEWLINE && this._newlineCharacters != Constants.MAC_NEWLINE)
                    {
                        this.SetNewlineCharacters();
                    }
                    this._LineOffsets.TryGetValue(this.LinesInFile, out start);
                    this.PositionFilePointerToOffset(streamReader, start); // only count from previously loaded last line offset
                    offset = streamReader.BaseStream.Position;
                    while (offset < this.CurrentLength)
                    {
                        // honour cancellation request
                        if (this.CancellationPending())
                        {
                            return false;
                        }
                        // record the offset for the current line
                        if (this._LineOffsets.ContainsKey(this.LinesInFile))
                        {
                            // do nothing for now
                        }
                        else
                        {
                            this._LineOffsets.Add(this.LinesInFile, offset);
                        }
                        // skip lines and keep track of the position of the end of the line by measuring the length of the line and adding on the length of our line ending
                        // n.b. StreamReader.Readline reads a block of data from the underlying filestream so does not reflect the position of the end of each line
                        string line = streamReader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        offset += line.Length + this._newlineCharacters.Length;
                        if (offset >= this.CurrentLength)
                        {
                            break;
                        }
                        this.LinesInFile++;

                        if (this.LinesInFile % 1000 == 0)
                        {
                            ReportProgress((int)(offset * 100 / this.CurrentLength), string.Format("counting lines in file: {0}", this.LinesInFile), false, System.Windows.Visibility.Visible);
                        }
                    }
                } 
            }

#if DEBUG
            Console.WriteLine("scanned {0} lines - {1} offsets recorded - final offset {2} - current length {3}", this.LinesInFile, this._LineOffsets.Count, offset, this.CurrentLength);
#endif
            this.Dispatcher.Invoke(DispatcherPriority.Loaded, (Action)(() => { this.PadOutListView(); }));
            ReportProgress(100, string.Format(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "lineCount"), this.LinesInFile), false, System.Windows.Visibility.Hidden);
            return true;
        }

        private Dictionary<long, long> _LineOffsets;

        private bool CancellationPending()
        {
            if (this._bw.CancellationPending)
            {
                this._doWorkEventArgs.Cancel = true;
            }
            return this._bw.CancellationPending;
        }

        private void ReportProgress(int progressBarPercentage, string status, bool progressBarIndeterminate, System.Windows.Visibility progressBarVisibility)
        {
            this.Dispatcher.Invoke(new StatusNotificationDelegate(this.ShowStatus), DispatcherPriority.Normal, progressBarPercentage, new FileWatcherProgressChangedUserState(status, progressBarIndeterminate, progressBarVisibility));
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
                ReportProgress(0, LanguageHelper.GetLocalisedText((Application.Current as IApplication), "waitingForWatcher"), true, System.Windows.Visibility.Visible);
                Thread.Sleep(100);
            }
            return;
        }

        public void Dispose()
        {
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.PreviousLength = 0;
            this.ScanLinesInFile();
            this.ShowLines();
        }

        private HighlightCollection<HighlightItem> _patterns;

        public HighlightCollection<HighlightItem> Patterns
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

        private void colourfulListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.StartLine = (long)this.ScrollViewer.VerticalOffset + this.HorizontalScrollbarVisibilityOffset;

#if DEBUG
            Console.WriteLine("ScrollChanged: {0} {1}", e.VerticalChange, e.ViewportHeightChange);
#endif

            if (e.VerticalChange != 0)
            {
#if DEBUG
                Console.WriteLine("Vertical offset changed {0}", e.VerticalChange);
#endif
                this.ShowLines();
            }
            //else if (e.ViewportHeightChange != 0)
            //{
            //    Console.WriteLine("Viewport height changed {0}", e.ViewportHeightChange);
            //    if (this.IsFollowTail)
            //    {
            //        this.ScrollToEnd();
            //    }
            //    else
            //    {
            //        this.ShowLines();
            //    }
            //}

            this._previousStartLine = this.StartLine;
        }

        private void colourfulListView_Scroll(object sender, ScrollEventArgs e)
        {
#if DEBUG
            Console.WriteLine("Scroll: {0}", e.ReflectToString());
#endif
        }

        private void colourfulListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem.Clear();

            foreach (var item in e.AddedItems)
            {
                this.SelectedItem.Add((HighlightedItem)item);
            }
        }

        void Patterns_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.Update();
            this.ShowLines();
            this.ScrollViewer.ScrollToVerticalOffset(this.StartLine);
        }  

        internal double Find(string searchCriteria, double lastFindOffset)
        {
            double d = FindText(searchCriteria, lastFindOffset);
            this.ShowLines();
            this.ScrollViewer.ScrollToVerticalOffset(d);

            return d;
        }

        private double FindText(string text, double lastFindOffset)
        {
            this.SearchText = text;

            if(!File.Exists(this._filename))
            {
                throw new Exception(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "fileNoLongerExistsOnDisk"));
            }

            FileStream fs = File.Open(this._filename, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs);
            int i = 0;
            try
            {
                while (read.Peek() >= 0)
                {
                    string line = read.ReadLine();

                    if (i > lastFindOffset)
                    {

                        if (_patternMatching.MatchPattern(line, text))
                        {
                            this.StartLine = (long)i;
                            break;
                        }
                    }

                    i++;
                }

                // If we reached the end of the stream reset the last search position
                if (read.EndOfStream)
                {
                    i = 0;
                }

                LastSearchIndex = i;

            }
            finally
            {
                read.Close();
                read.Dispose();
                fs.Close();
                fs.Dispose();
            }

            return i;
        }

        internal void ResetSearchCriteria()
        {
            this.SearchText = string.Empty;
            this.Update();
        }
    }
}

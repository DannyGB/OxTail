using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using OxTail.Helpers;
using System.ComponentModel;
using System.Threading;
using OxTailLogic.PatternMatching;
using System.Windows.Threading;

namespace OxTail.Controls
{
    /// <summary>
    /// Interaction logic for FileWatcher.xaml
    /// </summary>
    public partial class FileWatcher : UserControl, IDisposable
    {
        private StreamReader _streamReader = null;
        private FileStream _fileStream = null;
        private BackgroundWorker _bw = null;
        private volatile bool _loading = false;
        private long _startLine = 0;
        private long _numberOfLinesInFile = 0;
        private int _visibleLines = 20;
        private Encoding _encoding = Encoding.UTF8;
        private DateTime _dateLastTime = DateTime.MinValue;
        int _chunkSize = 32768;
        private bool _followTail = true;
        private FileInfo _fileInfo;
        private DoWorkEventArgs _doWorkEventArgs;
        private string _newline = "\n";
        private long _currentLength = 0;
        private long _offset = 0;
        private List<string> _readLines = new List<string>();


        public Encoding Encoding
        {
            get { return this._encoding; }
            set { this._encoding = value; }
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
            set
            {
                this._startLine = value;
                if (this._startLine > -1)
                {
                    ReadLines();
                    Dispatcher.Invoke(DispatcherPriority.Render, new Action(this.ShowLines));
                }
            }
        }

        private void ShowLines()
        {
            //this.Lines.Clear();
            for (int i = 0; i < this._readLines.Count; i++)
            {
                TextBlock textBlock = new TextBlock(new Run(this._readLines[i]));
                if (this.Lines.Count <= i)
                {
                    this.Lines.Add(textBlock);
                }
                else
                {
                    this.Lines[i] = textBlock;
                }
                //this.ReportProgress((int)(i * 100 / this.VisibleLines), "added line: " + this._readLines[i], false, System.Windows.Visibility.Visible);
            }
            this.ReportProgress(0, string.Format("Showing lines {0}-{1} of {2}", this._startLine, this._startLine + this._visibleLines, this._numberOfLinesInFile), false, System.Windows.Visibility.Hidden);
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
            //this._streamReader.DiscardBufferedData();
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
            int linesEncountered = 1;
            StringBuilder firstLineOfPreviousChunk = new StringBuilder();
            StringBuilder chunkAsString = new StringBuilder();

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
                chunkAsString.Append(firstLineOfPreviousChunk);

                string[] lineArray = chunkAsString.ToString().Split(new string[] { this._newline }, StringSplitOptions.None);
                if (lineArray.Length > 0)
                {
                    if (this._readLines.Count == 0)
                    {
                        this._readLines.Add(string.Empty);
                    }
                    this._readLines[0] = lineArray[lineArray.Length - 1] + this._readLines[0];
                }
                for (int i = lineArray.Length - 2; i >= 0; i--)
                {
                    this._readLines.Insert(0, lineArray[i]);
                }

                linesEncountered = this._readLines.Count;
                ReportProgress((int)(Math.Min(linesEncountered, linesToRead) * 100 / linesToRead), string.Format("Skipped {0} of {1} lines backwards", linesEncountered, linesToRead), false, System.Windows.Visibility.Visible);
                this._offset += indexIncrement;
                if (linesEncountered > linesToRead)
                {
                    break;
                }
                // rewind the pointer back to where it started on the most recent read operation
                this.PositionFilePointerToOffset(this._offset);
                //if (offset != this._fileStream.Position)
                //{
                //    ReportProgress(0, string.Format("The FileWatcher's streamReader position ({0}) went out of phase after skipping {1} lines backwards: expected offset {2}", this._fileStream.Position, linesEncountered, offset), false, System.Windows.Visibility.Hidden);
                //    return -1;
                //}
                // retain contents of most recent trunk up to the first newline
                // this should cover the case where lines are longer than chunksize
                // and also where a newline might be split assunder by the chunk (i.e the \r is at the end of one chunk and the \n is at the beginning of another)
                // probably: this won't work where the file is written by OSes other than windows where newline is something other than \r\n
                //if (lineArray.Length > 0)
                //{
                //    int firstNewLine = chunkAsString.ToString().IndexOf(this._newline);
                //    firstLineOfPreviousChunk.Clear();
                //    firstLineOfPreviousChunk.Append(chunkAsString.ToString().Substring(0, firstNewLine));
                //}
                //else
                //{
                //    firstLineOfPreviousChunk.Append(chunkAsString);
                //}
            } while (linesEncountered <= linesToRead && this._offset > 0);

            // we will almost definitely have read more than we needed to unless by some miracle the nth newline is exactly in position 0 of the most recent chunk
            int linesToTrim = Math.Max((int)(linesEncountered - linesToRead), 1);
            if (linesToTrim > 0)
            {
                this._readLines.RemoveRange(0, linesToTrim);
            }
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
                this._readLines.Add(this._streamReader.ReadLine());
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
            this._fileInfo = new FileInfo(filename);
            this._fileStream = new FileStream(filename,FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize);
            this._streamReader = new StreamReader(this._fileStream, this._encoding, true, this._chunkSize);
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
                if (!this._loading)
                {
                    //this._streamReader.DiscardBufferedData();
                    this._fileInfo.Refresh();
                    if (this.FollowTail && (this._currentLength != this._fileStream.Length || this._dateLastTime.Ticks != Math.Max(this._fileInfo.LastWriteTime.Ticks, this._fileInfo.CreationTime.Ticks)))
                    {
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.ReadNewTextFromFile));
                        this.ReadNewTextFromFile(this._fileStream.Length);
                    }
                }
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
        private void ReadNewTextFromFile(long length)
        {
            this._currentLength = length;
            this._dateLastTime = new DateTime(Math.Max(this._fileInfo.CreationTime.Ticks, this._fileInfo.LastWriteTime.Ticks));
            this.StartLine = this.CalculateStartLine();
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
            long pos = this._fileStream.Position; // save position
            this._numberOfLinesInFile = 0;
            //this._fileStream.Position = 0; // go to beginning of file
            this._fileStream.Seek(0, SeekOrigin.Begin);
            while (this._fileStream.Position < this._currentLength)
            {
                // honour cancellation request
                if (this.CancellationPending())
                {
                    return -1;
                }
                this._streamReader.DiscardBufferedData();
                this._streamReader.ReadLine();
                this._numberOfLinesInFile++;
                if (this._numberOfLinesInFile % 1000 == 0)
                {
                    ReportProgress((int)(this._fileStream.Position * 100 / this._currentLength), string.Format("counting lines in file: {0}", this._numberOfLinesInFile), false, System.Windows.Visibility.Visible);
                }
            }
            this._fileStream.Position = pos; // back to saved position
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

    }
}

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
        private long _linesInFile = 0;
        private Encoding _encoding = Encoding.Default;
        private long _positionLastTime = 0;
        int _chunkSize = 512;

        public Encoding Encoding
        {
            get { return this._encoding; }
            set { this._encoding = value; }
        }

        private int _visibleLines = 20;
        public int VisibleLines
        {
            get { return this._visibleLines; }
            set { this._visibleLines = value; }
        }

        private bool _followTail = true;
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
                this.ShowLines();
            }
        }

        public void ShowLines()
        {
            try
            {
                long target = StartLineFileStreamOffset();
                long smallestOffset;
                SeekOrigin seekOrigin = this.CalculateClosestSeekOrigin(target, out smallestOffset);
                this._streamReader.DiscardBufferedData();
                this._fileStream.Seek(smallestOffset, seekOrigin);
                this.Lines.Clear();
                for (int i = 0; i < this.VisibleLines; i++)
                {
                    string line = this._streamReader.ReadLine();
                    this.Lines.Add(new TextBlock(new Run(line)));
                }
            }
            catch (FileWatcherSeekOutOfPhaseException ex)
            {
                this.ReportProgress(0, ex.Message, false, System.Windows.Visibility.Hidden);
                // ensure not to try again
                this._fileStream.Position = this._fileStream.Length;
            }
            finally
            {
                this._positionLastTime = Math.Min(this._fileStream.Position, this._fileStream.Length);
            }
        }

        private SeekOrigin CalculateClosestSeekOrigin(long target, out long smallestOffset)
        {
            long beginOffset = target;
            // we want the following be negative for use in conjunction with SeekOrigin.Current or SeekOrigin.End
            long currentOffset = target - this._fileStream.Position;
            long endOffset = target - this._fileStream.Length;

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

        private long StartLineFileStreamOffset()
        {
            long offset = 0;
            // educated guess at quickest direction to go - position the cursor at the beginning if our first line is in the first "half" (variable line lengths!) of the file
            if (this._startLine < (this._linesInFile / 2))
            {
                offset = SkipLinesForwards();
            }
            else
            {
                offset = SkipLinesBackwards();
            }
            return offset;

        }

        private long SkipLinesBackwards()
        {
            int indexIncrement = -_chunkSize;
            int chunksRead = 0;
            char[] chunk = new char[_chunkSize];
            long linesToRead;
            long linesEncountered = 0;
            long offset;
            string firstLineOfPreviousChunk = string.Empty;
            string chunkAsString = string.Empty;

            offset = (int)this._fileStream.Length;
            linesToRead = this._linesInFile - this._startLine;

            // go directly to the end of the file
            this._fileStream.Seek(0, SeekOrigin.End);

            do
            {
                // shorten chunk to remaining file if necessary
                if (this._fileStream.Position + indexIncrement < 0)
                {
                    indexIncrement = -(int)this._fileStream.Position;
                }
                // set pointer to beginning of next chunk
                this._streamReader.DiscardBufferedData();
                this._fileStream.Seek(indexIncrement, SeekOrigin.Current);
                // read the block 
                this._streamReader.Read(chunk, 0, _chunkSize);
                chunksRead++;
                chunkAsString = new String(chunk);
                linesEncountered += CountLines(chunkAsString);
                ReportProgress((int)(Math.Min(linesEncountered, linesToRead) / linesToRead * 100), string.Format("Skipped {0} of {1} lines backwards", linesEncountered, linesToRead), false, System.Windows.Visibility.Visible);
                offset += indexIncrement;
                if (linesEncountered >= linesToRead)
                {
                    break;
                }
                // rewind the pointer back to where it started on the most recent read operation
                this._streamReader.DiscardBufferedData();
                this._fileStream.Seek(indexIncrement, SeekOrigin.Current);
                if (offset != this._fileStream.Position)
                {
                    throw new FileWatcherSeekOutOfPhaseException(string.Format("The FileWatcher's streamReader position ({0}) went out of phase after skipping {1} lines backwards: expected offset {2}", this._fileStream.Position, linesEncountered, offset));
                }
                // retain contents of most recent trunk up to the first newline
                // this should cover the case where lines are longer than chunksize
                // and also where a newline might be split assunder by the chunk (i.e the \r is at the end of one chunk and the \n is at the beginning of another)
                // probably: this won't work where the file is written by OSes other then windows where newline is something other than \r\n
                int firstNewLine = chunkAsString.IndexOf(Environment.NewLine);
                if (firstNewLine > -1)
                {
                    firstLineOfPreviousChunk += chunkAsString.Substring(0, firstNewLine);
                }
                else
                {
                    firstLineOfPreviousChunk += chunkAsString;
                }
            } while (linesEncountered < linesToRead && offset > 0);

            // we will almost definitely have read more than we needed to unless by some miracle the nth newline is exactly in position 0 of the most recent chunk
            int linesToTrim = Math.Max((int)(linesEncountered - linesToRead - 1), 0);

            int adjustment = 0;
            for (int i = 0; i < linesToTrim; i++)
            {
                adjustment += chunkAsString.IndexOf(Environment.NewLine, adjustment) - adjustment + Environment.NewLine.Length;
            }

            return offset + adjustment;
        }

        private long SkipLinesForwards()
        {
            this._fileStream.Seek(0, SeekOrigin.Begin);
            long offset = 0;
            for (int i = 0; i < this._startLine; i++)
            {
                string line = this._streamReader.ReadLine();
                //this._streamReader.DiscardBufferedData();
                offset += this.Encoding.GetByteCount(line) + Environment.NewLine.Length;
            }
            return offset;
        }

        private long CountLines(string stringBlock)
        {
            return stringBlock.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).LongLength;
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
        }

        public void Start(string filename)
        {
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
            while (!this._bw.CancellationPending)
            {
                if (!this._loading)
                {
                    //this._streamReader.DiscardBufferedData();
                    // reset to beginning if file is smaller than previously 
                    if (this._positionLastTime > this._fileStream.Length)
                    {
                        this._fileStream.Position = 0;
                    }
                    if (this.FollowTail && this._positionLastTime < this._fileStream.Length)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.ReadNewTextFromFile(); }));
                    }
                }
                Thread.Sleep(this.Interval);
            }
        }

        /// <summary>
        /// Reads only the latest additional text that has been appended to the file
        /// </summary>
        private void ReadNewTextFromFile()
        {
            //IStringPatternMatching patternMatch = new StringPatternMatching();

            // our stream position will be the point where it last read up to
            // we only want to read from that position onwards to the latest end of the document
            this._loading = true;
            this.CountLinesInFile();
            this.StartLine = this.CalculateStartLine();
            this._loading = false;
            this.RaiseFileChangedEvent();
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

        private void CountLinesInFile()
        {
            long pos = this._fileStream.Position; // save position
            this._linesInFile = 0;
            this._fileStream.Position = 0; // go to beginning of file
            while (!this._streamReader.EndOfStream)
            {
                this._streamReader.ReadLine();
                this._linesInFile++;
                if (this._linesInFile % 100 == 0)
                {
                    ReportProgress(0, string.Format("counting lines in file: {0}", this._linesInFile), true, System.Windows.Visibility.Visible);
                }
            }
            this._streamReader.DiscardBufferedData();
            this._fileStream.Position = pos; // back to saved position
            ReportProgress(0, string.Format("counted {0} lines", this._linesInFile), false, System.Windows.Visibility.Hidden);
        }

        private long CalculateStartLine()
        {
            long newStartLine;
            if (this.FollowTail)
            {
                newStartLine = Math.Max(this._linesInFile - this.VisibleLines, 0);
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
            this.Dispatcher.Invoke(new StatusNotificationDelegate(ShowStatus), DispatcherPriority.Render, progressBarPercentage, new FileWatcherProgressChangedUserState(status, progressBarIndeterminate, progressBarVisibility));
        }

        internal void Stop()
        {
            if (this._bw.IsBusy)
            {
                this._bw.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (this._streamReader != null)
            {
                this._streamReader.Close();
                this._streamReader.Dispose();
            }
        }

    }
}

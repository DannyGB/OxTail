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
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using OxTailLogic.Helpers;
    using System.Windows.Controls;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Threading;
    using System.Windows;
    using OxTailLogic.PatternMatching;
    using System.Windows.Media.Imaging;
    using System.Windows.Documents;

    /// <summary>
    /// An extension of the CloseableTabItem control that views a file.
    /// </summary>
    public class OxTailFileWatcher : CloseableTabItem, IDisposable
    {
        private StreamReader _streamReader = null;
        private ColourfulListView _viewer = null;
        private BackgroundWorker _bw = null;
        private Grid _grid = null;
        private volatile bool _loading = false;
        private long _startLine = 0;
        private long _linesInFile = 0;
        private Encoding _encoding = Encoding.Default;
        private long _positionLastTime = 0;

        public Encoding Encoding 
        {
            get { return this._encoding;}
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
            long target = StartLineFileStreamOffset();
            long smallestOffset;
            SeekOrigin seekOrigin = this.CalculateClosestSeekOrigin(target, out smallestOffset);
            this._streamReader.DiscardBufferedData();
            this._streamReader.BaseStream.Seek(smallestOffset, seekOrigin);
            this._viewer.Items.Clear();
            for (int i = 0; i < this.VisibleLines; i++)
            {
                string line = this._streamReader.ReadLine();
                this._viewer.Items.Add(new TextBlock(new Run(line)));
            }
            this._positionLastTime = Math.Min(this._streamReader.BaseStream.Position, this._streamReader.BaseStream.Length);
        }

        private SeekOrigin CalculateClosestSeekOrigin(long target, out long smallestOffset)
        {
            long beginOffset = target;
            // we want the following be negative for use in conjunction with SeekOrigin.Current or SeekOrigin.End
            long currentOffset = target - this._streamReader.BaseStream.Position;
            long endOffset = target - this._streamReader.BaseStream.Length;

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
                offset=SkipLinesForwards();
            }
            else
            {
                offset=SkipLinesBackwards();
            }
            return offset;
            
        }

        private long SkipLinesBackwards()
        {
            int chunkSize = 512;
            int indexIncrement = -chunkSize;
            int chunksRead = 0;
            char[] chunk = new char[chunkSize];
            long linesToRead;
            long linesEncountered = 0;
            long offset;
            string firstLineOfPreviousChunk = string.Empty;
            string chunkAsString = string.Empty;

            offset = (int)this._streamReader.BaseStream.Length;
            linesToRead = this._linesInFile - this._startLine;

            // go directly to the end of the file
            this._streamReader.BaseStream.Seek(0, SeekOrigin.End);

            do
            {
                // shorten chunk to remaining file if necessary
                if (this._streamReader.BaseStream.Position + indexIncrement < 0)
                {
                    indexIncrement = -(int)this._streamReader.BaseStream.Position;
                }
                // set pointer to beginning of next chunk
                this._streamReader.BaseStream.Seek(indexIncrement, SeekOrigin.Current);
                // read the block 
                this._streamReader.Read(chunk, 0, chunkSize);
                chunksRead++;
                chunkAsString = new String(chunk);
                linesEncountered += CountLines(chunkAsString);
                offset += indexIncrement;
                if (linesEncountered >= linesToRead)
                {
                    break;
                }
                // rewind the pointer back to where it started on the most recent read operation
                this._streamReader.BaseStream.Seek(indexIncrement, SeekOrigin.Current);
                this._streamReader.DiscardBufferedData();
                if (offset != this._streamReader.BaseStream.Position)
                {
                    throw new Exception("The reverse seek position went out of phase");
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
            int linesToTrim = Math.Max((int)(linesEncountered - linesToRead -1), 0);

            int adjustment = 0;
            for (int i = 0; i < linesToTrim; i++)
            {
                adjustment += chunkAsString.IndexOf(Environment.NewLine, adjustment) - adjustment + Environment.NewLine.Length;
            }

            return offset + adjustment;
        }

        private long SkipLinesForwards()
        {
            this._streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
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
            return stringBlock.Split(new string[] {Environment.NewLine}, StringSplitOptions.None).LongLength;
        }
        
        int Interval { get; set; }

        public OxTailFileWatcher()
            : base()
        {
        }

        /// <summary>
        /// Creates an OxTailFileViewer instance and starts viewing the specified filename.
        /// </summary>
        /// <param name="filename">The full file path of the file to view.</param>
        public OxTailFileWatcher(string filename)
        {
            this.Header = Path.GetFileName(filename);
            this.ToolTip = filename;
            this.Uid = filename;
            this.Interval = 1000;
            this._streamReader = new StreamReader(FileHelper.OpenFile(filename));
            this._grid = new Grid();
            this.Visibility = System.Windows.Visibility.Visible;
            this.AddChild(this._grid);
            this._viewer = new ColourfulListView();
            this._viewer.Visibility = System.Windows.Visibility.Visible;
            this._grid.Children.Add(this._viewer);
            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerAsync();
            this.GotFocus += new RoutedEventHandler(OxTailFileViewer_GotFocus);
        }

        void OxTailFileViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            Image img = base.GetTemplateChild("PART_Icon") as Image;
            if (img != null)
            {
                img.Source = null;
            }
        }

        void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this._bw.CancellationPending)
            {
                if (!this._loading)
                {
                    //this._streamReader.DiscardBufferedData();
                    // reset to beginning if file is smaller than previously 
                    if (this._positionLastTime > this._streamReader.BaseStream.Length)
                    {
                        this._streamReader.BaseStream.Position = 0;
                    }
                    if (this.FollowTail && this._streamReader.BaseStream.Position < this._streamReader.BaseStream.Length)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.ReadNewTextFromFile(); }));
                    }
                }
                Thread.Sleep(this.Interval);
            }
        }

        public override void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this._bw.CancelAsync();
            base.closeButton_Click(sender, e);
        }

        /// <summary>
        /// Reads only the latest additional text that has been appended to the file
        /// </summary>
        private void ReadNewTextFromFile()
        {
            IStringPatternMatching patternMatch = new StringPatternMatching();

            // our stream position will be the point where it last read up to
            // we only want to read from that position onwards to the latest end of the document
            this._loading = true;
            this.CountLinesInFile();
            this.StartLine = this.CalculateStartLine();
            this._loading = false;
            this.RaiseFileChangedEvent();
        }

        private void CountLinesInFile()
        {
            long pos = this._streamReader.BaseStream.Position; // save position
            this._linesInFile = 0;
            this._streamReader.BaseStream.Position = 0; // go to beginning of file
            while (!this._streamReader.EndOfStream)
            {
                this._streamReader.ReadLine();
                this._linesInFile++;
            }
            this._streamReader.DiscardBufferedData();
            this._streamReader.BaseStream.Position = pos; // back to saved position
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
            return newStartLine;
        }

        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OxTailFileWatcher));

        // Provide CLR accessors for the event
        public event RoutedEventHandler FileChanged
        {
            add { AddHandler(FileChangedEvent, value); }
            remove { RemoveHandler(FileChangedEvent, value); }
        }

        // This method raises the FileChanged event
        void RaiseFileChangedEvent()
        {
            Image img = base.GetTemplateChild("PART_Icon") as Image;
            if (img != null)
            {
                img.Source = new BitmapImage(new Uri("/Controls/Images/bell.png", UriKind.Relative));
            }
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OxTailFileWatcher.FileChangedEvent);
            this.RaiseEvent(new RoutedEventArgs(OxTailFileWatcher.FileChangedEvent, this));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this._streamReader != null)
            {
                this._streamReader.Close();
                this._streamReader.Dispose();
            }
            //if (this._fileStream != null)
            //{
            //    _fileStream.Close();
            //    _fileStream.Dispose();
            //}
        }

        #endregion
    }
}

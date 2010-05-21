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
    public class OxTailFileViewer : CloseableTabItem, IDisposable
    {
        private StreamReader _streamReader = null;
        private ColourfulListView _viewer = null;
        private BackgroundWorker _bw = null;
        private Grid _grid = null;
        private volatile bool _loading = false;
        private long _startLine = 0;
        private long _linesInFile = 0;

        private int _visibleLines = 10;
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
            SeekOrigin seekOrigin = this.FindShortestSeekOrigin(target, out smallestOffset);
            this._streamReader.BaseStream.Seek(smallestOffset, seekOrigin);
            this._viewer.Items.Clear();
            for (int i = 0; i < this.VisibleLines; i++)
            {
                this._viewer.Items.Add(new TextBlock(new Run(this._streamReader.ReadLine())));
            }
        }

        private SeekOrigin FindShortestSeekOrigin(long target, out long smallestOffset)
        {
            long beginOffset = target;
            long currentOffset = Math.Max(target - this._streamReader.BaseStream.Position, 0);
            long endOffset = this._streamReader.BaseStream.Length - target;
            smallestOffset = Math.Min(Math.Min(beginOffset, currentOffset), endOffset);

            SeekOrigin shortestSeekOrigin = smallestOffset == beginOffset ? SeekOrigin.Begin : smallestOffset == currentOffset? SeekOrigin.Current : SeekOrigin.End;
            
            return shortestSeekOrigin;
        }

        private long StartLineFileStreamOffset()
        {
            long offset;
            int blockSize = 512;
            int indexIncrement = blockSize;
            char[] block = new char[blockSize];
            long linesToRead;
            long linesEncountered = 0;
            int index;

            // educated guess at quickest direction to go - position the cursor at the beginning if our first line is in the first "half" (variable line lengths!) of the file
            if (this._startLine < (this._linesInFile / 2))
            {
                index = 0;
                linesToRead = this._startLine;
            }
            else
            {
                index = (int)this._streamReader.BaseStream.Length - indexIncrement;
                indexIncrement *= -1;
                linesToRead = this._linesInFile - this._startLine;
            }


            StringBuilder stringBuilderBlock = new StringBuilder();
            do
            {
                // read the next block 
                this._streamReader.ReadBlock(block, index, blockSize);
                string stringBlock = new String(block);
                stringBuilderBlock.Append(stringBlock);
                linesEncountered += CountLines(stringBlock);
                index += indexIncrement;
            } while (linesEncountered < linesToRead);

            //todo: calculate offset from index to linesToRead-th newline

            return offset;
        }

        private long CountLines(string stringBlock)
        {
            return stringBlock.Split(new string[] {Environment.NewLine}, StringSplitOptions.None).LongLength;
        }
        
        int Interval { get; set; }

        public OxTailFileViewer()
            : base()
        {
        }

        /// <summary>
        /// Creates an OxTailFileViewer instance and starts viewing the specified filename.
        /// </summary>
        /// <param name="filename">The full file path of the file to view.</param>
        public OxTailFileViewer(string filename)
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
                    if (this._streamReader.BaseStream.Position > this._streamReader.BaseStream.Length)
                    {
                        this._streamReader.BaseStream.Position = 0;
                    }
                    if (this._streamReader.BaseStream.Position < this._streamReader.BaseStream.Length)
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
                newStartLine = this._linesInFile - this.VisibleLines;
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
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OxTailFileViewer));

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
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OxTailFileViewer.FileChangedEvent);
            this.RaiseEvent(new RoutedEventArgs(OxTailFileViewer.FileChangedEvent, this));
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

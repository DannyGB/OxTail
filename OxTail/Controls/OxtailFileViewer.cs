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

    public class OxtailFileViewer : CloseableTabItem, IDisposable
    {
        private StreamReader _streamReader = null;
        private System.Windows.Controls.TextBox _viewer = null;
        private BackgroundWorker _bw = null;
        private Grid _grid = null;

        public int Interval { get; set; }

        public OxtailFileViewer(string filename)
        {
            this.Header = Path.GetFileName(filename);
            this.Uid = filename;
            this.Interval = 1000;
            //this._fileStream = FileHelper.OpenFile(filename);
            this._streamReader = new StreamReader(FileHelper.OpenFile(filename));
            //this._lastLength = _fileStream.Length;
            this._grid = new Grid();
            this.Visibility = System.Windows.Visibility.Visible;
            this.AddChild(this._grid);
            this._viewer = new System.Windows.Controls.TextBox();
            this._viewer.Visibility = System.Windows.Visibility.Visible;
            this._viewer.IsReadOnly  = true;
            this._viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this._viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this._grid.Children.Add(this._viewer);
            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerAsync();
        }

        void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this._bw.CancellationPending)
            {
                if (!_streamReader.EndOfStream)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { this.ReadNewTextFromFile(); }));
                }
                Thread.Sleep(this.Interval);
            }
        }

        public override void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this._bw.CancelAsync();
            base.closeButton_Click(sender, e);
        }

        private void ReadNewTextFromFile()
        {
            this._viewer.Text += this._streamReader.ReadToEnd();
            this.RaiseFileChangedEvent();
        }

        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OxtailFileViewer));

        // Provide CLR accessors for the event
        public event RoutedEventHandler FileChanged
        {
            add { AddHandler(FileChangedEvent, value); }
            remove { RemoveHandler(FileChangedEvent, value); }
        }

        // This method raises the FileChanged event
        void RaiseFileChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OxtailFileViewer.FileChangedEvent);
            this.RaiseEvent(new RoutedEventArgs(OxtailFileViewer.FileChangedEvent, this));
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

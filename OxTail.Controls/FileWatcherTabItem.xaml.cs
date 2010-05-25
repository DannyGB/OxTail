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
    using System.Text;
    using System.IO;
    using System.Windows.Controls;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Threading;
    using System.Windows;
    using OxTailLogic.PatternMatching;
    using System.Windows.Media.Imaging;
    using System.Windows.Documents;
    using OxTail.Helpers;

    /// <summary>
    /// An extension of the CloseableTabItem control that views a file.
    /// </summary>
    public partial class FileWatcherTabItem : CloseableTabItem, IDisposable
    {
        //private FileWatcher fileWatcher = null;
        
        public FileWatcherTabItem()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Creates an OxTailFileViewer instance and starts viewing the specified filename.
        /// </summary>
        /// <param name="filename">The full file path of the file to view.</param>
        public FileWatcherTabItem(string filename): this()
        {
            this.Header = Path.GetFileName(filename);
            this.ToolTip = filename;
            this.Uid = filename;
            this.Visibility = System.Windows.Visibility.Visible;
            //this.fileWatcher = new FileWatcher(filename);
            //this.fileWatcher.Visibility = System.Windows.Visibility.Visible;
            //this.fileWatcher.FileChanged += new RoutedEventHandler(_fileWatcher_FileChanged);
            //this.gridContent.Children.Add(this.fileWatcher);
            this.GotFocus += new RoutedEventHandler(OxTailFileViewer_GotFocus);
        }

        void fileWatcher_FileChanged(object sender, RoutedEventArgs e)
        {
            RaiseFileChangedEvent();
        }

        void OxTailFileViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            Image img = base.GetTemplateChild("PART_Icon") as Image;
            if (img != null)
            {
                img.Source = null;
            }
        }

        public override void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileWatcher.Stop();
            base.closeButton_Click(sender, e);
        }

        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileWatcherTabItem));

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
                img.Source = new BitmapImage(new Uri("Images/bell.png", UriKind.Relative));
            }
            RoutedEventArgs newEventArgs = new RoutedEventArgs(FileWatcherTabItem.FileChangedEvent);
            this.RaiseEvent(new RoutedEventArgs(FileWatcherTabItem.FileChangedEvent, this));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.fileWatcher != null)
            {
                this.fileWatcher.Stop();
                this.fileWatcher.Dispose();
            }
        }

        #endregion

        private void CloseableTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.fileWatcher.Start(this.Uid);
        }
    }
}

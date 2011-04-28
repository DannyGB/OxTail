/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
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
    using System.IO;
    using System.Windows.Controls;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Collections.Generic;
    using OxTailHelpers;

    /// <summary>
    /// An extension of the CloseableTabItem control that views a file.
    /// </summary>
    public partial class FileWatcherTabItem : TabItem, IDisposable
    {
        public event EventHandler<EventArgs> FindFinished;

        private Image _image = null;
        private int LastFindOffset { get; set; }

        public List<HighlightedItem> SelectedItem 
        {
            get
            {
                return this.fileWatcher.SelectedItem;
            }
        }

        public FindDetails FindDetails
        {
            set
            {
                this.fileWatcher.FindDetails = value;
            }
        }

        public FileWatcherTabItem()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Creates an OxTailFileViewer instance and starts viewing the specified filename.
        /// </summary>
        /// <param name="filename">The full file path of the file to view.</param>
        public FileWatcherTabItem(string filename, HighlightCollection<HighlightItem> patterns)
            : this()
        {
            this.Header = Path.GetFileName(filename);
            this.ToolTip = filename;
            this.Uid = filename;
            this.Visibility = System.Windows.Visibility.Visible;
            this.fileWatcher.Patterns = patterns;
            this.GotFocus += new RoutedEventHandler(OxTailFileViewer_GotFocus);
            this.fileWatcher.FindFinished += new EventHandler<EventArgs>(fileWatcher_FindFinished);
        }        

        void fileWatcher_FileChanged(object sender, RoutedEventArgs e)
        {
            OnFileChanged();
        }

        void OxTailFileViewer_GotFocus(object sender, RoutedEventArgs e)
        {
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
        void OnFileChanged()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(RationalFileWatcher.FileChangedEvent);
                this.RaiseEvent(new RoutedEventArgs(RationalFileWatcher.FileChangedEvent, this));
            }
             ));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.fileWatcher != null)
            {
                this.fileWatcher.Dispose();
            }
        }

        #endregion

        private void CloseableTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.fileWatcher.Start(this.Uid);
        }

        public void ResetSearchCriteria()
        {
            this.fileWatcher.ResetSearchCriteria();
        }

        private void ThrowFindFinished()
        {
            if (this.FindFinished != null)
            {
                this.FindFinished(this, new EventArgs());
            }
        }

        void fileWatcher_FindFinished(object sender, EventArgs e)
        {
            this.ThrowFindFinished();
        }
    }
}

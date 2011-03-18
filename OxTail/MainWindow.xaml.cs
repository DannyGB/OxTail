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

namespace OxTail
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using OxTail.Controls;
    using OxTail.Helpers;
    using OxTail.Properties;
    using System.Collections.Generic;
    using System.Text;
    using OxTailHelpers;
    using OxTailLogic.Compare;
    using OxTailLogic;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : BaseMainWindow, IMainWindowKeyPressMethods
    {
        private List<Window> OpenWindows { get; set; }
        private FileWatcherTabItem FileToSearch { get; set; }
        private bool StopFileSwitchOnSearch { get; set; }

        /// <summary>
        /// The current <see cref="HighlightCollection<T>"/> of <see cref="HighlightItem"/>
        /// </summary>
        public static HighlightCollection<HighlightItem> HighlightItems { get; set; }

        /// <summary>
        /// Initialise MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.OpenWindows = new List<Window>();
        }

        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        public void OpenFile()
        {
            string filename = FileHelper.ShowOpenFileDialog();

            OpenFile(filename);
        }

        private void MenuHighlightingClick(object sender, RoutedEventArgs e)
        {
            OpenHightlightScreen();
        }

        public void OpenHightlightScreen()
        {
            Highlight hl = new Highlight();
            hl.ShowDialog();
        }

        private void MenuExpressionBuilderClick(object sender, RoutedEventArgs e)
        {
            ExpressionBuilder hl = new ExpressionBuilder();
            hl.ShowDialog();
        }

        private void OpenFile(string filename)
        {
            if (filename != string.Empty)
            {
                if (System.IO.File.Exists(filename))
                {
                    OxTail.Controls.FileWatcherTabItem newTab = FindTabByFilename(filename);
                    if (newTab == null)
                    {
                        newTab = new OxTail.Controls.FileWatcherTabItem(filename, MainWindow.HighlightItems);
                        newTab.FindFinished += new EventHandler<EventArgs>(MainWindow_FindFinished);
                        tabControlMain.Items.Add(newTab);
                    }
                    tabControlMain.SelectedItem = newTab;
                    recentFileList.InsertFile(filename);
                }
                else if (MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.REMOVE_FROM_RECENT_FILE_LIST), LanguageHelper.GetLocalisedText((Application.Current as IApplication), "fileNotFound"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    recentFileList.RemoveFile(filename);
                }
            }
        }

        private OxTail.Controls.FileWatcherTabItem FindTabByFilename(string filename)
        {
            OxTail.Controls.FileWatcherTabItem foundTab = null;
            foreach (OxTail.Controls.FileWatcherTabItem tab in tabControlMain.Items.OfType<OxTail.Controls.FileWatcherTabItem>())
            {
                if (tab.Uid == filename)
                {
                    foundTab = tab;
                    break;
                }
            }
            return foundTab;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HighlightItems = HighlightItem.LoadHighlights(Settings.Default.HighlightFileLocations);
            HighlightItems.ApplySort(null, ListSortDirection.Descending);
        }

        private void MenuOpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            List<FileInfo> files = this.OpenDirectory(true);

            foreach (FileInfo item in files)
            {
                OpenFile(item.FullName);
            }
        }

        private List<FileInfo> OpenDirectory(bool showFileLimitMessage)
        {
            return FileOpenLogic.OpenDirectory(showFileLimitMessage, (Application.Current as IApplication), Settings.Default.MaxFilesToOpen);
        }

        private void MenuOpenLastWrittenPatterns_Click(object sender, RoutedEventArgs e)
        {
            ISaveExpressionMessage msg = new SaveExpressionMessage();
            msg.Label = LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.MULTIPLE_FILE_TEXT_PATTERN);
            msg.Message = Constants.DEFAULT_MULTIPLE_FILE_OPEN_PATTERN;

            List<List<FileInfo>> files = FileOpenLogic.OpenFilePattern(msg);
            files = FileOpenLogic.OpenLastWrittenToFile(files);

            foreach (List<FileInfo> fileList in files)
            {
                if (fileList.Count > 0)
                {
                    OpenFile(fileList[0].FullName);
                }
            }
        }

        private void MenuOpenLastWritten_Click(object sender, RoutedEventArgs e)
        {
            List<List<FileInfo>> files = FileOpenLogic.OpenLastWrittenToFile();

            foreach (List<FileInfo> fileList in files)
            {
                if (fileList.Count > 0)
                {
                    OpenFile(fileList[0].FullName);
                }
            }
        }

        private void MenuOpenFilePattern_Click(object sender, RoutedEventArgs e)
        {
            List<List<FileInfo>> fileInfos = FileOpenLogic.OpenFilePattern((new SaveExpressionMessage() as ISaveExpressionMessage));

            foreach (List<FileInfo> fileList in fileInfos)
            {
                foreach (FileInfo item in fileList)
                {
                    this.OpenFile(item.FullName);
                }
            }
        }

        private void MenuItemCloseAll_Click(object sender, RoutedEventArgs e)
        {
            tabControlMain.Items.Clear();
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            FileWatcherTabItem closeTab = (FileWatcherTabItem)this.tabControlMain.SelectedItem;
            tabControlMain.Items.Remove(closeTab);
            closeTab.Dispose();
        }

        private void tabControlMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is FileWatcherTabItem)
            {
                this.FileToSearch = (FileWatcherTabItem)e.AddedItems[0];
            }
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyText();
        }

        public void CopyText()
        {
            if (tabControlMain.SelectedIndex >= 0 && tabControlMain.SelectedIndex < tabControlMain.Items.Count)
            {
                FileWatcherTabItem tab = tabControlMain.Items[tabControlMain.SelectedIndex] as FileWatcherTabItem;
                if (tab != null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (HighlightedItem item in tab.SelectedItem)
                    {
                        sb.AppendLine(item.Text);
                    }

                    ClipboardHelper.AddTextToClipboard(sb.ToString());
                }
            }
        }

        private void MenuFind_Click(object sender, RoutedEventArgs e)
        {
            OpenFindScreen();
        }

        public void OpenFindScreen()
        {
            foreach (Window w in this.OpenWindows)
            {
                if (w is Find)
                {
                    w.Activate();
                    return;
                }
            }

            Find find = new Find();
            find.FindCriteria += new Controls.Find.FindText(find_FindCriteria);
            find.Closed += new EventHandler(find_Closed);
            this.OpenWindows.Add(find);

            find.Show();
        }

        void find_Closed(object sender, EventArgs e)
        {
            if (this.OpenWindows.Contains((sender as Window)))
            {
                this.OpenWindows.Remove((sender as Window));

                if (tabControlMain.Items.Count > 0)
                {
                    ((FileWatcherTabItem)tabControlMain.Items[tabControlMain.SelectedIndex]).ResetSearchCriteria();
                }
            }
        }

        void find_FindCriteria(object sender, FindEventArgs e)
        {
            if (tabControlMain.Items.Count > 0)
            {
                if (e.Options == OxTailLogic.PatternMatching.FindOptions.AllOpenDocuments)
                {
                    StopFileSwitchOnSearch = false;
                    FileToSearch.Find(e.FindCriteria);
                }
                else if (e.Options == OxTailLogic.PatternMatching.FindOptions.CurrentDocument)
                {
                    StopFileSwitchOnSearch = true;
                    ((FileWatcherTabItem)tabControlMain.Items[tabControlMain.SelectedIndex]).Find(e.FindCriteria);
                }
            }
        }

        void MainWindow_FindFinished(object sender, EventArgs e)
        {
            if (!this.StopFileSwitchOnSearch)
            {
                int index = tabControlMain.Items.IndexOf(this.FileToSearch);

                if (index >= 0 && index < (tabControlMain.Items.Count - 1))
                {
                    tabControlMain.SelectedIndex = (index + 1);
                }
                else
                {
                    tabControlMain.SelectedIndex = 0;
                }
            }
        }

        private void MenuOnWeb_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Settings.Default.WebsiteUrl);
        }

        private void BaseWindow_Closing(object sender, CancelEventArgs e)
        {
            CloseAllOpenWindowsWhenMainWindowClosed();
        }

        private void CloseAllOpenWindowsWhenMainWindowClosed()
        {
            for (int i = this.OpenWindows.Count - 1; i >= 0; i--)
            {
                Window w = this.OpenWindows[i];
                w.Close();
            }
        }

        private void recentFileList_SubMenuClick(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string filename = (string)item.Header;

            if (string.IsNullOrEmpty(filename) || !System.IO.File.Exists(filename))
            {
                return;
            }

            this.OpenFile(filename);

        }

        private void BaseWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (KeyboardHelper.IsLeftControlDown())
            {
                IDictionary<Key, KeyboardHelper.Action> dict = new Dictionary<Key, KeyboardHelper.Action>(4);
                dict.Add(Key.C, CopyText);
                dict.Add(Key.H, OpenFindScreen);
                dict.Add(Key.O, OpenFile);
                dict.Add(Key.I, OpenHightlightScreen);

                KeyboardHelper.StandardControlKeyCombinationPressed(e.Key, (this as IMainWindowKeyPressMethods));
            }            
        }
    }
}
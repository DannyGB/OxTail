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

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {        
        private List<Window> OpenWindows { get; set; }
 
        public MainWindow()
        {
            InitializeComponent();
            this.OpenWindows = new List<Window>();
        }

        public static HighlightCollection<HighlightItem> HighlightItems { get; set; }

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
            string filename = FileHelper.ShowOpenFileDialog();

            OpenFile(filename);
        }

        private void MenuHighlightingClick(object sender, RoutedEventArgs e)
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
                if (File.Exists(filename))
                {
                    OxTail.Controls.FileWatcherTabItem newTab = FindTabByFilename(filename);
                    if (newTab == null)
                    {                        
                        newTab = new OxTail.Controls.FileWatcherTabItem(filename, MainWindow.HighlightItems);
                        newTab.CloseTab += new RoutedEventHandler(newTab_CloseTab);
                        tabControlMain.Items.Add(newTab);                        
                    }
                    tabControlMain.SelectedItem = newTab;                    
                    RecentFileList.InsertFile(filename);
                }
                else if (MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "removeFromRecentFileList"), LanguageHelper.GetLocalisedText((Application.Current as IApplication), "fileNotFound"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RecentFileList.RemoveFile(filename);
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

        void newTab_CloseTab(object sender, RoutedEventArgs e)
        {
            if (e.Source is OxTail.Controls.FileWatcherTabItem)
            {
                OxTail.Controls.FileWatcherTabItem closeTab = e.Source as OxTail.Controls.FileWatcherTabItem;
                tabControlMain.Items.Remove(closeTab);
                
                closeTab.Dispose();
            }
        }

        private void RecentFileList_MenuClick(object sender, RecentFileList.MenuClickEventArgs e)
        {
            this.OpenFile(e.Filepath);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HighlightItems = HighlightItem.LoadHighlights(Settings.Default.HighlightFileLocations);
        }

        private void MenuOpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            List<FileInfo> files = OpenDirectory();

            foreach (FileInfo item in files)
            {
                OpenFile(item.FullName);
            }
        }

        private List<FileInfo> OpenDirectory()
        {
            MessageBox.Show(string.Format(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "fileOpenLimit"), Settings.Default.MaxFilesToOpen), Application.Current.MainWindow.GetType().Assembly.GetName().Name);

            List<FileInfo> fileInfoList = new List<FileInfo>();

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileInfoList = FileHelper.GetFiles(folderDialog.SelectedPath, "*", Settings.Default.MaxFilesToOpen);
            }

            return fileInfoList;
        }

        private void MenuOpenFilePattern_Click(object sender, RoutedEventArgs e)
        {
            List<FileInfo> fileInfos = OpenFilePattern();
            foreach (FileInfo item in fileInfos)
            {
                this.OpenFile(item.FullName);
            }
        }

        private static List<FileInfo> OpenFilePattern()
        {
            // Currently using the OpenFileDialog box and then hacking the return value.
            // 
            // Started to look at inheriting from OpenFileDialog but it's a sealed class so this is the 
            // short term "get it working" code and will look at creating an OpenFilePatternDialog
            // latter
            //string filename = FileHelper.ShowOpenFileDialog();
            string filename = FileHelper.ShowOpenDirectory();
            List<FileInfo> fileInfos = new List<FileInfo>();

            SaveExpressionMessage msg = new SaveExpressionMessage();
            msg.Label = LanguageHelper.GetLocalisedText((Application.Current as IApplication), "filePatternText");
            msg.Message = "*.log";
            msg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrWhiteSpace(filename))
            {
                bool? result = msg.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    if (msg.Message == "*.*")
                    {
                        if (MessageBoxResult.Yes == MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "lotOfFilesText"), LanguageHelper.GetLocalisedText((Application.Current as IApplication), "question"), MessageBoxButton.YesNo))
                        {
                            fileInfos = FileHelper.GetFiles(filename, msg.Message);
                        }
                    }
                }
            }

            return fileInfos;
        }

        private void MenuItemCloseAll_Click(object sender, RoutedEventArgs e)
        {
            tabControlMain.Items.Clear();
        }

        private void tabControlMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {          
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
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
                    //foreach (FileWatcherTabItem item in tabControlMain.Items)
                    //{
                    //    item.Find(e.FindCriteria);
                    //}
                }
                else if (e.Options == OxTailLogic.PatternMatching.FindOptions.CurrentDocument)
                {
                    ((FileWatcherTabItem)tabControlMain.Items[tabControlMain.SelectedIndex]).Find(e.FindCriteria);
                }
            }
        }

        private void MenuOnWeb_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Settings.Default.WebsiteUrl);
        }
    }
}
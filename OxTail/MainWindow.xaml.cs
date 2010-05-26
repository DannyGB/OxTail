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
    using OxTail.Helpers;
    using System.IO;
    using OxTail.Controls;
    using System.Collections.ObjectModel;
    using OxTailLogic.PatternMatching;
    using OxTail.Properties;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static ObservableCollection<HighlightItem> HighlightItems { get; set; }

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

        private void OpenFile(string filename)
        {
            if (filename != string.Empty)
            {
                if (File.Exists(filename))
                {
                    OxTail.Controls.FileWatcherTabItem newTab = FindTabByFilename(filename);
                    if (newTab == null)
                    {
                        newTab = new OxTail.Controls.FileWatcherTabItem(filename);
                        newTab.CloseTab += new RoutedEventHandler(newTab_CloseTab);
                        tabControlMain.Items.Add(newTab);
                    }
                    tabControlMain.SelectedItem = newTab;
                    RecentFileList.InsertFile(filename);
                }
                else if (MessageBox.Show("Remove from recent file list?", "File not found!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
    }
}
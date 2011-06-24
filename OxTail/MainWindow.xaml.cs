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
    using OxTailLogic;
    using System.Windows.Controls;
    using System.Windows.Input;
    using OxTailHelpers.Data;
    using OxTailLogic.Data;    
    using System.Drawing;
    using System.Reflection;
    using Ninject;    

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : BaseMainWindow, IMainWindowKeyPressMethods
    {
        private List<IWindow> OpenWindows { get; set; }
        private FileWatcherTabItem FileToSearch { get; set; }
        private bool StopFileSwitchOnSearch { get; set; }
        private List<LastOpenFiles> LastOpenFiles { get; set; }
        

        // Ensure good encapsulation
        private readonly RecentFileList recentFileList;
        private readonly ILastOpenFilesData LastOpenFilesData;
        private readonly IAppSettingsData AppSettingsData;
        private readonly IHighlightItemData HighlightItemData;
        private readonly IWindowFactory WindowFactory;
        private readonly IFindWindowFactory FindWindowFactory;
        private readonly ISaveExpressionMessageWindowFactory SaveExpressionMessageWindowFactory;
        private readonly ISystemTray SystemTray;
        private readonly System.Windows.Forms.NotifyIcon Notify;

        private IWindow About;
        private IWindow Highlight;
        private IWindow ExpressionBuilder;
        private IWindow ApplicationSettings;
        private IFindWindow Find;

        /// <summary>
        /// The current <see cref="HighlightCollection<T>"/> of <see cref="HighlightItem"/>
        /// </summary>
        public static HighlightCollection<HighlightItem> HighlightItems { get; set; }       

        [Inject]
        // If DI is about removing all "new" operators from your logic to enable a plugin architecture where
        // every component can be changed in one place and that we must rely on abstraction rather than concretions
        // then everything must implement an interface and be passed in on the constructor (even other windows)
        // This makes sense as a for instance: if you wanted to replace the crappy About window we have here
        // with a nice spanking one then we change it when we load our Ninject Kernel in App.xaml. Then if we called
        // that screen from other windows later on in the stack the change is only in one place
        // I fully intend to remove all "new" operators from this code file as a test to see how plausible it is to do!
        public MainWindow(RecentFileList recentFileList, ILastOpenFilesData lastOpenFilesData, IAppSettingsData appSettingsData,
            IHighlightItemData highlightItemData, IWindowFactory windowFactory, IFindWindowFactory findWindowFactory, ISystemTray systemTray, 
            System.Windows.Forms.NotifyIcon notifyIcon, ISaveExpressionMessageWindowFactory saveExpressionMessageWindowFactory)
        {
            
            this.AppSettingsData = appSettingsData;
            this.LastOpenFilesData = lastOpenFilesData;
            this.recentFileList = recentFileList;
            this.WindowFactory = windowFactory;
            this.FindWindowFactory = findWindowFactory;
            this.HighlightItemData = highlightItemData;
            this.SaveExpressionMessageWindowFactory = saveExpressionMessageWindowFactory;
            this.SystemTray = systemTray;

            InitializeComponent();
            
            this.MenuItemFile.Items.Insert(2, this.recentFileList);

            // Null Object pattern (http://en.wikipedia.org/wiki/Null_Object_pattern)
            this.OpenWindows = new List<IWindow>(0);
            this.LastOpenFiles = new List<LastOpenFiles>(0);

            this.Notify = notifyIcon;
            this.Notify.Icon = this.SystemTray.Icon;
            this.Notify.DoubleClick += new EventHandler(Notify_DoubleClick);
            this.Notify.ContextMenu = this.SystemTray.ContextMenu;

            this.Notify.ContextMenu.MenuItems[0].Click += new EventHandler(disableSoundsMenuItem_Click);
            this.Notify.ContextMenu.MenuItems[1].Click += new EventHandler(minimuseToTrayItem_Click);
            this.Notify.ContextMenu.MenuItems[3].Click += new EventHandler(exitItem_Click);

            this.Notify.Visible = true;
        }

        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            this.About = WindowFactory.CreateWindow("About");
            About.ShowDialog();
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();            
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
            this.Highlight = WindowFactory.CreateWindow("Highlight");
            this.Highlight.ShowDialog();
        }

        private void MenuExpressionBuilderClick(object sender, RoutedEventArgs e)
        {
            this.ExpressionBuilder = WindowFactory.CreateWindow("ExpressionBuilder");
            ExpressionBuilder.ShowDialog();
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
            SettingsHelper.AppSettings = this.AppSettingsData.ReadAppSettings();

            HighlightItems = this.HighlightItemData.Read();
            HighlightItems.ApplySort(null, ListSortDirection.Descending);

            if (bool.Parse(SettingsHelper.AppSettings[AppSettings.REOPEN_FILES]))
            {
                LoadLastOpenFiles();
            }
        }

        private void Notify_DoubleClick(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.WindowState = System.Windows.WindowState.Minimized;
                    break;
                case WindowState.Minimized:
                    this.WindowState = System.Windows.WindowState.Normal;
                    break;
                case WindowState.Normal:
                    this.WindowState = System.Windows.WindowState.Minimized;
                    break;
                default:
                    break;
            }
        }       

        private void minimuseToTrayItem_Click(object sender, EventArgs e)
        {
            SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY] = (!bool.Parse(SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY])).ToString();
            ((System.Windows.Forms.MenuItem)sender).Checked = bool.Parse(SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY]);
        }

        private void disableSoundsMenuItem_Click(object sender, EventArgs e)
        {
            SettingsHelper.AppSettings[AppSettings.PLAY_SOUND] = (!bool.Parse(SettingsHelper.AppSettings[AppSettings.PLAY_SOUND])).ToString();
            ((System.Windows.Forms.MenuItem)sender).Checked = bool.Parse(SettingsHelper.AppSettings[AppSettings.PLAY_SOUND]);
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void LoadLastOpenFiles()
        {
            this.LastOpenFiles = LastOpenFilesData.Read();

            foreach (LastOpenFiles file in LastOpenFiles)
            {
                this.OpenFile(file.Filename);
            }
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
            return FileOpenLogic.OpenDirectory(showFileLimitMessage, (Application.Current as IApplication), int.Parse(SettingsHelper.AppSettings[AppSettings.MAX_OPEN_FILES]));
        }

        private void MenuOpenLastWrittenPatterns_Click(object sender, RoutedEventArgs e)
        {
            ISaveExpressionMessage msg = this.SaveExpressionMessageWindowFactory.CreateWindow();
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
            
            if (closeTab != null)
            {
                tabControlMain.Items.Remove(closeTab);
                closeTab.Dispose();
            }
        }

        private void tabControlMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is FileWatcherTabItem)
            {
                this.FileToSearch = (FileWatcherTabItem)e.AddedItems[0];
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {            
            ApplicationSettings = this.WindowFactory.CreateWindow("ApplicationSettings");
            ApplicationSettings.SaveClick += (s, ev) => SaveApplicationSettings(ApplicationSettings);
            ApplicationSettings.ShowDialog();
        }

        private void SaveApplicationSettings(IWindow settings)
        {
            this.AppSettingsData.WriteAppSettings(SettingsHelper.AppSettings);
            settings.Close();
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
            foreach (IWindow w in this.OpenWindows)
            {
                if (w is IFindWindow)
                {
                    w.Activate();
                    return;
                }
            }

            this.Find = FindWindowFactory.CreateWindow();
            this.Find.FindCriteria += new FindText(find_FindCriteria);
            this.Find.Closed += new RoutedEventHandler(find_Closed);
            this.OpenWindows.Add(this.Find);

            this.Find.Show();
        }

        void find_Closed(object sender, EventArgs e)
        {
            if (this.OpenWindows.Contains((sender as IWindow)))
            {
                this.OpenWindows.Remove((sender as IWindow));

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
                if (e.FindDetails.Options == FindOptions.AllOpenDocuments)
                {
                    StopFileSwitchOnSearch = false;
                    FileToSearch.FindDetails = e.FindDetails;
                    e.FindDetails.InitiateSearch();
                    //FileToSearch.Find(e.FindDetails.FindCriteria);
                }
                else if (e.FindDetails.Options == FindOptions.CurrentDocument)
                {
                    StopFileSwitchOnSearch = true;
                    ((FileWatcherTabItem)tabControlMain.Items[tabControlMain.SelectedIndex]).FindDetails = e.FindDetails;
                    e.FindDetails.InitiateSearch();
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
            try
            {
                System.Diagnostics.Process.Start(Settings.Default.WebsiteUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Constants.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BaseWindow_Closing(object sender, CancelEventArgs e)
        {
            if (bool.Parse(SettingsHelper.AppSettings[AppSettings.REOPEN_FILES]))
            {
                foreach (FileWatcherTabItem tab in this.tabControlMain.Items)
                {
                    var exists = from p in this.LastOpenFiles where p.Filename == tab.Uid select p;
                    if (exists == null || exists.Count<LastOpenFiles>() <= 0)
                    {
                        this.LastOpenFiles.Add(new LastOpenFiles(tab.Uid));
                    }
                }

                if (this.LastOpenFiles != null)
                {
                    this.LastOpenFilesData.Write(this.LastOpenFiles);
                }
            }
            else
            {
                LastOpenFilesData.Clear();
            }

            CloseAllOpenWindowsWhenMainWindowClosed();
            this.Notify.Visible = false;
        }

        private void CloseAllOpenWindowsWhenMainWindowClosed()
        {
            for (int i = this.OpenWindows.Count - 1; i >= 0; i--)
            {
                IWindow w = this.OpenWindows[i];
                w.Close();
            }
        }

        private void recentFileList_SubMenuClick(object sender, EventArgs e)
        {
            string filename = (string)sender;

            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            this.OpenFile(filename);

        }

        private void BaseWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (KeyboardHelper.IsLeftControlDown())
            {
                KeyboardHelper.StandardControlKeyCombinationPressed(e.Key, (this as IMainWindowKeyPressMethods));
            }            
        }

        private void BaseMainWindow_StateChanged(object sender, EventArgs e)
        {
            this.ToggleWindowState();
        }

        private void ToggleWindowState()
        {
            if (bool.Parse(SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY]))
            {
                switch (this.WindowState)
                {
                    case WindowState.Maximized:
                        this.ShowInTaskbar = true;
                        break;
                    case WindowState.Minimized:
                        this.ShowInTaskbar = false;
                        break;
                    case WindowState.Normal:
                        this.ShowInTaskbar = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void BaseMainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
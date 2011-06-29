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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using OxTail.Helpers;
using OxTailHelpers;
using System.Windows;
using OxTailLogic;
using OxTailHelpers.Data;
using OxTailLogic.Data;

namespace OxTail.Controls
{
    public class RecentFileList : MenuItem
    {
        /// <summary>
        /// A Sub menu item has been clicked
        /// </summary>
        public event EventHandler<EventArgs> SubMenuClick;

        private readonly IMostRecentFilesData Data;
        private readonly IFileFactory FileFactory;
        private readonly ISettingsHelper SettingsHelper;

        private List<IFile> Files { get; set; }

        private void Load()
        {
            this.Files = Data.Read(new List<IFile>(0));
        }

        /// <summary>
        /// Initialise instance
        /// </summary>
        public RecentFileList(IMostRecentFilesData data, IFileFactory fileFactory, ISettingsHelper settingsHelper)
        {
            this.SettingsHelper = settingsHelper;
            this.FileFactory = fileFactory;
            this.Data = data;
            this.Load();
            this.Header = LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.RECENT_FILES_MENUITEM_HEADER);
            this.Loaded += (s, e) => GetParentItem();
        }

        private void GetParentItem()
        {
            MenuItem parent = (MenuItem)this.Parent;
            MenuItem child = null;

            this.Items.Clear();

            int i = 1;
            foreach(OxTail.Helpers.File file in this.Files)
            {
                child = new MenuItem();                
                child.Header = "_" + i.ToString() + " " + file.Filename;
                child.Click += new System.Windows.RoutedEventHandler(child_Click);
                this.Items.Add(child);
                i++;
            }
        }

        void child_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            EventHandler<EventArgs> MenuClick = SubMenuClick;
            
            string headerText = item.Header.ToString();
            int startIndex = headerText.IndexOf(' ') + 1;
            string filename = headerText.Substring(startIndex, (headerText.Length - startIndex));

            if (MenuClick != null) MenuClick(filename, new EventArgs());
        }

        /// <summary>
        /// Insert <paramref name="filename"/> into the MRU
        /// </summary>
        /// <param name="filename"></param>
        public void InsertFile(string filename)
        {
            bool found = false;

            foreach(IFile file in Files)
            {
                if (file.Filename == filename)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Files.Add(this.FileFactory.CreateFile(0, filename, "MostRecentFile"));

                if (Files.Count > int.Parse(SettingsHelper.AppSettings[AppSettings.MAX_MRU_FILES]))
                {
                    Files.RemoveAt(0);                    
                }

                WriteMostRecentFilesToDatabase();
            }
        }

        /// <summary>
        /// Remove <paramref name="filename"/> from the MRU
        /// </summary>
        /// <param name="filename"></param>
        public void RemoveFile(string filename)
        {
            for (int i = Files.Count-1; i >= 0; i--)
            {
                IFile file = Files[i];

                if (file.Filename == filename)
                {
                    this.Files.Remove(file);
                    WriteMostRecentFilesToDatabase();
                    
                    break;
                }
            }
        }

        private void WriteMostRecentFilesToDatabase()
        {
            if (this.Files != null)
            {
                this.Files = Data.Write(this.Files);
            }
        }
    }
}

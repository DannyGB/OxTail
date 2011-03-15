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
using System.IO;
using OxTail.Helpers;
using System.Windows;
using OxTailHelpers;
using OxTailLogic.Compare;
using System.ComponentModel;

namespace OxTailLogic
{
    public class FileOpenLogic
    {
        public static List<FileInfo> OpenFilePattern(ISaveExpressionMessage saveExpression)
        {
            // Currently using the OpenFileDialog box and then hacking the return value.
            // 
            // Started to look at inheriting from OpenFileDialog but it's a sealed class so this is the 
            // short term "get it working" code and will look at creating an OpenFilePatternDialog
            // latter
            //string filename = FileHelper.ShowOpenFileDialog();
            string filename = FileHelper.ShowOpenDirectory();

            List<FileInfo> fileInfos = new List<FileInfo>();

            saveExpression.Label = LanguageHelper.GetLocalisedText((Application.Current as IApplication), "filePatternText");
            saveExpression.Message = "*.log";
            saveExpression.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrWhiteSpace(filename))
            {
                bool? result = saveExpression.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    if (saveExpression.Message == "*.*")
                    {
                        if (MessageBoxResult.Yes == MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "lotOfFilesText"), LanguageHelper.GetLocalisedText((Application.Current as IApplication), "question"), MessageBoxButton.YesNo))
                        {
                            fileInfos = FileHelper.GetFiles(filename, saveExpression.Message);
                        }
                    }
                    else
                    {
                        fileInfos = FileHelper.GetFiles(filename, saveExpression.Message);
                    }
                }
            }

            return fileInfos;
        }

        public static List<FileInfo> OpenDirectory(bool showFileLimitMessage, IApplication application, int maxOpenFiles)
        {
            if (showFileLimitMessage)
            {
                MessageBox.Show(string.Format(LanguageHelper.GetLocalisedText(application, "fileOpenLimit"), maxOpenFiles), "OxTail"); //TODO: Remove hardcoding
            }

            List<FileInfo> fileInfoList = new List<FileInfo>();

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (maxOpenFiles == 0)
                {
                    fileInfoList = FileHelper.GetFiles(folderDialog.SelectedPath, "*");
                }
                else
                {
                    fileInfoList = FileHelper.GetFiles(folderDialog.SelectedPath, "*", maxOpenFiles);
                }
            }

            return fileInfoList;
        }

        public static List<FileInfo> OpenLastWrittenToFile()
        {
            List<FileInfo> files = OpenDirectory(false, (Application.Current as IApplication), 0);

            if (files.Count > 0)
            {
                files.Sort(new GenericComparer<FileInfo>("LastWriteTime", ListSortDirection.Descending));
            }

            return files;
        }
    }
}

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
        /// <summary>
        /// Opens the Directory open dialog and uses the passed in <paramref name="saveExpression"/> <see cref="ISaveExpressionMessage"/> instance
        /// to get user input of file pattern.
        /// If the <see cref="ISaveExpressionMessage"/> Label and Message properties are string.Empty they are defaulted
        /// </summary>
        /// <param name="saveExpression">An <see cref="ISaveExpressionMessage"/> instance</param>
        /// <returns>A <see cref="List<T>"/> of <see cref="List<T>"/> so that multiple patterns can be searched for and opened</returns>
        public static List<List<FileInfo>> OpenFilePattern(ISaveExpressionMessage saveExpression)
        {
            string filename = FileHelper.ShowOpenDirectory();

            List<List<FileInfo>> fileInfos = new List<List<FileInfo>>();

            if (string.IsNullOrEmpty(saveExpression.Label))
            {
                saveExpression.Label = LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.FILE_TEXT_PATTERN);
            }

            if (string.IsNullOrEmpty(saveExpression.Message))
            {
                saveExpression.Message = Constants.DEFAULT_FILE_OPEN_PATTERN;
            }

            saveExpression.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrWhiteSpace(filename))
            {
                bool? result = saveExpression.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    if (saveExpression.Message.Contains(Constants.ALL_FILES_PATTERN))
                    {
                        if (MessageBoxResult.Yes == MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.LOTS_OF_FILES_TEXT), LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.QUESTION), MessageBoxButton.YesNo))
                        {
                            SplitFileInputPatternAndGetFiles(saveExpression, filename, fileInfos);
                        }
                    }
                    else
                    {
                        SplitFileInputPatternAndGetFiles(saveExpression, filename, fileInfos);
                    }
                }
            }

            return fileInfos;
        }

        private static void SplitFileInputPatternAndGetFiles(ISaveExpressionMessage saveExpression, string filename, List<List<FileInfo>> fileInfos)
        {
            string[] split = saveExpression.Message.Split(',');
            foreach (string s in split)
            {
                fileInfos.Add(new List<FileInfo>(FileHelper.GetFiles(filename, s)));
            }
        }        

        /// <summary>
        /// Shows the OpenDirectory dialog and optionally shows a file open limit message dependant on <paramref name="showFileLimitMessage"/>
        /// <paramref name="maxOpenFiles"/> controls how many files to open, this is only used if <paramref name="showFileLimitMessage"/> is true
        /// </summary>
        /// <param name="showFileLimitMessage">Show a message box explaining that only the <paramref name="maxOpenFiles"/> will be opened</param>
        /// <param name="application">The current application as a <see cref="IApplication"/></param>
        /// <param name="maxOpenFiles">The maximum files to open</param>
        /// <returns>A <see cref="List<T>"/></returns>
        public static List<FileInfo> OpenDirectory(bool showFileLimitMessage, IApplication application, int maxOpenFiles)
        {
            if (showFileLimitMessage)
            {
                MessageBox.Show(string.Format(LanguageHelper.GetLocalisedText(application, Constants.FILE_OPEN_LIMIT), maxOpenFiles), Constants.APPLICATION_NAME);
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

        /// <summary>
        /// Opens the last written to file in the directory
        /// In all fairness all this does is Sort them into LastWrite order descending
        /// </summary>
        /// <returns>A <see cref="List<T>"/> of <see cref="List<T>"/></returns>
        public static List<List<FileInfo>> OpenLastWrittenToFile()
        {
            List<List<FileInfo>> fileList = new List<List<FileInfo>>();
            fileList.Add(new List<FileInfo>(OpenDirectory(false, (Application.Current as IApplication), 0)));

            return OpenLastWrittenToFile(fileList);
        }

        /// <summary>
        /// Opens all the last written to files in the directory
        /// In all fairness all this does is Sort them into LastWrite order descending
        /// </summary>
        /// <returns>A <see cref="List<T>"/> of <see cref="List<T>"/></returns>
        public static List<List<FileInfo>> OpenLastWrittenToFile(List<List<FileInfo>> fileList)
        {
            foreach (List<FileInfo> files in fileList)
            {
                if (files.Count > 0)
                {
                    files.Sort(new GenericComparer<FileInfo>(Constants.LAST_WRITE_TIME_SORT_HEADER, ListSortDirection.Descending));
                }
            }

            return fileList;
        }
    }
}

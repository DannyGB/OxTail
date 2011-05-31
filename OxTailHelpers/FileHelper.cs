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

namespace OxTail.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Win32;
    using System.Windows;
    using System.IO;
    using System.Windows.Documents;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Xml;
    using OxTailHelpers.Data;

    public class FileHelper
    {
        /// <summary>
        /// Shows the OpenDirectory dialog
        /// </summary>
        /// <returns></returns>
        public static string ShowOpenDirectory()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the OpenFileDialog
        /// </summary>
        /// <returns></returns>
        public static string ShowOpenFileDialog()
        {
            return ShowOpenFileDialog(string.Empty);
        }

        /// <summary>
        /// Shows the OpenFileDialog
        /// </summary>
        /// <returns></returns>
        public static string ShowOpenFileDialog(string fileExtensionPattern)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = fileExtensionPattern;
            bool? retVal = dialog.ShowDialog();
            if (retVal.HasValue && retVal.Value)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Opens a the file identified by <paramref name="filename"/> to a <see cref="Stream"/>
        /// </summary>
        /// <param name="filename">The name of the file to open (fully qualified)</param>
        /// <returns>A <see cref="Stream"/> containing the file contents</returns>
        public static Stream OpenFile(string filename)
        {
            try
            {
                FileStream stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Position = 0;
                return stream;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a flow document from the <paramref name="content"/>
        /// </summary>
        /// <param name="content">The content of the stream to be added to the <see cref="FlowDocument"/></param>
        /// <returns>A <see cref="FlowDocument"/></returns>
        public static FlowDocument CreateFlowDocument(Stream content)
        {
            FlowDocument fd = new FlowDocument();
            TextRange textRange = new TextRange(fd.ContentStart, fd.ContentEnd);
            textRange.Load(content, DataFormats.Rtf);

            return fd;
        }

        /// <summary>
        /// Returns a <see cref="Stream"/> containing the contents of the file identified by <paramref name="assembly"/> and <paramref name="resourceName"/>
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/></param>
        /// <param name="resourceName">The resource name</param>
        /// <returns>A <see cref="Stream"/> containing the resource contents</returns>
        public static Stream GetResourceStream(Assembly assembly, string resourceName)
        {
            Stream s = assembly.GetManifestResourceStream(resourceName);

            return s;
        }

        /// <summary>
        /// Serializes the <paramref name="list"/> to the <paramref name="filename"/> using the <paramref name="serializer"/>
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="list">The object to searilize</param>
        public static void SerializeToExecutableDirectory(string filename, XmlSerializer serializer, object list)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException();
            }

            filename = CreateExecutableFilename(filename);
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Creates a <see cref="string"/> containing the executing assembly location and the filename appended
        /// </summary>
        /// <param name="filename">The filename to append to the executing assembly location</param>
        /// <returns>A <see cref="string"/> containing the executing assembly location and the filename appended</returns>
        public static string CreateExecutableFilename(string filename)
        {
            filename = string.Format("{0}\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            return filename;
        }

        /// <summary>
        /// Deserializes from the <paramref name="filename"/> using the <paramref name="serializer"/>
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        /// <param name="serializer">The serializer</param>
        /// <returns>An object containing the data</returns>
        public static object DeserializeFromExecutableDirectory(string filename, XmlSerializer serializer)
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException();
            }

            filename = CreateExecutableFilename(filename);

            FileStream s = new System.IO.FileStream(filename, FileMode.Open);
            using (XmlTextReader reader = new XmlTextReader(s))
            {
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Gets a <see cref="String"/> representation of the given <see cref="Stream"/>
        /// </summary>
        /// <param name="s">The <see cref="Stream"/></param>
        /// <returns>The <see cref="String"/> representation of the <see cref="Stream"/></returns>
        public static string GetStringFromStream(Stream s)
        {
            StreamReader reader = new StreamReader(s);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Gets files from the <paramref name="path"/> using the <paramref name="searchPattern"/>
        /// </summary>
        /// <param name="path">The path to the files</param>
        /// <param name="searchPattern">The search pattern to look for</param>
        /// <returns>A <see cref="List<T>"/> if <see cref="FileInfo"/></returns>
        public static List<FileInfo> GetFiles(string path, string searchPattern)
        {
            string[] files = Directory.GetFiles(path, searchPattern);
            List<FileInfo> fileInfos = new List<FileInfo>(files.Length);

            foreach (string file in files)
            {
                fileInfos.Add(new FileInfo(file));
            }

            return fileInfos;
        }

        /// <summary>
        /// Gets files from the <paramref name="path"/> using the <paramref name="searchPattern"/>
        /// </summary>
        /// <param name="path">The path to the files</param>
        /// <param name="searchPattern">The search pattern to look for</param>
        /// <param name="maxFiles">The maximum files to open</param>
        /// <returns>A <see cref="List<T>"/> if <see cref="FileInfo"/></returns>
        public static List<FileInfo> GetFiles(string path, string searchPattern, int maxFiles)
        {
            List<FileInfo> fileInfos = GetFiles(path, searchPattern);

            if (fileInfos.Count > maxFiles)
            {
                int diff = fileInfos.Count - maxFiles;
                fileInfos.RemoveRange((fileInfos.Count - diff), (fileInfos.Count - maxFiles));
            }

            return fileInfos;
        }
    }
}

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

    public class FileHelper
    {
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

        public static string ShowOpenFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? retVal = dialog.ShowDialog();
            if (retVal.HasValue && retVal.Value)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

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

        public static FlowDocument CreateFlowDocument(Stream content)
        {
            FlowDocument fd = new FlowDocument();
            TextRange textRange = new TextRange(fd.ContentStart, fd.ContentEnd);
            textRange.Load(content, DataFormats.Rtf);

            return fd;
        }

        public static Stream GetResourceStream(Assembly assembly, string resourceName)
        {
            Stream s = assembly.GetManifestResourceStream(resourceName);

            return s;
        }

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

        public static string GetStringFromStream(Stream s)
        {
            StreamReader reader = new StreamReader(s);
            return reader.ReadToEnd();
        }

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

        public static void MruSave(List<OxTail.Helpers.File> files, string saveToFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<OxTail.Helpers.File>), new Type[] { typeof(OxTail.Helpers.File) });
            FileHelper.SerializeToExecutableDirectory(saveToFilename, serializer, files);
        }

        public static List<OxTail.Helpers.File> MruLoad(string openFromFilename)
        {
            List<OxTail.Helpers.File> files = new List<File>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<OxTail.Helpers.File>), new Type[] { typeof(OxTail.Helpers.File) });
            files = (List<OxTail.Helpers.File>)FileHelper.DeserializeFromExecutableDirectory(openFromFilename, serializer);

            return files;
        }
    }
}

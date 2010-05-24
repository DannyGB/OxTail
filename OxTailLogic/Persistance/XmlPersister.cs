﻿/*****************************************************************
* This file is part of OxTail.
*
* OxTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OxTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

/*
 * This code is taken from http://www.codeproject.com/KB/WPF/RecentFileList.aspx and is covered by the http://www.codeproject.com/info/cpol10.aspx
 */

namespace OxTailLogic.Persistance
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Diagnostics;

    public class XmlPersister : IPersist
    {
        public string Filepath { get; set; }
        public Stream Stream { get; set; }

        public XmlPersister()
        {
            Filepath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList.xml");
        }

        public XmlPersister(string filepath)
        {
            Filepath = filepath;
        }

        public XmlPersister(Stream stream)
        {
            Stream = stream;
        }

        public List<string> RecentFiles(int max)
        {
            return Load(max);
        }

        public void InsertFile(string filepath, int max)
        {
            Update(filepath, true, max);
        }

        public void RemoveFile(string filepath, int max)
        {
            Update(filepath, false, max);
        }

        void Update(string filepath, bool insert, int max)
        {
            List<string> old = Load(max);

            List<string> list = new List<string>(old.Count + 1);

            if (insert) list.Add(filepath);

            CopyExcluding(old, filepath, list, max);

            Save(list, max);
        }

        void CopyExcluding(List<string> source, string exclude, List<string> target, int max)
        {
            foreach (string s in source)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    if (!s.Equals(exclude, StringComparison.OrdinalIgnoreCase))
                    {
                        if (target.Count < max)
                        {
                            target.Add(s);
                        }
                    }
                }
            }
        }

        class SmartStream : IDisposable
        {
            bool _IsStreamOwned = true;
            Stream _Stream = null;

            public Stream Stream { get { return _Stream; } }

            public static implicit operator Stream(SmartStream me) { return me.Stream; }

            public SmartStream(string filepath, FileMode mode)
            {
                _IsStreamOwned = true;

                Directory.CreateDirectory(Path.GetDirectoryName(filepath));

                _Stream = File.Open(filepath, mode);
            }

            public SmartStream(Stream stream)
            {
                _IsStreamOwned = false;
                _Stream = stream;
            }

            public void Dispose()
            {
                if (_IsStreamOwned && _Stream != null)
                {
                    _Stream.Dispose();
                }

                _Stream = null;
            }
        }

        SmartStream OpenStream(FileMode mode)
        {
            if (!String.IsNullOrEmpty(Filepath))
            {
                return new SmartStream(Filepath, mode);
            }
            else
            {
                return new SmartStream(Stream);
            }
        }

        List<string> Load(int max)
        {
            List<string> list = new List<string>(max);

            using (MemoryStream ms = new MemoryStream())
            {
                using (SmartStream ss = OpenStream(FileMode.OpenOrCreate))
                {
                    if (ss.Stream.Length == 0) return list;

                    ss.Stream.Position = 0;

                    byte[] buffer = new byte[1 << 20];
                    for (; ; )
                    {
                        int bytes = ss.Stream.Read(buffer, 0, buffer.Length);
                        if (bytes == 0) break;
                        ms.Write(buffer, 0, bytes);
                    }

                    ms.Position = 0;
                }

                XmlTextReader x = null;

                try
                {
                    x = new XmlTextReader(ms);

                    while (x.Read())
                    {
                        switch (x.NodeType)
                        {
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.Whitespace:
                                break;

                            case XmlNodeType.Element:
                                switch (x.Name)
                                {
                                    case "RecentFiles": break;

                                    case "RecentFile":
                                        if (list.Count < max) list.Add(x.GetAttribute(0));
                                        break;

                                    default: Debug.Assert(false); break;
                                }
                                break;

                            case XmlNodeType.EndElement:
                                switch (x.Name)
                                {
                                    case "RecentFiles": return list;
                                    default: Debug.Assert(false); break;
                                }
                                break;

                            default:
                                Debug.Assert(false);
                                break;
                        }
                    }
                }
                finally
                {
                    if (x != null) x.Close();
                }
            }
            return list;
        }

        void Save(List<string> list, int max)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlTextWriter x = null;

                try
                {
                    x = new XmlTextWriter(ms, Encoding.UTF8);
                    if (x == null) { Debug.Assert(false); return; }

                    x.Formatting = Formatting.Indented;

                    x.WriteStartDocument();

                    x.WriteStartElement("RecentFiles");

                    foreach (string filepath in list)
                    {
                        x.WriteStartElement("RecentFile");
                        x.WriteAttributeString("Filepath", filepath);
                        x.WriteEndElement();
                    }

                    x.WriteEndElement();

                    x.WriteEndDocument();

                    x.Flush();

                    using (SmartStream ss = OpenStream(FileMode.Create))
                    {
                        ss.Stream.SetLength(0);

                        ms.Position = 0;

                        byte[] buffer = new byte[1 << 20];
                        for (; ; )
                        {
                            int bytes = ms.Read(buffer, 0, buffer.Length);
                            if (bytes == 0) break;
                            ss.Stream.Write(buffer, 0, bytes);
                        }
                    }
                }
                finally
                {
                    if (x != null) x.Close();
                }
            }
        }
    }
}
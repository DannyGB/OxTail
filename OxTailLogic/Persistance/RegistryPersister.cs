/*****************************************************************
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
    using System.Linq;
    using System.Text;
    using Microsoft.Win32;

    public class RegistryPersister : IPersist
    {
        public string RegistryKey { get; set; }

        public RegistryPersister()
        {
            RegistryKey =
                "Software\\" +
                ApplicationAttributes.CompanyName + "\\" +
                ApplicationAttributes.ProductName + "\\" +
                "RecentFileList";
        }

        public RegistryPersister(string key)
        {
            RegistryKey = key;
        }

        string Key(int i) { return i.ToString("00"); }

        public List<string> RecentFiles(int max)
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null) k = Registry.CurrentUser.CreateSubKey(RegistryKey);

            List<string> list = new List<string>(max);

            for (int i = 0; i < max; i++)
            {
                string filename = (string)k.GetValue(Key(i));

                if (String.IsNullOrEmpty(filename)) break;

                list.Add(filename);
            }

            return list;
        }

        public void InsertFile(string filepath, int max)
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null) Registry.CurrentUser.CreateSubKey(RegistryKey);
            k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);

            RemoveFile(filepath, max);

            for (int i = max - 2; i >= 0; i--)
            {
                string sThis = Key(i);
                string sNext = Key(i + 1);

                object oThis = k.GetValue(sThis);
                if (oThis == null) continue;

                k.SetValue(sNext, oThis);
            }

            k.SetValue(Key(0), filepath);
        }

        public void RemoveFile(string filepath, int max)
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null) return;

            for (int i = 0; i < max; i++)
            {
            again:
                string s = (string)k.GetValue(Key(i));
                if (s != null && s.Equals(filepath, StringComparison.CurrentCultureIgnoreCase))
                {
                    RemoveFile(i, max);
                    goto again;
                }
            }
        }

        void RemoveFile(int index, int max)
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (k == null) return;

            k.DeleteValue(Key(index), false);

            for (int i = index; i < max - 1; i++)
            {
                string sThis = Key(i);
                string sNext = Key(i + 1);

                object oNext = k.GetValue(sNext);
                if (oNext == null) break;

                k.SetValue(sThis, oNext);
                k.DeleteValue(sNext);
            }
        }
    }
}

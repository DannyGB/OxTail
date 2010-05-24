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

    public class FileHelper
    {
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
                FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
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
    }
}

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
                FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read);
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
    }
}

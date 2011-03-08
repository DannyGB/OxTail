using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OxTailHelpers
{
    public static class ClipboardHelper
    {
        /// <summary>
        /// Adds text to clipboard
        /// </summary>
        /// <param name="text">The text to add to the clipboard</param>
        public static void AddTextToClipboard(string text)
        {
            // See for try/catch explaination: http://www.switchonthecode.com/tutorials/wpf-tutorial-using-the-clipboard
            try
            {
                Clipboard.SetData(DataFormats.Text, text);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                System.Threading.Thread.Sleep(0);
                try
                {
                    Clipboard.SetData(DataFormats.Text, text);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    MessageBox.Show("Can't Access Clipboard");
                } 
            }
        }
    }
}

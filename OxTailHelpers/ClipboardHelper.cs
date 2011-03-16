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
                    MessageBox.Show(LanguageHelper.GetLocalisedText((System.Windows.Application.Current as IApplication), Constants.CANT_ACCESS_CLIPBOARD));
                } 
            }
        }
    }
}

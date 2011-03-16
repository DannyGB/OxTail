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
using System.Windows.Controls;
using OxTailHelpers;

namespace OxTail.Controls
{
    public class DisappearingTextTextBox : TextBox
    {
        public string ResourceFileKey { get; set; }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            CheckResourceFileKey();
            string val = GetStringFromResource(ResourceFileKey);

            if (this.Text == val)
            {
                this.Text = "";
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            CheckResourceFileKey();

            if (this.Text == string.Empty)
            {
                string val = GetStringFromResource(ResourceFileKey);
                this.Text = val;
            }

            base.OnMouseLeave(e);
        }

        private static string GetStringFromResource(string key)
        {
            string val = ResourceHelper.GetStringFromStringResourceFile(key);
            return val;
        }

        private void CheckResourceFileKey()
        {
            if (string.IsNullOrEmpty(ResourceFileKey))
            {
                throw new ArgumentException(LanguageHelper.GetLocalisedText((System.Windows.Application.Current as IApplication), Constants.RESOURCE_KEY_FILE_NOT_SET));
            }
        }
    }
}

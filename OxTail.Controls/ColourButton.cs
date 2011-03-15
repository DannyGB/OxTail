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

namespace OxTail.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    
    public class ColourButton : System.Windows.Controls.Button
    {
        public Color SelectedColour { get; set; }

        public Brush ColorBrush 
        {
            get
            {
                return new SolidColorBrush(this.SelectedColour);
            }
        }

        public bool? ShowColourSelectDialog()
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                SelectedColour = new SolidColorBrush(Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B)).Color;

                return true;
            }

            return false;
            
        }
    }
}

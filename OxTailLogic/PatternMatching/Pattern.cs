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

namespace OxTailLogic.PatternMatching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using System.ComponentModel;

    public class Pattern : INotifyPropertyChanged
    {
        public string StringPattern { get; set; }
        public Color Colour { get; set; }
        public bool? IgnoreCase { get; set; }

        public Pattern()
        {
        }

        public Pattern(string pattern, Color colour, bool? ignoreCase)
        {
            StringPattern = pattern;
            Colour = colour;
            IgnoreCase = ignoreCase;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}", this.StringPattern, this.Colour.ToString(), this.IgnoreCase.Value.ToString());

            return sb.ToString();
        }
    }
}

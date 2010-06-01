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

namespace OxTail.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.IO;
    using OxTail.Controls;
    using OxTail.Helpers;

    [Serializable]
    public class HighlightItem : INotifyPropertyChanged, IColourfulItem
    {
        private string _stringPattern;
        private Color _colour;
        private Color _backColour;

        public HighlightItem()
        {
        }

        public HighlightItem(string pattern, Color colour, Color backColour)
        {
            Pattern = pattern;
            ForeColour = colour;
            BackColour = backColour;
        }

        public string Pattern
        {
            get
            {
                return this._stringPattern;
            }

            set
            {
                this._stringPattern = value;
                OnPropertyChanged("StringPattern");

            }
        }

        public Color ForeColour
        {
            get
            {
                return this._colour;
            }

            set
            {
                this._colour = value;
                OnPropertyChanged("Colour");
            }
        }    

        public Color BackColour
        {
            get
            {
                return this._backColour;
            }

            set
            {
                this._backColour = value;
                OnPropertyChanged("Colour");
            }
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
            sb.AppendFormat("{0}", this.Pattern);

            return sb.ToString();
        }

        public static void SaveHighlights(BindingList<HighlightItem> patterns, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BindingList<HighlightItem>));
            FileHelper.SerializeToExecutableDirectory(filename, serializer, patterns);
        }

        public static BindingList<HighlightItem> LoadHighlights(string filename)
        {
            if (!File.Exists(filename))
            {
                return new BindingList<HighlightItem>();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(BindingList<HighlightItem>));
            return (BindingList<HighlightItem>)FileHelper.DeserializeFromExecutableDirectory(filename, serializer);
        }
    }
}

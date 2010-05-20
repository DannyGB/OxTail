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
    using System.Xml.Serialization;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.IO;

    [Serializable]
    public class Pattern : INotifyPropertyChanged
    {
        private string _stringPattern;
        private Color _colour;
        private bool? _ignoreCase;
        private Color _backColour;

        public Pattern()
        {
        }

        public Pattern(string pattern, Color colour, bool? ignoreCase, Color backColour)
        {
            StringPattern = pattern;
            Colour = colour;
            IgnoreCase = ignoreCase;
            BackColour = backColour;
        }

        public string StringPattern
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

        public Color Colour
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

        public bool? IgnoreCase
        {
            get
            {
                return this._ignoreCase;
            }

            set
            {
                this._ignoreCase = value;
                OnPropertyChanged("IgnoreCase");
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
            sb.AppendFormat("{0} {1}", this.StringPattern, this.IgnoreCase.Value.ToString());

            return sb.ToString();
        }

        public static void SaveHighlights(ObservableCollection<Pattern> patterns, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Pattern>));
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                serializer.Serialize(writer, patterns);
            }
        }

        public static ObservableCollection<Pattern> LoadHighlights(string filename)
        {
            if (!File.Exists(filename))
            {
                return new ObservableCollection<Pattern>();
            }
         
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Pattern>));

            FileStream s = new System.IO.FileStream(filename, FileMode.Open);
            using (XmlTextReader reader = new XmlTextReader(s))
            {
                return (ObservableCollection<Pattern>)serializer.Deserialize(reader);
            }
        }
    }
}

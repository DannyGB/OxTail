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

namespace OxTailHelpers
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
    using OxTail.Helpers;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Highlight item
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(HighlightItem))]
    public class HighlightItem : INotifyPropertyChanged, IColourfulItem, IComparable
    {
        private string _stringPattern;
        private Color _colour;
        private Color _backColour;
        private int _order;
        
        /// <summary>
        /// Initialise instance
        /// </summary>
        public HighlightItem()
        {
        }

        /// <summary>
        /// Initialise instance
        /// </summary>
        /// <param name="pattern">The regular expression pattern</param>
        /// <param name="colour">The fore color of the item</param>
        /// <param name="backColour">The back color of the item</param>
        public HighlightItem(string pattern, Color colour, Color backColour)
        {
            Pattern = pattern;
            ForeColour = colour;
            BackColour = backColour;
        }

        /// <summary>
        /// The regular expression pattern
        /// </summary>
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

        /// <summary>
        /// The fore color of the item
        /// </summary>
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

        /// <summary>
        /// The back color of the item
        /// </summary>
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

        /// <summary>
        /// The border color of the item
        /// </summary>
        public Color BorderColour { get; set; }

        /// <summary>
        /// The order of importance of the item
        /// </summary>
        public int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
                OnPropertyChanged("Order");
            }
        }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Override ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", this.Pattern);

            return sb.ToString();
        }

        /// <summary>
        /// Saves the highlight collection
        /// </summary>
        /// <param name="patterns">The collection of patterns</param>
        /// <param name="filename">The filename to save to</param>
        public static void SaveHighlights(HighlightCollection<HighlightItem> patterns, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(HighlightCollection<HighlightItem>), new Type[] { typeof(HighlightItem) });
            FileHelper.SerializeToExecutableDirectory(filename, serializer, patterns);
        }

        /// <summary>
        /// Load the collection of highlights
        /// </summary>
        /// <param name="filename">The filename to open</param>
        /// <returns>The highlight collection</returns>
        public static HighlightCollection<HighlightItem> LoadHighlights(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                return new HighlightCollection<HighlightItem>();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(HighlightCollection<HighlightItem>), new Type[]{ typeof(HighlightItem) });
            
            return (HighlightCollection<HighlightItem>)FileHelper.DeserializeFromExecutableDirectory(filename, serializer);
        }   
 
         #region IComparable

        /// <summary>
        /// Compare this to the <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">THe item to compare to</param>
        /// <returns>Comparision result</returns>
        public int CompareTo(object obj)
        {
            if (((HighlightItem)obj).Order == this.Order)
            {
                return 0;
            }

            if (((HighlightItem)obj).Order > this.Order)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        #endregion IComparable
    }
}

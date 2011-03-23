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
    using System.Windows.Media;

    [Serializable]
    public class HighlightedItem : IColourfulItem
    {
        private string _text;
        private Color _colour;
        private Color _backColour;

        public HighlightedItem()
        {
        }

        public HighlightedItem(string text, Color foreColour, Color backColour)
        {
            this.Text = text;
            this.ForeColour = foreColour;
            this.BackColour = backColour;
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
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
            }
        }

        public Color BorderColour { get; set; }

        public override string ToString()
        {
            return this.Text;
        }
    }
}

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
using System.Windows.Media;

namespace OxTailHelpers
{
    public class Constants
    {
        public Constants()
        {
        }

        public static readonly Color DEFAULT_BACKCOLOUR = Colors.White;
        public static readonly Color DEFAULT_FORECOLOUR = Colors.Black;
        public static readonly Color DEFAULT_BORDERCOLOUR = Colors.Violet;
        public static readonly Color DEFAULT_NULL_COLOUR = Color.FromArgb(0, 0, 0, 0);

        /// <summary>
        /// A SolidColorBrush that uses the DEFAULT_BORDERCOLOUR constant
        /// </summary>
        public static readonly Brush DEFAULT_BORDER_BRUSH = new SolidColorBrush(Constants.DEFAULT_BORDERCOLOUR);

        public const int DEFAULT_FOUND_RESULT_BORDER_SIZE = 2;

        public const string WINDOWS_NEWLINE = "\r\n";
        public const string UNIX_NEWLINE = "\n";
        public const string MAC_NEWLINE = "\r";
        public const string CARRIAGE_RETURN = "<cr>";
        public const string LINE_FEED = "<lf>";
        public const char NULL_TERMINATOR = '\0';
        public const string LINE_NUMBER_DIVIDER = ": ";

#if DEBUG
        public const int MAX_MRU_LIST = 5;
#else
        public const int MAX_MRU_LIST = 10;
#endif

    }
}

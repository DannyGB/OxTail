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
    }
}

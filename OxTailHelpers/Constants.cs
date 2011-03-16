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

#region Resource keys

        public const string UKNOWN_SEARCH_OPTION = "unknownSearchOption";
        public const string COMBO_CURRENT_DOCUMENT = "comboBoxItemCurrentDocument";
        public const string COMBO_ALL_DOCUMENTS = "comboBoxItemAllOpenDocuments";
        public const string FILE_NO_LONGER_EXISTS = "fileNoLongerExistsOnDisk";
        public const string REMOVE_FROM_RECENT_FILE_LIST = "removeFromRecentFileList";
        public const string FILE_TEXT_PATTERN = "filePatternText";
        public const string LOTS_OF_FILES_TEXT = "lotOfFilesText";
        public const string QUESTION = "question";
        public const string FILE_OPEN_LIMIT = "fileOpenLimit";
        public const string CANT_ACCESS_CLIPBOARD = "cantAccessClipboard";
        public const string CHOOSE_ITEM = "chooseItem";
        public const string ENTER_EXPRESSION_NAME = "enterExpressionName";
        public const string NO_SORTING_ALLOWED = "noSortingAllowed";
        public const string LINE_NUMBER_ERROR = "lineNumberError";
        public const string RESOURCE_KEY_FILE_NOT_SET = "resourceFileKeyNotSet";        

#endregion Resource keys

        public const string PART_ICON = "PART_Icon";
        public const string DEFAULT_FILE_OPEN_PATTERN = "*.log";
        public const string ALL_FILES_PATTERN = "*.*";
        public const string APPLICATION_NAME = "OxTail";
        public const string LAST_WRITE_TIME_SORT_HEADER = "LastWriteTime";
        public const string STRING_RESOURCE_URI = "/OxTail;component/Resources/StringResources.xaml";
        public const string SAVED_EXPRESSION_FILE_NAME = "SavedExpression.xml";
        public const string UNAMED = "UNNAMED";
        public const string EXAMPLE_REGEX_DATA_FILENAME = "OxTail.Controls.ExampleRegexData.txt";
        public const string RECENT_FILE_LIST_NAME = "RecentFileList.xml";
        public const string RECENT_FILES_MENUITEM_HEADER = "Recent Files";
        public const string HIGHLIGHT_ITEM_SORT_HEADER = "Order";        
        public const string ABOUT_PAGE_GPL_TEXT_RTF = "OxTail.Controls.AboutPageGplText.rtf";
        public const string CREDITS_AUTHORS = "Dan Beavon{0}Dave Wedgbury";
    }
}

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

namespace OxTail.Data.SQLite
{
    internal class Constants
    {
        #region Database Constants

        public const string APPSETTINGS_TABLE_DDL = @"CREATE TABLE IF NOT EXISTS AppSettings ( Code VARCHAR(20) PRIMARY KEY, Value VARCHAR(4000) )";
        public const string HIGHLIGHTITEMS_TABLE_DDL = @"CREATE TABLE IF NOT EXISTS HighlightItems ( ID INTEGER PRIMARY KEY AUTOINCREMENT, Pattern VARCHAR(4000), [Order] INTEGER, ForeColour INTEGER, BackColour INTEGER, BorderColour INTEGER);";
        public const string SAVEDEXPRESSIONS_TABLE_DDL = @"CREATE TABLE IF NOT EXISTS SavedExpressions ( ID INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(4000), Expression VARCHAR(4000));";
        public const string LAST_OPEN_FILES_TABLE_DDL = @"CREATE TABLE IF NOT EXISTS LastOpenFiles ( ID INTEGER PRIMARY KEY AUTOINCREMENT, [Filename] VARCHAR(4000));";
        public const string MOST_RECENT_FILES_TABLE_DDL = @"CREATE TABLE IF NOT EXISTS MostRecentFiles ( ID INTEGER PRIMARY KEY AUTOINCREMENT, [Filename] VARCHAR(4000));";
       
        public const string APPSETTINGS_SELECT_ALL = "SELECT * FROM AppSettings";
        public const string HIGHTLIGHTITEMS_SELECT_ALL = "SELECT * FROM HighlightItems";
        public const string SAVEDEXPRESSIONS_SELECT_ALL = "SELECT * FROM SavedExpressions";
        public const string LAST_OPEN_FILES_SELECT_ALL = "SELECT * FROM LastOpenFiles";
        public const string LAST_OPEN_FILES_DELETE_ALL = "DELETE FROM LastOpenFiles";

        public const string MOST_RECENT_FILES_SELECT_ALL = "SELECT * FROM MostRecentFiles";
        public const string MOST_RECENT_FILES_DELETE_ALL = "DELETE FROM MostRecentFiles";

        public const string DATABASE_NAME = @"\oxtail.db3";
        public const string DATA_SOURCE = @"Data Source={0}";

        #endregion Database Constants
    }
}

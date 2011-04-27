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
using System.Data.Common;
using System.Data.SQLite;
using OxTailHelpers;
using System.Windows.Forms;
using System.IO;

namespace OxTail.Data.SQLite
{
    public class SQLiteBase
    {
        protected DbConnection DbConnection { get; set; }
        protected SQLiteFactory Factory { get; set; }

        public SQLiteBase()
        {
            Factory = System.Data.SQLite.SQLiteFactory.Instance;
            DbConnection = Factory.CreateConnection();
            string path = Path.GetDirectoryName(Application.ExecutablePath) + Constants.DATABASE_NAME;
            DbConnection.ConnectionString = string.Format(Constants.DATA_SOURCE, path);

            if (!File.Exists(path))
            {
                SQLiteDatabaseHelper.CreateDatabaseFile(path);

                DbConnection.Open();
                DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable);

                try
                {
                    SQLiteDatabaseHelper.ExecuteNonQuery(Constants.APPSETTINGS_TABLE_DDL, DbConnection, Factory, trans);
                    SQLiteDatabaseHelper.ExecuteNonQuery(Constants.HIGHLIGHTITEMS_TABLE_DDL, DbConnection, Factory, trans);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new DatabaseCreateFailureException(path, "Failure creating database!", ex);
                }
                finally
                {
                    DbConnection.Close();
                }
            }
        }
    }
}

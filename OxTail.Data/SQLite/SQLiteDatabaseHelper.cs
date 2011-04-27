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
using System.Data.SQLite;
using System.Data.Common;
using System.Data;

namespace OxTail.Data.SQLite
{
    public static class SQLiteDatabaseHelper
    {
        internal static void CreateDatabaseFile(string filename)
        {
            SQLiteConnection.CreateFile(filename);
        }

        internal static void ExecuteNonQuery(string command, DbConnection conn, SQLiteFactory factory)
        {
            DbTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                conn.Open();

                ExecuteNonQuery(command, conn, factory, trans);
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }

        internal static void ExecuteNonQuery(string command, DbConnection conn, SQLiteFactory factory, DbTransaction trans)
        {
            using (DbDataAdapter adpt = factory.CreateDataAdapter())
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = trans;
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

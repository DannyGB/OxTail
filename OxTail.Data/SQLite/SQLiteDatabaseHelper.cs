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
            conn.Open();

            using (DbTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = command;
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
            }

            conn.Close();
        }
    }
}

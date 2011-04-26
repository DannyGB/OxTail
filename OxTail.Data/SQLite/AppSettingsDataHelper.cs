using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using OxTailHelpers;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using OxTailHelpers.Data;

namespace OxTail.Data.SQLite
{
    public class AppSettingsDataHelper : IAppSettingsData
    {
        private DbConnection DbConnection { get; set; }
        private SQLiteFactory Factory { get; set; }

        public AppSettingsDataHelper()
        {
            Factory = System.Data.SQLite.SQLiteFactory.Instance;
            DbConnection = Factory.CreateConnection();
            string path = Path.GetDirectoryName(Application.ExecutablePath) + Constants.DATABASE_NAME;
            DbConnection.ConnectionString = string.Format(Constants.DATA_SOURCE, path);

            if (!File.Exists(path))
            {
                SQLiteDatabaseHelper.CreateDatabaseFile(path);
                SQLiteDatabaseHelper.ExecuteNonQuery(Constants.APPSETTINGS_TABLE_DDL, DbConnection, Factory);
            }
        }

        public AppSettings ReadAppSettings()
        {
            AppSettings appSettings = new AppSettings();

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.APPSETTINGS_SELECT_ALL;
                        cmd.ExecuteNonQuery();
                        adpt.SelectCommand = cmd;

                        using (DbCommandBuilder bld = Factory.CreateCommandBuilder())
                        {
                            bld.DataAdapter = adpt;

                            using (DataTable tbl = new DataTable())
                            {
                                adpt.Fill(tbl);

                                foreach (DataRow row in tbl.Rows)
                                {
                                    appSettings[row[0].ToString()] = row[1].ToString();
                                }
                            }
                        }
                    }
                }
            }

            DbConnection.Close();

            return appSettings;
        }
        
        public int WriteAppSettings(AppSettings settings)
        {
            int retval = 0;

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.APPSETTINGS_SELECT_ALL;
                        cmd.ExecuteNonQuery();
                        adpt.SelectCommand = cmd;

                        using (DbCommandBuilder bld = Factory.CreateCommandBuilder())
                        {
                            bld.DataAdapter = adpt;

                            using (DataTable tbl = new DataTable())
                            {
                                adpt.Fill(tbl);
                                
                                foreach (string key in settings.Keys)
                                {
                                    bool found = false;

                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        if(row[0].ToString() == key)
                                        {
                                            row[1] = settings[key];
                                            found = true;
                                            break;
                                        }
                                    }

                                    if(!found)
                                    {
                                        DataRow r = tbl.NewRow();
                                        r[0] = key;
                                        r[1] = settings[key];
                                        tbl.Rows.Add(r);
                                    }
                                }

                                retval = adpt.Update(tbl);
                                trans.Commit();
                            }
                        }
                    }
                }
            }

            DbConnection.Close();

            return retval;
        }        
    }
}

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
using OxTailHelpers;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using OxTailHelpers.Data;

namespace OxTail.Data.SQLite
{
    public class AppSettingsDataHelper : SQLiteBase, IAppSettingsData
    {
        public AppSettingsDataHelper()
        {            
        }

        public IAppSettings ReadAppSettings(IAppSettings settings)
        {
            IAppSettings appSettings = settings;

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
        
        public int WriteAppSettings(IAppSettings settings)
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

                                try
                                {
                                    retval = adpt.Update(tbl);
                                    trans.Commit();
                                }
                                catch (Exception)
                                {
                                    trans.Rollback();
                                    throw new DatabaseWriteException();
                                }
                                finally
                                {
                                    DbConnection.Close();
                                }
                            }
                        }
                    }
                }
            }

            return retval;
        }        
    }
}

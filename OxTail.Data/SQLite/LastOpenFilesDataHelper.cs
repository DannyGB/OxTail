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
using OxTailHelpers.Data;
using OxTailHelpers;
using System.Data.Common;
using System.Data;

namespace OxTail.Data.SQLite
{
    public class LastOpenFilesDataHelper : SQLiteBase, ILastOpenFilesData
    {
        public List<LastOpenFiles> Read()
        {
            List<LastOpenFiles> items = new List<LastOpenFiles>();

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.LAST_OPEN_FILES_SELECT_ALL;
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
                                    LastOpenFiles item = new LastOpenFiles()
                                    {
                                        ID = int.Parse(row[0].ToString()),
                                        Filename = row[1].ToString(),
                                    };

                                    items.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            DbConnection.Close();

            return items;
        }

        public List<LastOpenFiles> Write(List<LastOpenFiles> files)
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
                        cmd.CommandText = Constants.LAST_OPEN_FILES_SELECT_ALL;
                        cmd.ExecuteNonQuery();
                        adpt.SelectCommand = cmd;

                        using (DbCommandBuilder bld = Factory.CreateCommandBuilder())
                        {
                            bld.DataAdapter = adpt;

                            using (DataTable tbl = new DataTable())
                            {
                                adpt.Fill(tbl);
                                DataRow[] existingRows = tbl.Select(string.Empty, string.Empty, DataViewRowState.OriginalRows);

                                // Deletes rows
                                bool found;
                                foreach (DataRow row in existingRows)
                                {
                                    found = false;
                                    foreach (LastOpenFiles item in files)
                                    {
                                        if (item.ID > 0 && item.ID == int.Parse(row[0].ToString()))
                                        {
                                            found = true;
                                            continue;
                                        }
                                    }

                                    if (!found)
                                    {
                                        row.Delete();
                                    }
                                }

                                foreach (LastOpenFiles item in files)
                                {
                                    found = false;

                                    foreach (DataRow row in existingRows)
                                    {
                                        if (row.RowState == DataRowState.Deleted)
                                        {
                                            found = true;
                                            break;
                                        }

                                        // Updates existing rows
                                        if (int.Parse(row[0].ToString()) == item.ID)
                                        {
                                            row.BeginEdit();
                                            row[1] = item.Filename;
                                            row.EndEdit();

                                            found = true;
                                            break;
                                        }
                                    }

                                    // Inserts new rows
                                    if (!found)
                                    {
                                        DataRow r = tbl.NewRow();

                                        r[1] = item.Filename;

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

            return this.Read();
        }

        public void Clear()
        {
            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {

                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.LAST_OPEN_FILES_DELETE_ALL;
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw new DatabaseWriteException("Could not write to database");
                }
                finally
                {
                    DbConnection.Close();
                }
            }

        }
    }
}

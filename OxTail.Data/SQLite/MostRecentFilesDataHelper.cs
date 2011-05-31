﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers.Data;
using System.Data.Common;
using System.Data;
using OxTailHelpers;

namespace OxTail.Data.SQLite
{
    public class MostRecentFilesDataHelper : SQLiteBase, IMostRecentFilesData
    {
        public List<OxTail.Helpers.File> Read()
        {
            List<OxTail.Helpers.File> items = new List<OxTail.Helpers.File>();

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.MOST_RECENT_FILES_SELECT_ALL;
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
                                    OxTail.Helpers.File item = new OxTail.Helpers.File()
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

        public List<OxTail.Helpers.File> Write(List<OxTail.Helpers.File> files)
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
                        cmd.CommandText = Constants.MOST_RECENT_FILES_SELECT_ALL;
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
                                    foreach (OxTail.Helpers.File item in files)
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

                                foreach (OxTail.Helpers.File item in files)
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
                                            row[1] = item.Filename;

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
                        cmd.CommandText = Constants.MOST_RECENT_FILES_DELETE_ALL;
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

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
using System.Windows.Media;

namespace OxTail.Data.SQLite
{
    public class HighlightDataHelper : SQLiteBase, IHighlightItemData
    {

        public HighlightCollection<HighlightItem> Read()
        {
            HighlightCollection<HighlightItem> highlights = new HighlightCollection<HighlightItem>();

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.HIGHTLIGHTITEMS_SELECT_ALL;
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
                                    HighlightItem item = new HighlightItem()
                                    {
                                        ID = int.Parse(row[0].ToString()),
                                        Pattern = row[1].ToString(),
                                        Order = int.Parse(row[2].ToString()),
                                    };

                                    //HACK: System.Drawing.Color allows storing of the colour as one integer, gonna use this to populate DB and re-hydrate back in code. Easier!

                                    System.Drawing.Color tempCol = System.Drawing.Color.FromArgb(int.Parse(row[3].ToString()));
                                    Color tempCol2 = Color.FromArgb(tempCol.A, tempCol.R, tempCol.G, tempCol.B);
                                    item.ForeColour = tempCol2;

                                    tempCol = System.Drawing.Color.FromArgb(int.Parse(row[4].ToString()));
                                    tempCol2 = Color.FromArgb(tempCol.A, tempCol.R, tempCol.G, tempCol.B);
                                    item.BackColour = tempCol2;

                                    tempCol = System.Drawing.Color.FromArgb(int.Parse(row[5].ToString()));
                                    tempCol2 = Color.FromArgb(tempCol.A, tempCol.R, tempCol.G, tempCol.B);
                                    item.BorderColour = tempCol2;

                                    highlights.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            DbConnection.Close();

            return highlights;
        }

        public HighlightCollection<HighlightItem> Write(HighlightCollection<HighlightItem> items)
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
                        cmd.CommandText = Constants.HIGHTLIGHTITEMS_SELECT_ALL;
                        cmd.ExecuteNonQuery();
                        adpt.SelectCommand = cmd;

                        using (DbCommandBuilder bld = Factory.CreateCommandBuilder())
                        {
                            bld.DataAdapter = adpt;

                            using (DataTable tbl = new DataTable())
                            {
                                adpt.Fill(tbl);

                                foreach (HighlightItem item in items)
                                {
                                    bool found = false;

                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        if (int.Parse(row[0].ToString()) == item.ID)
                                        {
                                            row[1] = item.Pattern;
                                            row[2] = item.Order;

                                            System.Drawing.Color tempCol = System.Drawing.Color.FromArgb(item.ForeColour.A, item.ForeColour.R, item.ForeColour.G, item.ForeColour.B);
                                            row[3] = tempCol.ToArgb();

                                            tempCol = System.Drawing.Color.FromArgb(item.BackColour.A, item.BackColour.R, item.BackColour.G, item.BackColour.B);
                                            row[4] = tempCol.ToArgb();

                                            tempCol = System.Drawing.Color.FromArgb(item.BorderColour.A, item.BorderColour.R, item.BorderColour.G, item.BorderColour.B);
                                            row[5] = tempCol.ToArgb();

                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        DataRow r = tbl.NewRow();

                                        r[1] = item.Pattern;
                                        r[2] = item.Order;

                                        System.Drawing.Color tempCol = System.Drawing.Color.FromArgb(item.ForeColour.A, item.ForeColour.R, item.ForeColour.G, item.ForeColour.B);
                                        r[3] = tempCol.ToArgb();

                                        tempCol = System.Drawing.Color.FromArgb(item.BackColour.A, item.BackColour.R, item.BackColour.G, item.BackColour.B);
                                        r[4] = tempCol.ToArgb();

                                        tempCol = System.Drawing.Color.FromArgb(item.BorderColour.A, item.BorderColour.R, item.BorderColour.G, item.BorderColour.B);
                                        r[5] = tempCol.ToArgb();

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
    }
}

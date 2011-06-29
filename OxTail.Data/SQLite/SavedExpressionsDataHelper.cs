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
using System.Collections.ObjectModel;
using OxTailHelpers;
using System.Data.Common;
using System.Data;

namespace OxTail.Data.SQLite
{
    public class SavedExpressionsDataHelper : SQLiteBase, ISavedExpressionsData
    {
        private readonly IExpressionFactory ExpressionFactory;

        public SavedExpressionsDataHelper(IExpressionFactory expressionFactory)
        {
            this.ExpressionFactory = expressionFactory;
        }

        public System.Collections.ObjectModel.ObservableCollection<IExpression> Read(ObservableCollection<IExpression> exprs)
        {
            exprs.Clear();
            ObservableCollection<IExpression> expressions = exprs;

            DbConnection.Open();

            using (DbTransaction trans = DbConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                using (DbDataAdapter adpt = Factory.CreateDataAdapter())
                {
                    using (DbCommand cmd = DbConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = Constants.SAVEDEXPRESSIONS_SELECT_ALL;
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
                                    IExpression expr = this.ExpressionFactory.CreateFile(int.Parse(row[0].ToString()), row[2].ToString(), row[1].ToString());
                                    expressions.Add(expr);
                                }
                            }
                        }
                    }
                }
            }

            DbConnection.Close();

            return expressions;
        }

        public ObservableCollection<IExpression> Write(ObservableCollection<IExpression> items)
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
                        cmd.CommandText = Constants.SAVEDEXPRESSIONS_SELECT_ALL;
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
                                    foreach (IExpression item in items)
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

                                foreach (IExpression item in items)
                                {
                                    found = false;

                                    foreach (DataRow row in existingRows)
                                    {
                                        if (row.RowState == DataRowState.Deleted)
                                        {
                                            found = true;
                                            break;
                                        }

                                        if (int.Parse(row[0].ToString()) == item.ID)
                                        {
                                            row[1] = item.Name;
                                            row[2] = item.Text;

                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        DataRow r = tbl.NewRow();

                                        r[1] = item.Name;
                                        r[2] = item.Text;

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

            return this.Read(items);
        }
    }
}

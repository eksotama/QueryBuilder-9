using Core.Extensions;
using QueryBuilder.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using QueryBuilder.Helper;

namespace QueryBuilder.Service
{
    public class DBService
    {
        //================= Member variables ============//
        private string connectionString;
        private SqlConnection sqlConnection;
        private SqlDataAdapter daTable;
        private SqlDataAdapter daTableFunction;
        private SqlDataAdapter daView;
        private SqlDataAdapter daColumn;
        private SqlDataAdapter daColumnFunction;

        //================ Getter ======================//
        public SqlConnection SqlConnection
        {
            get
            {
                return sqlConnection;
            }
        }

        //=============== Constructor =================//
        public DBService(string connectionString)
        {
            this.connectionString = connectionString;
            sqlConnection = new SqlConnection(connectionString);
            daTable = new SqlDataAdapter();
            daTableFunction = new SqlDataAdapter();
            daView = new SqlDataAdapter();
            daColumn = new SqlDataAdapter();
            daColumnFunction = new SqlDataAdapter();
        }

        //=============== Reinit sqlconnection and open connection ========//
        public void Connect()
        {
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());
            }          
        }

        //============== Close sqlconnection to prevent leaking ===========//
        public void Close()
        {
            try
            {
                sqlConnection.Close();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                sqlConnection.Dispose();
            }
        }

        //============= Get list of available tables in a database =======//
        public List<QTable> GetTables()
        {
            List<QTable> tables = new List<QTable>();
            try
            {
                SqlCommand cmdSelect = new SqlCommand();
                using (cmdSelect)
                {
                    cmdSelect.CommandText = "SELECT name from sys.tables ";
                    cmdSelect.CommandType = CommandType.Text;
                    cmdSelect.Connection = sqlConnection;
                }
                daTable.SelectCommand = cmdSelect;
                DataSet dsTable = new DataSet();
                daTable.Fill(dsTable, "Tables");
                var data = dsTable.Tables["Tables"].AsEnumerable().Select(r => new QTable
                {
                    Name = r.Field<string>("name")
                });
                tables = data.ToList();
            }
            catch (SqlException se)
            {
                Debug.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            //UNCOMMENT CODE BELOW TO USE CONNECTED LAYER
            //using (sqlConnection)
            //{
            //    Connect();
            //    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.tables ", this.SqlConnection))
            //    {
            //        using (IDataReader dr = cmd.ExecuteReader())
            //        {
            //            tables = dr.Select(t => new QTable(dr[0].ToString())).ToList();
            //        }
            //    }
            //}
            //this.Close();

            return tables;
        }

        //============= Get columns and their data type of a table/view/table_function =======//
        public List<QColumn> GetColumns(string table)
        {
            List<QColumn> columns = new List<QColumn>();
            try
            {
                SqlCommand cmdSelect = new SqlCommand();
                using (cmdSelect)
                {
                    cmdSelect.CommandText = "SELECT column_name, data_type FROM  information_schema.COLUMNS WHERE table_name= '" + table + "'";
                    cmdSelect.CommandType = CommandType.Text;
                    cmdSelect.Connection = sqlConnection;
                    cmdSelect.CommandTimeout = 180;
                }
                daColumn.SelectCommand = cmdSelect;
                DataSet dsColumn = new DataSet();
                daColumn.Fill(dsColumn, "Columns");
                var data = dsColumn.Tables["Columns"].AsEnumerable().Select(r => new QColumn
                {
                    Name = r.Field<string>("column_name"),
                    DataType = r.Field<string>("data_type")
                });
                columns = data.ToList();
            }
            catch (SqlException se)
            {
                Debug.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            
            //UNCOMMENT CODE BELOW TO USE CONNECTED LAYER
            //using (sqlConnection)
            //{
            //    Connect();
            //    using (SqlCommand cmd = new SqlCommand("SELECT column_name, data_type FROM  information_schema.COLUMNS WHERE table_name= '" + table + "'", sqlConnection))
            //    {
            //        using (IDataReader dr = cmd.ExecuteReader())
            //        {
            //            columns = dr.Select(t => new QColumn(dr.GetValue(0).ToString(), dr.GetValue(1).ToString())).ToList();
            //        }
            //    }
            //}
            //this.Close();

            return columns;
        }

        //============= Get list of available views in a database =======//
        public List<QView> GetViews()
        {
            List<QView> views = new List<QView>();
            try
            {
                SqlCommand cmdSelect = new SqlCommand();
                using (cmdSelect)
                {
                    cmdSelect.CommandText = "SELECT name from sys.views ";
                    cmdSelect.CommandType = CommandType.Text;
                    cmdSelect.Connection = sqlConnection;
                }
                daView.SelectCommand = cmdSelect;
                DataSet dsView = new DataSet();
                daView.Fill(dsView, "Views");
                var data = dsView.Tables["Views"].AsEnumerable().Select(r => new QView
                {
                    Name = r.Field<string>("name")
                });
                views = data.ToList();
            }
            catch (SqlException se)
            {
                Debug.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            
            //UNCOMMENT CODE BELOW TO USE CONNECTED LAYER
            //using (this.SqlConnection)
            //{
            //    Connect();
            //    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.views ", this.SqlConnection))
            //    {
            //        using (IDataReader dr = cmd.ExecuteReader())
            //        {
            //            views = dr.Select(t => new QView(dr[0].ToString())).ToList();
            //        }
            //    }
            //}
            //this.Close();

            return views;
        }

        //============= Get list of available tables returned by a user defined function ===============================//
        public List<QTableFunction> GetTablesFunction()
        {
            List<QTableFunction> tables = new List<QTableFunction>();
            try
            {
                SqlCommand cmdSelect = new SqlCommand();
                using (cmdSelect)
                {
                    cmdSelect.CommandText = "SELECT name FROM sys.objects where RIGHT(type_desc,8) = 'function' " +
                                                        "and type_desc != 'SQL_SCALAR_FUNCTION'";
                    cmdSelect.CommandType = CommandType.Text;
                    cmdSelect.Connection = sqlConnection;
                }
                daTableFunction.SelectCommand = cmdSelect;
                DataSet dsTableFunction = new DataSet();
                daTableFunction.Fill(dsTableFunction, "TableFunctions");
                var data = dsTableFunction.Tables["TableFunctions"].AsEnumerable().Select(r => new QTableFunction
                {
                    Name = r.Field<string>("name")
                });
                tables = data.ToList();
            }
            catch (SqlException se)
            {
                Debug.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            
            //UNCOMMENT CODE BELOW FOR USING CONNECTED LAYER
            //using (this.SqlConnection)
            //{
            //    Connect();
            //    using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.objects where RIGHT(type_desc,8) = 'function' " +
            //                                        "and type_desc != 'SQL_SCALAR_FUNCTION'", this.SqlConnection))
            //    {
            //        using (IDataReader dr = cmd.ExecuteReader())
            //        {
            //            tables = dr.Select(t => new QTableFunction(dr[0].ToString())).ToList();
            //        }
            //    }
            //}
            //this.Close();

            return tables;
        }

        //============= Get list of available columns of a table returned by a user defined function ===================//
        public List<QColumn> GetColumnsFromTableFunction(string table)
        {
            List<QColumn> columns = new List<QColumn>();
            try
            {
                SqlCommand cmdSelect = new SqlCommand();
                using (cmdSelect)
                {
                    cmdSelect.CommandText = "SELECT name, xtype from sys.syscolumns " +
                                                                "where id in (select id from sysobjects where " +
                                                                "name = '" + table + "')" +
                                                                "and number != 1";
                    cmdSelect.CommandType = CommandType.Text;
                    cmdSelect.Connection = sqlConnection;
                    cmdSelect.CommandTimeout = 180;
                }
                daColumnFunction.SelectCommand = cmdSelect;
                DataSet dsColumn = new DataSet();
                daColumnFunction.Fill(dsColumn, "Columns");
                DataTableReader dtReader = dsColumn.CreateDataReader();
                while (dtReader.Read())
                {
                    string colname = dtReader.GetValue(0).ToString();
                    string xtype = dtReader.GetValue(1).ToString();
                    string datatype = "";
                    if (xtype == "48"
                            || xtype == "52"
                            || xtype == "56"
                            || xtype == "59"
                            || xtype == "60"
                            || xtype == "62"
                            || xtype == "104"
                            || xtype == "106"
                            || xtype == "108"
                            || xtype == "122"
                            || xtype == "127"
                            || xtype == "173"
                            || xtype == "56"
                            )
                    {
                        datatype = "decimal";
                    }
                    else
                    {
                        datatype = "varchar";
                    }
                    QColumn column = new QColumn(colname, datatype);
                    columns.Add(column);
                }
            }
            catch (SqlException se)
            {
                Debug.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
                 
            //UNCOMMENT THE CODE BELOW FOR USING CONNECTED LAYER
            //using (this.SqlConnection)
            //{
            //    Connect();
            //    using (SqlCommand cmd = new SqlCommand("SELECT name, xtype from sys.syscolumns " +
            //                                        "where id in (select id from sysobjects where " +
            //                                        "name = '" + table + "')" +
            //                                        "and number != 1", this.SqlConnection))
            //    {
            //        using (IDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                string colname = dr.GetValue(0).ToString();
            //                string xtype = dr.GetValue(1).ToString();
            //                string datatype = "";
            //                if (xtype == "48"
            //                        || xtype == "52"
            //                        || xtype == "56"
            //                        || xtype == "59"
            //                        || xtype == "60"
            //                        || xtype == "62"
            //                        || xtype == "104"
            //                        || xtype == "106"
            //                        || xtype == "108"
            //                        || xtype == "122"
            //                        || xtype == "127"
            //                        || xtype == "173"
            //                        || xtype == "56"
            //                        )
            //                {
            //                    datatype = "decimal";
            //                }
            //                else
            //                {
            //                    datatype = "varchar";
            //                }
            //                QColumn column = new QColumn(colname, datatype);
            //                columns.Add(column);
            //            }
            //        }
            //    }
            //}
            //this.Close();

            return columns;
        }
    }
}

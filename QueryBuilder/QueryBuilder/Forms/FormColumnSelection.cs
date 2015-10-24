using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QueryBuilder.Service;
using QueryBuilder.Model;
using QueryBuilder.CustomControl;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using Core.Extensions;
//using QueryBuilder.Helper;

namespace QueryBuilder.Forms
{
    public partial class FormColumnSelection : Form
    {
        private string selectedDatabase; //a string for current selected database
        private string connectionString; //a string for the connection string
        private DBService service; //a DAL for services dealing with the database

        private QTable table;
        private QView view;
        private QTableFunction tableFunction;
        private string tableName;
        private string columnName;
        private string dataType;
        private string alias;

        #region "SQL Fields"
        private Dictionary<string, List<QColumn>> sqlFields = new Dictionary<string, List<QColumn>>();
        private List<QColumn> selectedColumns = new List<QColumn>();
        private List<QColumn> groupByColumns = new List<QColumn>();
        private List<QColumn> aggregatedColumns = new List<QColumn>();
        private List<string> selectedTables = new List<string>();
        private List<QCriteria> selectedCriterias = new List<QCriteria>();
        private List<QCriteria> selectedHavingCriterias = new List<QCriteria>();
        private string sqlStatement;
        //property for sqlFields, selectedColumns and selectedTables
        private int index; //used for the order of selected column
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }
        public List<QColumn> SelectedColumns
        {
            get
            {
                return selectedColumns;
            }
            set
            {
                selectedColumns = value;
            }
        }
        public List<string> SelectedTables
        {
            get
            {
                return selectedTables;
            }
            set
            {
                selectedTables = value;
            }
        }
        public List<QColumn> GroupByColumns
        {
            get
            {
                return groupByColumns;
            }
            set
            {
                groupByColumns = value;
            }
        }
        public List<QColumn> AggregatedColumns
        {
            get
            {
                return aggregatedColumns;
            }
            set
            {
                aggregatedColumns = value;
            }
        }
        public Dictionary<string, List<QColumn>> SQLFields
        {
            get
            {
                return sqlFields;
            }
        }
        #endregion

        private DataTable dt = new DataTable(); //data table for grid view

        private enum Aggregates
        {
            NONE = 0,
            AVG,
            COUNT,
            MAX,
            MIN,
            SUM,
            LEN
        }

        //================================================================================//
        //=                                                                              =//
        //=      Constructors and background work for loading splash screen              =//
        //=                                                                              =//
        //================================================================================//

        /// <summary>
        /// Default constructor to help with generate a minimum window form
        /// </summary>
        public FormColumnSelection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// An overloading constructor that accept a selected database and a connection string
        /// </summary>
        /// <param name="selectedDatabase">This is passed from the FormConectServer. We need this for our query statement that query the available tables in a database</param>
        /// <param name="connectionString">This is a entire connection string. We need this to make a connection to our server</param>
        public FormColumnSelection(string selectedDatabase, string connectionString)
        {
            InitializeComponent();
            //begin showing the splashing screen
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync();
            //load the real thing
            this.selectedDatabase = selectedDatabase;
            this.connectionString = connectionString;
            service = new DBService(connectionString);
            onLoadForm(); //end load real thing
            bw.CancelAsync(); //close the splash screen
        }

        /// <summary>
        /// Private method for loading splashing screen
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">DoWorkEventArgs</param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            FormSplash fs = new FormSplash();
            fs.Show();
            while (!worker.CancellationPending)
            {
                Thread.Sleep(1000);
            }
            if (worker.CancellationPending)
            {
                fs.Close();
                e.Cancel = true;
            }
        }

        //======== Load all available tables into the treeview, and generate datagridview when we call our constructor =======//
        private void onLoadForm()
        {
            populateTreeView();
            generateColumnHeader(dgvSelectedColumns);
        }

        //==========================================================================================//
        //=                                                                                        =//
        //=      Reset everything when user hit Reset: reset control, and collections              =//
        //=                                                                                        =//
        //==========================================================================================//

        //change warning message, etc based on user selection
        private void modifyControl()
        {
            if (selectedTables.Count > 1)
            {
                lblJoinReminder.Text = "!ATTENTION: you have selected more than 1 table, a join is recommended.";
                lblJoinReminder.ForeColor = Color.Red;
                lblJoinReminder.Visible = true;
                btnJoin.Visible = true;
            }
            else
            {
                lblJoinReminder.Text = "";
                lblJoinReminder.Visible = false;
                btnJoin.Visible = false;
            }
        }

        //reset all control on form back to original state
        private void resetControl()
        {
            txbSqlStatement.Text = "";
            lblJoinReminder.Text = "";
            lblJoinReminder.Visible = false;
            btnJoin.Visible = false;
            TreeNodeCollection nodes = tvAvailableColumns.Nodes;
            foreach (TreeNode node in nodes)
            {
                if (node.Tag.GetType() == typeof(QTable))
                {
                    node.ForeColor = Color.Blue;
                }
                else if (node.Tag.GetType() == typeof(QTableFunction))
                {
                    node.ForeColor = Color.OrangeRed;
                }
                else
                {
                    node.ForeColor = Color.Green;
                }
                clearRecursive(node);
            }

            dt.Rows.Clear();
        }

        //recursively reset the color child nodes
        private void clearRecursive(TreeNode node)
        {
            foreach (TreeNode n in node.Nodes)
            {
                n.ForeColor = Color.Black;
                clearRecursive(n);
            }
        }

        private void resetSelectedTableAndColumn()
        {
            sqlFields.Clear();
            selectedTables.Clear();
            selectedColumns.Clear();
            groupByColumns.Clear();
            aggregatedColumns.Clear();
            sqlStatement = "";
            index = 0;
        }

        private void resetEverything()
        {
            resetControl();
            resetSelectedTableAndColumn();
        }

        //==========================================================================================//
        //=                                                                                        =//
        //=      Populate data for list of selected tables, selected columns, group by columns     =//
        //=                                                                                        =//
        //==========================================================================================//

        //====== Get all keys from dictionary. In this case, it is list of table in dictionary =====/
        private void populateSelectedTableList()
        {
            if (sqlFields.Count != 0)
            {
                selectedTables = sqlFields.Keys.ToList();
            }
        }

        //========== Get all columns associated with a table name from dictionary and put into the list ====/
        private void populateSelectedColumnList()
        {
            if (sqlFields.Count != 0)
            {
                foreach (KeyValuePair<string, List<QColumn>> dict in sqlFields)
                {
                    foreach (QColumn entry in dict.Value)
                    {
                        if (!selectedColumns.Contains(entry))
                        {
                            selectedColumns.Add(entry);
                        }
                            
                    }
                }
            }
        }

        //========= Get all columns that need to be put in group by based on aggregate columns
        private void bindGroupColumns()
        {
            aggregatedColumns.RemoveAll(c => c.Aggregate.Name.Equals("LEN"));
            if (aggregatedColumns.Count > 0)
            {
                groupByColumns = selectedColumns.Except(aggregatedColumns).ToList();              
            }
            else
            {
                groupByColumns.Clear();
            }
        }

        //==================================================================================//
        //=                                                                                =//
        //=         button clicking event for Review, Join, Reset buttons                  =//
        //=                                                                                =//
        //==================================================================================//

        private void btnReview_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("There are " + selectedTables.Count + " tables in list");
            foreach (var t in selectedTables)
            {
                Debug.WriteLine("Table: " + t + " in tables list");
            }
            Debug.WriteLine("There are " + selectedColumns.Count + " in list");
            foreach (QColumn c in selectedColumns)
            {
                Debug.WriteLine(c.Name + " at: " + c.Index + " has aggregate: " + c.Aggregate + " has having clause: " + c.HavingCriterias.Count());
            }
            Debug.WriteLine("There are " + groupByColumns.Count + " in group by list");
            foreach(QColumn c in groupByColumns) {
                Debug.WriteLine("group by " + c.Name + " " + c.Index);
            }
            Debug.WriteLine("There are " + aggregatedColumns.Count + " in aggregated list");
            foreach (QColumn c in aggregatedColumns)
            {
                Debug.WriteLine("aggregate " + c.Name + " " + c.Index);
            }
            groupByColumns = selectedColumns.Except(aggregatedColumns).ToList();
            Debug.WriteLine("After remove intersection, there are " + groupByColumns.Count + " need to be grouped");
            foreach (QColumn c in groupByColumns)
            {
                Debug.WriteLine("Grouping " + c.Name + " " + c.Index);
            }
        }  

        private void btnJoin_Click(object sender, EventArgs e)
        {
            FormJoin frmJoin = new FormJoin(selectedTables, connectionString);
            frmJoin.ShowDialog();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            resetEverything();
        }

        //==================================================================================//
        //=                                                                                =//
        //=        Generate each part, then combine into one big SQL Statement             =//
        //=                                                                                =//
        //==================================================================================//

        //============== generate entire sql statement ========================/
        private void generateSQLStatement()
        {
            modifyControl();
            txbSqlStatement.Clear();
            //obtain list of tables that user clicks
            StringBuilder tableFields = generateTableStatement();
            //obtain list columns that user clicks
            StringBuilder columnFields = generateColumnStatement();
            //obtain list of criteria
            StringBuilder whereClause = new StringBuilder();
            if (generateWhereClause().ToString().Length >= 6)
                whereClause = generateWhereClause();
            //obtain list of columns group by
            StringBuilder groupbyFields = generateGroupByStatement();
            //obtain list of having clause
            StringBuilder havingClause = new StringBuilder();
            if (generateHavingClause().ToString().Length > 7)
            {
                havingClause = generateHavingClause();
            }
            //generate final select statement
            if (tableFields.Length == 0)
            {
                sqlStatement = "";
            }
            else
            {
                sqlStatement = "SELECT " + columnFields.ToString() + System.Environment.NewLine +
                                "FROM " + tableFields.ToString() + System.Environment.NewLine +
                                (whereClause.ToString().Length > 6 ? whereClause + System.Environment.NewLine + 
                                                                    groupbyFields.ToString() + System.Environment.NewLine :
                                groupbyFields.ToString() + System.Environment.NewLine) +
                                havingClause;
            }
            txbSqlStatement.Text = sqlStatement;
        }

        //============== generate 'SELECT <column>' part of sql statement==================/
        private StringBuilder generateColumnStatement()
        {
            StringBuilder columnFields = new StringBuilder();
            for (int i = 0; i < selectedColumns.Count; i++)
            {
                if (selectedColumns.Count == 1) //only 1 selected column
                {
                    if (string.IsNullOrEmpty(selectedColumns.ElementAt(i).Alias)) //if a column does not have an alias, display column name
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) //if a column has aggregate, display aggregate
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                        }
                        else //if column does not have aggregate, just display column 
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());                     
                        }
                    }
                    else //if a column has an alias, display: [column] as [alias]
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) //if a column has aggregate, display it
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                            columnFields.Append(" AS [");
                            columnFields.Append(selectedColumns.ElementAt(i).Alias);
                            columnFields.Append("]");
                        }
                        else //if column does not have aggregate, display column with its alias
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString() + " AS [" + selectedColumns.ElementAt(i).Alias + "]");
                        }
                    }
                }
                else if (i == selectedColumns.Count - 1) //last selected column 
                {
                    if (string.IsNullOrEmpty(selectedColumns.ElementAt(i).Alias)) //if a column does not have alias
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) //a column has aggregate
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                        }
                        else //a column does not have aggregate and alias, only display column name
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                        }                    
                    }
                    else //if column has alias
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) //a column has aggregate and alias, display aggregate with its alias
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                            columnFields.Append(" AS [");
                            columnFields.Append(selectedColumns.ElementAt(i).Alias);
                            columnFields.Append("]");
                        }
                        else //column does not have aggregate but has alias
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString() + " AS [" + selectedColumns.ElementAt(i).Alias + "]");
                        }
                    }
                }
                else //selected column in the between the first and the last -- need comma
                {
                    if (string.IsNullOrEmpty(selectedColumns.ElementAt(i).Alias)) //no alias
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) // has aggregate, display aggregate and column name
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                        }
                        else //column does not have aggregate
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());                          
                        }
                        columnFields.Append(", ");
                        columnFields.Append(Environment.NewLine);
                    }
                    else //has alias
                    {
                        if (selectedColumns.ElementAt(i).Aggregate != null) //has aggregate, display aggregate and alias
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).Aggregate);
                            columnFields.Append("(");
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(")");
                            columnFields.Append(" AS [" + selectedColumns.ElementAt(i).Alias + "]");
                        }
                        else //column does not have aggregate, display column and its alias
                        {
                            columnFields.Append(selectedColumns.ElementAt(i).ToString());
                            columnFields.Append(" AS [" + selectedColumns.ElementAt(i).Alias + "]");
                        }                
                        columnFields.Append(", ");
                        columnFields.Append(Environment.NewLine);
                    }
                }
            }
            return columnFields;
        }

        //========== generate 'FROM <table>' part of sql statement ========================/
        private StringBuilder generateTableStatement()
        {
            StringBuilder tableFields = new StringBuilder();
            var str = String.Join(", ", selectedTables);
            tableFields.Append(str);
            return tableFields;
        }

        private StringBuilder generateWhereClause()
        {
            StringBuilder whereClause = new StringBuilder();
            whereClause.Append("WHERE ");
            List<string> allCriterias = new List<string>();
            foreach (var qc in selectedColumns)
            {
                if (qc.Criterias != null)
                {
                    foreach (var c in qc.Criterias)
                    {
                        allCriterias.Add(qc.Name + " " + c.ToString());
                    }
                }
            }
            var str = String.Join(" AND ", allCriterias);
            whereClause.Append(str);
            return whereClause;
        }

        private StringBuilder generateHavingClause()
        {
            StringBuilder havingClause = new StringBuilder();
            havingClause.Append("HAVING ");
            List<string> allCriterias = new List<String>();
            foreach (var qc in aggregatedColumns)
            {
                if (qc.HavingCriterias != null)
                {
                    foreach (var c in qc.HavingCriterias)
                    {
                        allCriterias.Add(qc.Aggregate + "(" + qc.Name + ")" + c.ToString());
                    }
                }
            }
            var str = String.Join(" AND ", allCriterias);
            havingClause.Append(str);
            return havingClause;
        }

        public void OnPostBack(QColumn colBack)
        {
            List<QCriteria> criterias = colBack.Criterias;
            if (selectedColumns.Contains(colBack))
            {
                if (colBack.Aggregate != null)
                {
                    if (colBack.Aggregate.Name.Equals("LEN")) //if a column has an aggregate that does not need group by, put criteria into where clause
                    {
                        selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).Criterias = colBack.Criterias;
                        selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).HavingCriterias = null;
                        generateSQLStatement();
                        return;
                    }
                    //a column that has an aggregate which needs a group by, put the criteria into having clause
                    selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).HavingCriterias = colBack.Criterias;
                    selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).Criterias = null;
                    generateSQLStatement();
                    return;
                }
                else //a column that does not have an aggregate, simply put into where clause
                {
                    selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).Criterias = colBack.Criterias;
                    generateSQLStatement();
                    return;
                }                
            }          
        }

        //========= generate 'GROUP BY <table>' part of sql statement ======================/
        private StringBuilder generateGroupByStatement()
        {
            StringBuilder groupByFields = new StringBuilder();
            if (groupByColumns.Count > 0)
                groupByFields.Append("GROUP BY ");
            for (int i = 0; i < groupByColumns.Count; i++)
            {
                if (groupByColumns.Count == 1)
                {
                    groupByFields.Append(groupByColumns.ElementAt(i).ToString());
                }
                else if (i == groupByColumns.Count - 1)
                {
                    groupByFields.Append(groupByColumns.ElementAt(i).ToString());
                }
                else
                {
                    groupByFields.Append(GroupByColumns.ElementAt(i).ToString());
                    groupByFields.Append(", ");
                    groupByFields.Append(Environment.NewLine);
                }
            }
            return groupByFields;
        }

        //======================================================================================//
        //=                                                                                    =//
        //=       TreeView events handling for initialize and select event handling            =//
        //=                                                                                    =//
        //======================================================================================//

        //===== Populate the tree view with the available tables in selected database, returned from user defined function, and views ====//       
        private void populateTreeView()
        {
            CustomParentNode parentNode;
            CustomChildNode childNode;
            //This is for tables. Sort the list first by calling Sort()
            List<QTable> tables = service.GetTables();
            List<QColumn> columns;
            tables.Sort(
                delegate(QTable table1, QTable table2)
                {
                    int compareTable = table1.Name.CompareTo(table2.Name);
                    if (compareTable == 0)
                    {
                        return table2.Name.CompareTo(table1.Name);
                    }
                    return compareTable;
                }
                );
            foreach (QTable table in tables) // this is for tables in nature
            {
                parentNode = new CustomParentNode(table);
                parentNode.Tag = table;
                parentNode.Text = table.Name;
                parentNode.ForeColor = Color.Blue;
                tvAvailableColumns.Nodes.Add(parentNode);
                //service.Connect(); //uncomment this if use connected layer
                columns = service.GetColumns(table.Name);
                foreach (QColumn column in columns)
                {
                    childNode = new CustomChildNode(column);
                    childNode.Text = column.Name;
                    childNode.ForeColor = Color.Black;
                    parentNode.Nodes.Add(childNode);
                }
            }
            //Tables that returned from user defined functions
            List<QTableFunction> tableFunctions = service.GetTablesFunction();
            tableFunctions.Sort(
                    delegate(QTableFunction table1, QTableFunction table2)
                    {
                        int compareTable = table1.Name.CompareTo(table2.Name);
                        if (compareTable == 0)
                        {
                            return table2.Name.CompareTo(table2.Name);
                        }
                        return compareTable;
                    }
                );
            foreach (QTableFunction table in tableFunctions)
            {
                parentNode = new CustomParentNode(table);
                parentNode.Tag = table;
                parentNode.Text = table.Name;
                parentNode.ForeColor = Color.OrangeRed;
                tvAvailableColumns.Nodes.Add(parentNode);
                //service.Connect(); //uncomment this if use connected layer
                columns = service.GetColumnsFromTableFunction(table.Name);
                foreach (QColumn column in columns)
                {
                    childNode = new CustomChildNode(column);
                    childNode.Text = column.Name;
                    childNode.ForeColor = Color.Black;
                    parentNode.Nodes.Add(childNode);
                }
            }
            //views
            List<QView> views = service.GetViews();
            views.Sort(
                delegate(QView view1, QView view2)
                {
                    int compareTable = view1.Name.CompareTo(view2.Name);
                    if (compareTable == 0)
                    {
                        return view2.Name.CompareTo(view1.Name);
                    }
                    return compareTable;
                }
                );
            foreach (QView view in views)
            {
                parentNode = new CustomParentNode(view);
                parentNode.Tag = view;
                parentNode.Text = view.Name;
                parentNode.ForeColor = Color.Green;
                tvAvailableColumns.Nodes.Add(parentNode);
                //service.Connect(); //uncomment this if use connected layer
                columns = service.GetColumns(view.Name);
                foreach (QColumn column in columns)
                {
                    childNode = new CustomChildNode(column);
                    childNode.Text = column.Name;
                    childNode.ForeColor = Color.Black;
                    parentNode.Nodes.Add(childNode);
                }
            }
        }

        //=============Handling item selecting on TreeView=================================//
        private void tvAvailableColumns_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.ForeColor == Color.Purple) //if a node is purple, that means a user select already, simply returns
            {
                return;
            }
            if (e.Node.Parent != null && e.Node.Parent.GetType() == typeof(CustomParentNode)) //if select child node which is column of a table
            {
                CustomParentNode cpn = (CustomParentNode)e.Node.Parent;
                CustomChildNode ccn = (CustomChildNode)e.Node;
                table = cpn.Table;
                view = cpn.View;
                tableFunction = cpn.TableFunction;
                if (view == null && tableFunction == null)
                    tableName = table.Name;
                else if (table == null && tableFunction == null)
                    tableName = view.Name;
                else if (table != null && view == null)
                    tableName = tableFunction.Name;

                index++;
                columnName = ccn.Column.Name;
                alias = ccn.Column.Alias;
                dataType = ccn.Column.DataType;

                populateDataGridView(dgvSelectedColumns);
                e.Node.Parent.ForeColor = Color.Purple;
                e.Node.ForeColor = Color.Purple;

                //-------------------------------------Tricky part here:------------------------------------------------------//
                //check dictionary for values of a given key. If the key has no value, add new value associated with key
                //If the dictionary already has the key, add more values for that key
                List<QColumn> existing;
                if (!sqlFields.TryGetValue(tableName, out existing))
                {
                    existing = new List<QColumn>();
                    sqlFields[tableName] = existing;
                }
                QColumn c = new QColumn(tableName + "." + "[" + columnName + "]", dataType);
                c.Index = index;
                existing.Add(c);
                existing.Sort();
                //-------------------------------------End Tricky part here:------------------------------------------------------//
                //after we populate our dictionary, pull out keys and list of value
                populateSelectedTableList();
                if (selectedColumns.Count == 0)
                {
                    populateSelectedColumnList();
                }
                else
                {
                    QColumn col = new QColumn(tableName + "." + "[" + columnName + "]", dataType);
                    col.Index = index;
                    selectedColumns.Add(col);
                    selectedColumns.Sort();
                }
            }
            else
            {
                tableName = e.Node.Text;
                columnName = "*";
            }
            bindGroupColumns();
            generateSQLStatement();
        }

        //=======================================================================================//
        //=                                                                                     =//
        //=       DataGridView events handling for TextBox changed, ComboBox, and CheckBox      =//
        //=                                                                                     =//
        //=======================================================================================//

        //======== Generate column headers for DataGridView ===================/
        private void generateColumnHeader(DataGridView dgv)
        {
            dt.Columns.Add("Table");
            dt.Columns["Table"].ReadOnly = true;

            dt.Columns.Add("Column");
            dt.Columns["Column"].ReadOnly = true;

            dt.Columns.Add("Index");

            dt.Columns.Add("DataType");
            dt.Columns["DataType"].ReadOnly = true;

            dt.Columns.Add("Alias");

            dt.Columns.Add("Output", System.Type.GetType("System.Boolean"));//NOTE: this is how we add a checkbox to a data grid view
            dgv.DataSource = dt;

            //populate aggregate list to drop down list
            DataGridViewComboBoxColumn comAgg = new DataGridViewComboBoxColumn();
            comAgg.Name = "Aggregate";
            comAgg.HeaderText = "Aggregate";
            comAgg.ValueType = typeof(Aggregates);
            comAgg.DataSource = Enum.GetValues(typeof(Aggregates));
            dgv.Columns.Add(comAgg);

            //NOTE: this is how we add a button to a data grid view
            DataGridViewButtonColumn btnCriteriaAnd = new DataGridViewButtonColumn();
            btnCriteriaAnd.UseColumnTextForButtonValue = true;
            btnCriteriaAnd.Name = "CriteriaAnd";
            btnCriteriaAnd.Text = "Add criteria";
            btnCriteriaAnd.HeaderText = "And";
            btnCriteriaAnd.FlatStyle = FlatStyle.Standard;
            btnCriteriaAnd.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(btnCriteriaAnd);

            //DataGridViewButtonColumn btnOr = new DataGridViewButtonColumn();
            //btnOr.UseColumnTextForButtonValue = true;
            //btnOr.Name = "CriteriaOr";
            //btnOr.Text = "Add criteria";
            //btnOr.HeaderText = "Or";
            //btnOr.FlatStyle = FlatStyle.Standard;
            //btnOr.CellTemplate.Style.BackColor = Color.Honeydew;
            //dgv.Columns.Add(btnOr);

            //dt.Columns.Add("CriteriaAnd");

            //dt.Columns.Add("CriteriaOr");

            dgv.Columns["Index"].Visible = false;
            dgv.AllowUserToAddRows = false; //initialially set the first row below header to empty
            dgv.Columns["Table"].SortMode = DataGridViewColumnSortMode.NotSortable; //disable sorting when user click column header
            dgv.Columns["Column"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["DataType"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["Alias"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["CriteriaAnd"].SortMode = DataGridViewColumnSortMode.NotSortable;
            //dgv.Columns["CriteriaOr"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["Aggregate"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }

        //======= Generate values for cells in DataGridView when user select columns from tree view ======/
        private void populateDataGridView(DataGridView dgv)
        {
            DataGridViewComboBoxColumn cbSortOrder = new DataGridViewComboBoxColumn();
            DataRow dr;
            dr = dt.NewRow();
            dr["Table"] = tableName;
            dr["Column"] = columnName;
            dr["Index"] = index;
            dr["DataType"] = dataType;
            dr["Alias"] = alias;
            dr["Output"] = true;
            //if (dgv.Rows.Count == 0)
            //{
            //    cbSortOrder.Items.AddRange(new string[] { "a", "b" });
            //}
            dt.Rows.Add(dr);
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
        }

        //======== Handle textbox changed in datagrid ======================================/
        private void dgvSelectedColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvSelectedColumns.Columns["Alias"].Index)
            {
                if (dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString() != null) //if a column already has alias, change it
                {
                    string alias = dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString();
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value.ToString();
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value.ToString();
                    int index = int.Parse((string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value.ToString());
                    string current = table + ".[" + column + "]";
                    selectedColumns.Where(c => string.Equals(c.Name, current))
                            .ToList().ForEach(c => c.Alias = alias);
                    generateSQLStatement();
                }
                if (dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value == null ||
                    dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value == DBNull.Value ||
                    string.IsNullOrWhiteSpace(dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString()) ||
                    dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString() == "")
                {
                    string alias = dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString();
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value.ToString();
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value.ToString();
                    int index = int.Parse((string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value.ToString());
                    string current = table + ".[" + column + "]";
                    selectedColumns.Where(c => string.Equals(c.Name, current))
                        .ToList().ForEach(c => c.Alias = alias);
                    generateSQLStatement();
                }
            }
            else if (e.ColumnIndex == dgvSelectedColumns.Columns["Aggregate"].Index)
            {
                string selectedAggregate = dgvSelectedColumns.Rows[e.RowIndex].Cells["Aggregate"].Value.ToString(); //get string value of Aggregate drop down
                if (!selectedAggregate.Equals("NONE")) //if an aggregate is not none, it is an aggregate
                {
                    QAggregate aggregate = new QAggregate { Name = selectedAggregate };
                    string alias = dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString();
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value.ToString();
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value.ToString();
                    int index = int.Parse((string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value.ToString());
                    string current = table + ".[" + column + "]";
                    foreach (var item in selectedColumns.Where(c => c.Name.Equals(current)))
                    {
                        item.Aggregate = aggregate;
                        aggregatedColumns.Add(item);
                        if (item.Criterias != null)
                        {
                            item.HavingCriterias = item.Criterias;
                            item.Criterias = null;
                        }
                    }       
                    bindGroupColumns();
                    generateSQLStatement();
                }
                else
                {
                    QAggregate aggregate = new QAggregate { Name = selectedAggregate };
                    string alias = dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value.ToString();
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value.ToString();
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value.ToString();
                    int index = int.Parse((string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value.ToString());
                    string current = table + ".[" + column + "]";
                    foreach (var item in selectedColumns.Where(c => c.Name.Equals(current)))
                    {
                        item.Aggregate = null;
                        aggregatedColumns.Remove(item);
                        if (item.HavingCriterias != null)
                        {
                            item.Criterias = item.HavingCriterias;
                            item.HavingCriterias = null;
                        }
                    }
                    bindGroupColumns();
                    generateSQLStatement();
                }
            }
        }

        //======== Handle checkbox checked/unchecked in datagrid ===========================/
        private void dgvSelectedColumns_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (checkboxEditing)
            {
                if (dgvSelectedColumns.IsCurrentCellDirty)
                {
                    dgvSelectedColumns.CommitEdit(DataGridViewDataErrorContexts.Commit);

                }
            }
            
        }

        //======== Handle checkbox event in datagrid ========================================/
        private void dgvSelectedColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvSelectedColumns.Columns["Output"].Index) //when re-check checkbox
            {
                if ((bool)dgvSelectedColumns.Rows[e.RowIndex].Cells["Output"].Value)
                { //recheck checkbox
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value;
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value;
                    string datatype = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["DataType"].Value;
                    string indexString = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value;
                    int index = int.Parse(indexString);
                    string selectedField = table + "." + "[" + column + "]";
                    QColumn col = new QColumn(selectedField, datatype);
                    col.Index = index;
                    selectedColumns.Add(col);
                    selectedColumns.Sort();
                    bindGroupColumns();
                    generateSQLStatement();
                }
                else //when uncheck the checkbox in datagridview
                {
                    dgvSelectedColumns.Rows[e.RowIndex].Cells["Alias"].Value = ""; //when uncheck the box, make sure to clear the Alias cell
                    dgvSelectedColumns.Rows[e.RowIndex].Cells["Aggregate"].Value = Aggregates.NONE;
                    string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value;
                    string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value;
                    string datatype = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["DataType"].Value;
                    string selectedField = table + "." + "[" + column + "]";
                    foreach (var item in selectedColumns.Where(c => c.Name.Equals(selectedField)))
                    {
                        item.Aggregate = null;
                    }
                    selectedColumns.RemoveAll(c => c.Name.Equals(selectedField));
                    foreach (var item in selectedColumns.Where(c => c.Name.Equals(selectedField)))
                    {
                        item.Aggregate = null;
                        aggregatedColumns.Remove(item);
                    }
                    bindGroupColumns();      
                    generateSQLStatement();
                }
            }
            if (e.ColumnIndex == dgvSelectedColumns.Columns["CriteriaAnd"].Index) //when re-check checkbox
            {
                string column = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Column"].Value;
                string table = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Table"].Value;
                string datatype = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["DataType"].Value;
                string indexString = (string)dgvSelectedColumns.Rows[e.RowIndex].Cells["Index"].Value;
                int index = int.Parse(indexString);
                string selectedField = table + "." + "[" + column + "]";
                var col = selectedColumns.Where(c => string.Equals(c.Name, selectedField)).First();
                FormCriteriaAnd frmCriteriaAnd = new FormCriteriaAnd(this, col);
                frmCriteriaAnd.ShowDialog();
            }
        }



        /// <summary>
        /// A little hack here:
        /// Get CellEnter event to pay attention to Output checkbox only!
        /// Then in CurrentCellDirtyChange, I check for this to call the Commit change
        /// </summary>
        private bool checkboxEditing; //variable to record what cell is being changed. In this case, I only want to record changes made to checkbox
        private void dgvSelectedColumns_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvSelectedColumns.Columns["Output"].Index)
            {
                checkboxEditing = true;
            }
            else
            {
                checkboxEditing = false;
            }

            
        }

        //==================================================================================//
        //=                                                                                =//
        //=             Menu item clicking for New, Exit, About, X closing icon            =//
        //=                                                                                =//
        //==================================================================================//
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConnectServer fcs = new FormConnectServer();
            fcs.Show();
            this.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (service.SqlConnection != null)
            {
                this.service.SqlConnection.Close();
            }
            Application.Exit();
        }

        private void aboutQueryBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Query Builder" + System.Environment.NewLine +
                            "Version 1.0" + System.Environment.NewLine +
                            "Copyright (c) 2015 Jacky Nguyen");
        }

        private void FormColumnSelection_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (service.SqlConnection != null)
            {
                this.service.SqlConnection.Close();
            }
            Application.Exit();
        }

        //==================================================================================//
        //=                                                                                 //
        //=             Work on saving and opening to/from a json format file               //
        //=                                                                                 //
        //==================================================================================//

        private void mnuSave_Click(object sender, EventArgs e)
        {
            promptSaveLocation();
        }

        private void promptSaveLocation()
        {
            try
            {
                Stream myStream;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog.OpenFile()) != null)
                    {
                        string fileName = saveFileDialog.FileName;
                        writeDataToFile(fileName);
                        myStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something bad has happened, please contact IT services for immediate assist");
            }
            
        }

        private void writeDataToFile(string fileName)
        {
            try
            {
                using (FileStream fs = File.Open(fileName + ".json", FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, prepareData());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while writing file. Please contact IT department for immediate assist!");
            }         
        }
        
        /// <summary>
        /// put all selectedColumns, selectedTables, aggregateColumns, groupByColumns list into 1 object 
        /// named QueryInfo in order to serialize for writing Json
        /// </summary>
        /// <returns></returns>
        private QueryInfo prepareData()
        {
            QueryInfo qi = new QueryInfo();
            qi.Tables = selectedTables;
            qi.Columns = selectedColumns;
            qi.AggregatedColumns = aggregatedColumns;
            qi.GroupByColumns = groupByColumns;
            return qi;
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            getFileOpen();
        }

        private void getFileOpen()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.CheckFileExists = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.FileName.Trim() != string.Empty)
                    {
                        using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                        {
                            string json = sr.ReadToEnd();
                            QueryInfo qi = JsonConvert.DeserializeObject<QueryInfo>(json);
                            if (qi == null)
                            {
                                MessageBox.Show("It appears to be nothing in the file the you are trying to open. Please check again or contact IT services for immediate help.");
                                return; 
                            }
                            refillSelection(qi);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }       
        }

        private void refillSelection(QueryInfo qi)
        {
            resetEverything();
            List<string> refillTables = qi.Tables; //get selected tables
            List<QColumn> refillColumns = qi.Columns; //get selected columns
            List<QColumn> refillAggregatedColums = qi.AggregatedColumns; //get columns that have aggregated function set
            List<QColumn> refillGroupByColumns = qi.GroupByColumns; //get columns that have grouped by
           
            foreach (var item in qi.Tables)
            {
                TreeNode[] treeNodes = tvAvailableColumns.Nodes
                                    .Cast<TreeNode>()
                                    .Where(r => r.Text == item)
                                    .ToArray();

                foreach(var node in treeNodes){ //parent nodes
                    node.Toggle();
                    foreach (var col in refillColumns)
                    {
                        foreach (TreeNode n in node.Nodes) //child nodes
                        {
                            int dotIndex = col.Name.IndexOf(".");
                            int lastSquareIndex = col.Name.IndexOf("]");
                            int lengthGet = lastSquareIndex - (dotIndex + 2);
                            if (n.Text == col.Name.Substring(dotIndex + 2, lengthGet))
                            {
                                tvAvailableColumns.SelectedNode = n;
                            }          
                        }
                    }   
                }
            }  //end outer foreach loop 

            //because when i call treeview.SelectedNode = node, i automatically populate my selectedColumns and selectedTable lists
            //so i need a way to set associated aggregate back
            //by reset our selectedColumns and selectedTables.
            //NOTE: DO NOT RESET AggregateColumns and GroupByColumns list back, it will cause bugs!!
            selectedColumns.Clear();
            selectedColumns = refillColumns;

    

            selectedTables.Clear();
            selectedTables = refillTables;

            for (int i = 0; i < refillAggregatedColums.Count; i++)
            {
                int datagridViewIndex = refillAggregatedColums.ElementAt(i).Index - 1;
                dgvSelectedColumns.Rows[datagridViewIndex].Cells["Aggregate"].Value = EnumExtension.ParseEnum<Aggregates>(refillAggregatedColums.ElementAt(i).Aggregate.ToString());
            }

            for (int i = 0; i < selectedColumns.Count; i++)
            {
                int datagridViewIndex = selectedColumns.ElementAt(i).Index - 1;
                if (selectedColumns.ElementAt(i).Aggregate != null && selectedColumns.ElementAt(i).Aggregate.Name.Equals("LEN"))
                {
                    dgvSelectedColumns.Rows[datagridViewIndex].Cells["Aggregate"].Value = EnumExtension.ParseEnum<Aggregates>(selectedColumns.ElementAt(i).Aggregate.ToString());
                }
                performTransferHavingToWhereClause(selectedColumns.ElementAt(i)); //a little hack here for changing from having clause to where clause
            }
            bindGroupColumns();
            generateSQLStatement();
        }

        //because when we set an aggregate, the code automatically detect a column to have a having clause, so we need to perform check
        //  if an aggregate is non group by, we put the having clause back to where clause and vice versa
        private void performTransferHavingToWhereClause(QColumn colBack)
        {
            if (colBack.Aggregate != null)
            {
                if (colBack.Aggregate.Name.Equals("LEN")) //if a column has an aggregate that does not need group by, put criteria into where clause
                {
                    selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).Criterias = colBack.HavingCriterias;
                    selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).HavingCriterias = null;
                    generateSQLStatement();
                    return;
                }
                //a column that has an aggregate which needs a group by, put the criteria into having clause
                selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).HavingCriterias = colBack.HavingCriterias;
                generateSQLStatement();
                return;
            }
            else //a column that does not have an aggregate, simply put into where clause
            {
                selectedColumns.ElementAt(selectedColumns.IndexOf(colBack)).Criterias = colBack.Criterias;
                generateSQLStatement();
                return;
            }    
        }

        private void mnuHelp_Click(object sender, EventArgs e)
        {
        }

        private void tvAvailableColumns_MouseClick(object sender, MouseEventArgs e)
        {
            tvAvailableColumns.SelectedNode = tvAvailableColumns.GetNodeAt(e.X, e.Y);
            List<MenuItem> joinTables = new List<MenuItem>();
            if (selectedTables.Count > 1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    TreeNode selectedNode = ((TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                    ContextMenu cm = new ContextMenu();
                    if (tvAvailableColumns.SelectedNode.Parent == null) //only join if right click on table name
                    {
                        foreach (var t in selectedTables.Where(a => !a.Equals(selectedNode.Text)))
                        {
                            MenuItem item = new MenuItem(t);
                            joinTables.Add(item);
                        }
                    }
                    foreach (var i in joinTables)
                    {
                        cm.MenuItems.Add(i);
                    }
                    
                    cm.Show(tvAvailableColumns, e.Location);
                }
            }
        }
    }
}

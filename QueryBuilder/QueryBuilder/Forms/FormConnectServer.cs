using Core.Utils;
using QueryBuilder.Forms;
//using QueryBuilderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace QueryBuilder
{
    public partial class FormConnectServer : Form
    {
        private string serverName;
        private string selectedDatabase;
        private DataTable databases = null;
        private List<string> databaseNames = new List<string>();
        private List<string> databaseEmpty = new List<string>{
                                                 "Loading...."
                                             };

        public FormConnectServer()
        {
            InitializeComponent();
            btnOk.Enabled = false;
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectServer();
            }
            catch (SqlException ex)
            {
                databases = null;
                MessageBox.Show(ex.ToString());
                return;
            }
            
            if (databases != null)
            {
                MessageBox.Show("Test connection succeeded", "Query Builder",
                                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                MessageBox.Show("Test connection failed", "Query Builder",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=" + serverName + ";" + "Initial Catalog=" + selectedDatabase + ";"
                                        + " Integrated Security=True;";
            FormColumnSelection frmColumnSelection = new FormColumnSelection(selectedDatabase, connectionString);
            frmColumnSelection.Show();
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(databases != null) 
                databases.Dispose();
            this.Close();
            Application.Exit();
        }

        void ConnectServer()
        {
            var con = new SqlConnection("Data Source=" + serverName + ";"
                                        + " Integrated Security=True;");
            using (con)
            {
                con.Open();
                databases = con.GetSchema("Databases");
                con.Close();                
            }     
        }

        /// <summary>
        /// Populate the combobox with the list of databases existed in a server name
        /// </summary>
        void PopulateDatabaseName()
        {
            try
            {
                databaseNames.Clear(); //make sure to clear the list of databases
                cbDatabaseName.DataSource = null; //clear the combobox
                ConnectServer(); //make connection to new server name if so
            }
            catch (SqlException ex) //if an exception occurs
            {
                LogException le = new LogException(this.GetType().Name, 
                                                    "Catching SqlException", ex, 
                                                    "PopulateDatabaseName()", DateTime.Now);
                OutputLog ol = new OutputLog(le);
                ol.WriteLog();
                databases = null; //destroy the DataTable object
                MessageBox.Show("Server name does not exit or access denied"); //inform the user          
                cbDatabaseName.DataSource = null; //set the combobox to null
                cbDatabaseName.DataSource = databaseEmpty; //hack: displaying a member saying it is loading table
            }
            if (databases != null)
            {            
                foreach (DataRow row in databases.Rows)
                {
                    string databaseName = row.Field<string>("database_name");
                    databaseNames.Add(databaseName);
                }
                          
            }
            cbDatabaseName.DataSource = databaseNames;
        }
        //JACKYNGUYEN-HP\SQLSERVER2012
        private void txtServerName_TextChanged(object sender, EventArgs e)
        {
            this.btnOk.Enabled = !string.IsNullOrWhiteSpace(this.txtServerName.Text);
            serverName = this.txtServerName.Text;
        }

        private void cbDatabaseName_DropDown(object sender, EventArgs e)
        {
            PopulateDatabaseName();
        }

        private void cbDatabaseName_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selectedItem = cbDatabaseName.SelectedItem;
            if(selectedItem != null)
                selectedDatabase = cbDatabaseName.SelectedItem.ToString();
        }

        private void FormConnectServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(databases != null) 
                databases.Dispose();
        }

    }
}

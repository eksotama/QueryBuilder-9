using Core.Extensions;
//using QueryBuilder.Helper;
using QueryBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueryBuilder.Forms
{
    public partial class FormCriteriaAnd : Form
    {
        private QColumn column;
        private FormColumnSelection frm;

        public FormCriteriaAnd()
        {
            InitializeComponent();
            
        }

        public FormCriteriaAnd(FormColumnSelection frm, QColumn col)
            : this()
        {
            this.frm = frm;
            this.column = col;
            onLoadForm();
        }

        private void onLoadForm()
        {
            Debug.WriteLine("===== NAME: " + column.Name);           
            cboCriterias1.DataSource = Enum.GetNames(typeof(CriteriaOperator));
            txtColumnName1.Text += column.Name;
            cboCriterias1.SelectedIndexChanged += new EventHandler(Status_SelectedChanged);
            
        }

        private void Status_SelectedChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int index = cb.SelectedIndex;
            string selectedOperator = cboCriterias1.Items[index].ToString();
            Debug.WriteLine("cb name: " + cb.Name);
            Debug.WriteLine("SELECT: " + selectedOperator);
            QCriteria criteria = new QCriteria();
            criteria.CrtOperator = EnumExtension.ParseEnum<CriteriaOperator>(selectedOperator);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            
            if (cboCriterias1.SelectedIndex != -1 & String.IsNullOrEmpty(txtValue1.Text))
            {
                MessageBox.Show("A value is required");
                return;
            }
            column.Criterias = CollectCriterias();
            frm.OnPostBack(column);
            this.Close();
        }


        private List<QCriteria> CollectCriterias()
        {
            List<QCriteria> criterias = new List<QCriteria>();
            foreach (var ctr in this.flowLayoutPanel1.Controls.OfType<Panel>()) 
            {
                QCriteria criteria = new QCriteria();
                ComboBox cb = ctr.Controls.OfType<ComboBox>().First();
                TextBox tb = ctr.Controls.OfType<TextBox>().Skip(1).First();
                Debug.WriteLine(cb.Items[cb.SelectedIndex].ToString());
                Debug.WriteLine("text: " + tb.Text);
                criteria.CrtOperator = EnumExtension.ParseEnum<CriteriaOperator>(cb.Items[cb.SelectedIndex].ToString());
                criteria.Values = tb.Text;
                if (!tb.Text.Equals(String.Empty))
                {
                    criterias.Add(criteria);
                }       
            }
            return criterias;
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            LoadMe();
        }


        private int numberOfPanel = 1;
        private void LoadMe()
        {
            numberOfPanel++;

            Panel panel = new Panel();
            panel.Name = "panel" + numberOfPanel;
            panel.Width = flowLayoutPanel1.Width;
            panel.Height = panel1.Height;

            TextBox textbox = new TextBox();
            textbox.Name = "txtColumnName" + numberOfPanel;
            textbox.ReadOnly = true;
            textbox.Multiline = true;
            textbox.BorderStyle = BorderStyle.None;
            textbox.BackColor = Control.DefaultBackColor;
            textbox.Width = txtColumnName1.Width;
            textbox.Height = txtColumnName1.Height;
            Point pLabel = new Point();
            pLabel.X = txtColumnName1.Location.X;
            pLabel.Y = panel.Location.Y;
            textbox.Location = pLabel;
            textbox.Text = column.Name;
            
            ComboBox cb = new ComboBox();
            cb.Name = "cboCriterias" + numberOfPanel;
            cb.DataSource = Enum.GetNames(typeof(CriteriaOperator));
            Point pComboBox = new Point();
            pComboBox.X = cboCriterias1.Location.X;
            pComboBox.Y = panel.Location.Y;
            cb.Location = pComboBox;
            cb.Width = cboCriterias1.Width;
            cb.SelectedIndexChanged += new EventHandler(Status_SelectedChanged);

            TextBox txb = new TextBox();
            txb.Name = "txtValue" + numberOfPanel;
            Point pTextBox = new Point();
            pTextBox.X = txtValue1.Location.X;
            pTextBox.Y = panel.Location.Y;
            txb.Location = pTextBox;
            txb.Width = txtValue1.Width;

            panel.Controls.Add(textbox);
            panel.Controls.Add(cb);
            panel.Controls.Add(txb);

            flowLayoutPanel1.Controls.Add(panel);
        }
    }
}

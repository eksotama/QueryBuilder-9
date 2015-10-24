using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QueryBuilder.Model;
using System.Diagnostics;
using QueryBuilder.Forms;

namespace QueryBuilder.CustomControl
{
    public partial class Table : UserControl
    {
        public Table()
        {
            InitializeComponent();
            this.tvColumns.AllowDrop = true;
            this.tvColumns.ItemDrag += new ItemDragEventHandler(this.tvColumns_ItemDrag);
            this.tvColumns.DragOver += new DragEventHandler(this.tvColumns_DragOver);
            this.tvColumns.DragEnter += new DragEventHandler(this.tvColumns_DragEnter);
            this.tvColumns.DragDrop += new DragEventHandler(this.tvColumns_DragDrop);
        }

        private void tvColumns_ItemDrag(Object sender, ItemDragEventArgs e)
        {
            TreeNode node = (TreeNode)e.Item;
            Debug.WriteLine("item drag: " + node.Text + " at: " + node.TreeView.Location);
            DoDragDrop(e.Item, DragDropEffects.Link);
        }

        private void tvColumns_DragOver(Object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }

        private void tvColumns_DragEnter(Object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", true))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tvColumns_DragDrop(Object sender, DragEventArgs e)
        {
            string sourceTableName;
            TreeNode sourceNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            sourceTableName = sourceNode.TreeView.Parent.Name;
            Table sourceTable = (Table)sourceNode.TreeView.Parent;
            Point start = sourceNode.TreeView.Parent.Location; //start position of a line
            Debug.WriteLine("source parent point: " + start.ToString());
            Debug.WriteLine("source node location: " + sourceNode.TreeView.PointToClient(new Point(e.X, e.Y)));

            string destinationTableName;
            TreeNode destinationNode;
            Table targetTable;
            Point destinationPoint = tvColumns.PointToClient(new Point(e.X, e.Y)); //destination of the tree node where we do the drop
            destinationNode = tvColumns.GetNodeAt(destinationPoint);
            Point end = destinationNode.TreeView.Parent.Location; //end position of a line;        
            Debug.WriteLine("dest parent point: " + end.ToString());
            Debug.WriteLine("dest node location: " + destinationNode.TreeView.PointToClient(new Point(e.X, e.Y)));
            if (destinationNode != null)
            {
                destinationTableName = (sender as TreeView).Parent.Name;
                targetTable = (Table)destinationNode.TreeView.Parent;
                if (sourceTableName.Equals(destinationTableName))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
                //Debug.WriteLine("drag from : " + sourceNode.Text);
                //Debug.WriteLine("drag from table: " + sourceNode.TreeView.Parent.Name);
                Debug.WriteLine("Drop at: " + destinationNode.Text);
                Debug.WriteLine("Drop at table: " + (sender as TreeView).Parent.Name);

                ///drawing line
                FormJoin frm = (FormJoin)destinationNode.TreeView.Parent.Parent.Parent.Parent.Parent.Parent;
                Relationship rel = new Relationship();
                rel.From = sourceTable;
                rel.SrcPoint = start;
                rel.SrcOffset = sourceNode.TreeView.PointToClient(new Point(e.X, e.Y));
                rel.To = targetTable;
                rel.TargetPoint = end;

                rel.TargetOffset = destinationNode.TreeView.PointToClient(new Point(e.X, e.Y));
                rel.FromColumn = sourceNode.Text;
                rel.ToColumn = destinationNode.Text;
                frm.onCallback(rel); 
            }
        }
        //populate table with columns
        public void setSource(List<QColumn> sources)
        {
            foreach (var col in sources)
            {
                tvColumns.Nodes.Add(col.Name);
            }
        }

        //set title
        public void setTitle(string text)
        {
            lblTableName.Text = text;
        }

        public override bool Equals(object obj)
        {
            Table other = (Table)obj;
            return this.lblTableName.Text.Equals(other.lblTableName.Text);
        }

        public override int GetHashCode()
        {
            return this.lblTableName.GetHashCode() * 11;
        }
    }
}

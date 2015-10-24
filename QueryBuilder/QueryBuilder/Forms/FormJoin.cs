using QueryBuilder.CustomControl;
using QueryBuilder.Model;
using QueryBuilder.Service;
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
    public partial class FormJoin : Form
    {
        private Point pointMouse = new Point();
        private Control ctrlMoved = new Control();
        private bool bMoving = false;

        private List<string> sqlTable = new List<string>();

        public FormJoin()
        {
            InitializeComponent();
        }

        public FormJoin(List<string> sqlTable, string connectionString)
            : this()
        {
            this.sqlTable = sqlTable;
            int oldX = 0;
            int oldY = 0;
            foreach (var table in sqlTable)
            {
                var tableEntity = new Table();
                tableEntity.Name = table;
                tableEntity.setTitle(table);
                DBService service = new DBService(connectionString);
                List<QColumn> columns = service.GetColumns(table);
                tableEntity.setSource(columns);

                Point p = new Point();
                p.X = panel1.Location.X + oldX;
                p.Y = panel1.Location.Y + oldY;

                tableEntity.Location = p;
                oldX += tableEntity.Width + 5;

                if (oldX > panel1.Width)
                {
                    oldX = 0;
                    oldY += tableEntity.Height + 5;
                }
                tableEntity.MouseDown += new MouseEventHandler(Control_MouseDown);
                tableEntity.MouseMove += new MouseEventHandler(Control_MouseMove);
                tableEntity.MouseUp += new MouseEventHandler(Control_MouseUp);

                panel1.Controls.Add(tableEntity);
            }
            panel1.Invalidate();
        }

        private void Control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if not left mouse button, exit
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            // save cursor location
            pointMouse = e.Location;
            //remember that we're moving
            bMoving = true;
            Cursor = Cursors.Hand;
        }

        private void Control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bMoving = false;
            ctrlMoved = null;
            Cursor = Cursors.Default;
        }

        private void Control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
            if (!bMoving || e.Button != MouseButtons.Left)
            {
                return;
            }
            //get control reference
            ctrlMoved = (Control)sender;

            //set control's position based upon mouse's position change
            ctrlMoved.Left += e.X - pointMouse.X;
            if (ctrlMoved.Top < ctrlMoved.Parent.Top)
            {
                ctrlMoved.Top = ctrlMoved.Parent.Top + 10;
            }
            else
            {
                ctrlMoved.Top += e.Y - pointMouse.Y;
            }

            //update connector when move table around
            Table table = (Table)ctrlMoved;
            foreach (var rel in relationship)
            {
                if (rel.From == table)
                {
                    rel.SrcPoint = new Point(table.Location.X + table.Width/2, table.Location.Y);
                }
                else if (rel.To == table) 
                {
                    rel.TargetPoint = new Point( table.Location.X + table.Width/2, table.Location.Y);
                }
            }
            this.panel1.Invalidate();
        }

        //============= update line
        private List<Relationship> relationship = new List<Relationship>();
        public void onCallback(Relationship rel)
        {
            this.relationship.Add(rel);
            this.panel1.Invalidate();
        }

        public class GraphLine
        {
            public GraphLine(Point start, Point end)
            {
                this.StartPoint = start;
                this.EndPoint = end;
            }
            public Point StartPoint;
            public Point EndPoint;
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (var rel in relationship)
            {
                var p = new Pen(Color.Black, 3);
                var point1 = new Point(rel.SrcPoint.X, rel.SrcPoint.Y + Math.Abs(rel.SrcOffset.Y));
                var point2 = new Point(rel.TargetPoint.X, rel.TargetPoint.Y + Math.Abs(rel.TargetOffset.Y));
                g.DrawLine(p, point1, point2);
            }
            g.Dispose();
        }


    }
}

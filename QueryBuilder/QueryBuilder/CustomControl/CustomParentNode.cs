using QueryBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueryBuilder.CustomControl
{
    public class CustomParentNode : TreeNode
    {
        private QTable table;
        private QTableFunction tableFunction;
        private QView view;

        public CustomParentNode(QTable table)
            : base(table.Name)
        {
            this.table = table;
        }

        public CustomParentNode (QTableFunction tableFunction)
            : base(tableFunction.Name) 
        {
            this.tableFunction = tableFunction;
        }

        public CustomParentNode(QView view)
            : base(view.Name)
        {
            this.view = view;
        }

        public QTable Table
        {
            get
            {
                return table;
            }
        }

        public QTableFunction TableFunction
        {
            get
            {
                return tableFunction;
            }
        }

        public QView View
        {
            get
            {
                return view;
            }
        }

        
    }
}

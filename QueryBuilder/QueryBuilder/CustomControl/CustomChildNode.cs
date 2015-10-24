using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QueryBuilder.Model;

namespace QueryBuilder.CustomControl
{
    public class CustomChildNode : TreeNode
    {
        private QColumn column;

        public CustomChildNode(QColumn column)
            : base(column.Name)
        {
            this.column = column;
        }

        public QColumn Column
        {
            get
            {
                return column;
            }
            set
            {
                column = value;
            }

        }

    }
}

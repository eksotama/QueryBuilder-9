using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class QTable
    {
        private string name;
        private List<QColumn> columns;
        private string tableType;
        private string tableAlias;

        public QTable() { }
        public QTable(string name)
        {
            this.name = name;
            this.columns = new List<QColumn>();
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public string Type { 
            get {
                return tableType;
            }
            set
            {
                tableType = value;
            }
        } //to distinguish if a table or table function
        public string Alias {
            get
            {
                return tableAlias;
            }
            set
            {
                tableAlias = value;
            }
        } //table alias

        public override string ToString()
        {
            return name;
        }

    }
}

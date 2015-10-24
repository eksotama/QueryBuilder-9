using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class QTableFunction : QTable
    {
        public QTableFunction() { }
        public QTableFunction(string name) : base(name) { }

        public override string ToString()
        {
            return "q" +  base.ToString();
        }
    }
}

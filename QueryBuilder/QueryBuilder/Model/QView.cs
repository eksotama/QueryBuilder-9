using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class QView : QTable
    {
        public QView() { }

        public QView(string name) : base(name)
        { }

        public override string ToString()
        {
            return "v" + base.ToString();
        }
    }
}

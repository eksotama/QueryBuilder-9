using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class QueryInfo
    {
        public List<string> Tables { get; set; }
        public List<QColumn> Columns { get; set; }
        public List<QColumn> AggregatedColumns {get;set;}
        public List<QColumn> GroupByColumns { get; set; }
        public List<QCriteria> WhereClause { get; set; }
        public List<QCriteria> HavingClause { get; set; }
        public QueryInfo()
        {  }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Tables)
            {
                sb.Append("Table: ");
                sb.Append(item);
                sb.Append(System.Environment.NewLine);
            }
            foreach (var item in Columns)
            {
                sb.Append("Column: ");
                sb.Append(item.Name);
                sb.Append(System.Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}

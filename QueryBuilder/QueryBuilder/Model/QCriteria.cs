using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public enum CriteriaOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterEqual,
        LessThan,
        LessEqual,
        Not,
        Like,
        NotLike,
        IsNull,
        Between
    };

    public class QCriteria
    {
        public CriteriaOperator CrtOperator { get; set; }
        public string Values { get; set; }

        public QCriteria()
        {

        }

        public override string ToString()
        {
            string criteria = string.Empty;

            if (CrtOperator.ToString().Equals("Equal"))
            {
                criteria = " = " + Values;
            }
            else if (CrtOperator.ToString().Equals("NotEqual"))
            {
                criteria = " != " + Values;
            }
            else if (CrtOperator.ToString().Equals("GreaterThan"))
            {
                criteria = " > " + Values;
            }
            else if (CrtOperator.ToString().Equals("GreaterEqual"))
            {
                criteria = " >= " + Values;
            }
            else if (CrtOperator.ToString().Equals("LessThan"))
            {
                criteria = " < " + Values;
            }
            else if (CrtOperator.ToString().Equals("LessEqual"))
            {
                criteria = " <= " + Values;
            }
            else if (CrtOperator.ToString().Equals("Not"))
            {
                criteria = " ! " + Values;
            }
            else if (CrtOperator.ToString().Equals("Like"))
            {
                criteria = " LIKE " + Values;
            }
            else if (CrtOperator.ToString().Equals("NotLike"))
            {
                criteria = " NOT LIKE " + Values;
            }
            else if (CrtOperator.ToString().Equals("IsNull"))
            {
                criteria = " ISNULL( " + Values + ")";
            }
            else
            {
                criteria = " BETWEEN " + Values + " AND " + Values;
            }
            return criteria;
        }

    }
}

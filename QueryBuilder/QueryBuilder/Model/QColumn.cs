using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class QColumn : IComparable
    {
        private int index;
        private string name;
        private string dataType;
        private string alias;
        private QAggregate aggregate;

        private string strCriteriaAnd; //for advanced users
        private string strCriteriaOr; //for advanced users

        private List<QCriteria> criteriaAnd;
        private List<QCriteria> havingCriteriaAnd;

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
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

        public string Alias
        {
            get
            {
                return alias;
            }
            set
            {
                alias = value;
            }
        }

        public string DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                dataType = value;
            }
        }

        public QAggregate Aggregate
        {
            get
            {
                return aggregate;
            }
            set
            {
                aggregate = value;
            }
        }

        public string StrCriteriaAnd
        {
            get
            {
                return strCriteriaAnd;
            }
            set
            {
                strCriteriaAnd = value;
            }

        }

        public string StrCriteriaOr
        {
            get
            {
                return strCriteriaOr;
            }
            set
            {
                strCriteriaOr = value;
            }
        }

        public List<QCriteria> Criterias
        {
            get
            {
                return criteriaAnd;
            }
            set
            {
                criteriaAnd = value;
            }
        }

        public List<QCriteria> HavingCriterias
        {
            get
            {
                return havingCriteriaAnd;
            }
            set
            {
                havingCriteriaAnd = value;
            }
        }

        public QColumn() { }

        public QColumn(string name, string dataType)
        {
            this.name = name;
            this.dataType = dataType;
        }

        public int CompareTo(object obj)
        {
            QColumn columnToCompare = obj as QColumn;
            if (columnToCompare.Index < this.Index)
            {
                return 1;
            }
            if (columnToCompare.Index > this.Index)
            {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return name;
        }

    }
}

using QueryBuilder.CustomControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Model
{
    public class Relationship
    {
        private Table from;
        private Point srcPoint;
        private Point srcOffset;

        private Table to;
        private Point targetPoint;
        private Point targetOffset;

        private string fromColumn;
        private string toColumn;

        public Relationship()
        {

        }

        public Table From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }

        public Point SrcPoint
        {
            get
            {
                return srcPoint;
            }
            set
            {
                srcPoint = value;
            }
            
        }

        public Point SrcOffset
        {
            get
            {
                return srcOffset;
            }
            set
            {
                srcOffset = value;
            }
        }

        public Table To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
            }
        }

        public Point TargetPoint
        {
            get
            {
                return targetPoint;
            }
            set
            {
                targetPoint = value;
            }
        }

        public Point TargetOffset
        {
            get
            {
                return targetOffset;
            }
            set
            {
                targetOffset = value;
            }
        }

        public string FromColumn
        {
            get
            {
                return fromColumn;
            }
            set
            {
                fromColumn = value;
            }
        }

        public string ToColumn
        {
            get
            {
                return toColumn;
            }
            set
            {
                toColumn = value;
            }
        }

        public override bool Equals(object obj)
        {
            Relationship other = obj as Relationship;

            return ((this.fromColumn == other.fromColumn && this.ToColumn == other.ToColumn) || (this.FromColumn == other.ToColumn && this.ToColumn == other.FromColumn));
        }

        public override int GetHashCode()
        {
            return fromColumn.GetHashCode() * 11 + toColumn.GetHashCode()*13;
        }
    }
}

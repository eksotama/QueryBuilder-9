using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Helper
{
    /// <summary>
    /// Extension method for IDataReader to help with Linq
    /// </summary>
    public static class IDataReaderHelper
    {
        public static IEnumerable<T> Select<T> (this IDataReader reader, Func<IDataReader, T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }
    }
}

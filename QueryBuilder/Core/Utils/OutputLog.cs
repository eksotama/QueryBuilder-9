using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public class OutputLog
    {
        private LogException le;
        private StreamWriter file = new StreamWriter("C:\\Users\\JackyNguyen\\Documents\\Visual Studio 2012\\Projects\\QueryBuilder\\log.txt", true);
        public OutputLog(LogException le)
        {
            this.le = le;
        }
        public void WriteLog()
        {
            file.WriteLine(le.ToString());
            file.Close();
        }
    }
}

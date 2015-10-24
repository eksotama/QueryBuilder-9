using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public class LogException
    {
        private String logFile;
        private String message;
        private Exception innerException;
        private String functionName;
        private DateTime dateOccurred;

        #region "Constructor"
        public LogException(String logFile)
        {
            this.logFile = logFile;
        }
        public LogException(String logFile, String message)
            : this(logFile)
        {
            this.message = message;
        }
        public LogException(String logFile, String message, Exception innerException)
            : this(logFile, message)
        {
            this.innerException = innerException;
        }
        public LogException(String logFile, String message, Exception innerException, String functionName)
            : this(logFile, message, innerException)
        {
            this.functionName = functionName;
        }
        public LogException(String logFile, String message, Exception innerException, String functionName, DateTime dateOccurred)
            : this(logFile, message, innerException, functionName)
        {
            this.dateOccurred = dateOccurred;
        }
        #endregion

        public String LogFile
        {
            get
            {
                return logFile;
            }
            set
            {
                logFile = value;
            }
        }
        public String Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }
        public Exception InnerException
        {
            get
            {
                return innerException;
            }
        }
        public String FunctionName
        {
            get
            {
                return functionName;
            }
        }
        public DateTime DateOccurred
        {
            get
            {
                return dateOccurred;
            }
        }

        public override string ToString()
        {
            return dateOccurred + ": " + "Exception occurs at file: " + logFile +
                    ". Function name: " + functionName + Environment.NewLine
                    + "\t\tMessage: " + message + Environment.NewLine
                    + "\t\tStack Trace: " + innerException;
        }
    }
}

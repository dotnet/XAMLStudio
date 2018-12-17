using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlStudio.Models
{
    public class UnhandledException
    {
        public UnhandledException(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}

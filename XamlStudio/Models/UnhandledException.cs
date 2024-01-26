using System;

namespace XamlStudio.Models;

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

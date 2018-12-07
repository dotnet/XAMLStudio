using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Diagnostics;
using XamlStudio.Services.Logging;

namespace XamlStudio.Services
{
    /// <summary>
    /// Provides app-wide logging to the application.
    /// </summary>
    public static class AppLoggerService
    {
        private static readonly object locker = new object();
        private static FileLogger fileLogger;
        private static Guid sessionGuid;

        /// <summary>
        /// Initializes the <see cref="AppLoggerService"/> to start
        /// tracking app activity.
        /// </summary>
        public static void Initialize()
        {
            fileLogger = FileLogger.Instance;
            FileLogger.Instance.Initialize();

            ResetSessionId();

            Task.Run(() => LogAppVersion());
        }

        /// <summary>
        /// Application session identifier gets assigned every time the application gets activated.
        /// </summary>
        public static Guid SessionId
        {
            get { return sessionGuid; }
        }

        /// <summary>
        /// Resets the session guid used by the app.
        /// </summary>
        public static void ResetSessionId()
        {
            sessionGuid = Guid.NewGuid();
            fileLogger.Log($"Session Id: {SessionId}");
        }

        /// <summary>
        /// Logs the version of the application at the start of the logging.
        /// </summary>
        private static void LogAppVersion()
        {
            var appVersion = Package.Current.Id.Version;
            fileLogger.Log($"App version: {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}");
        }

        /// <summary>
        /// Causes the file logger to store all the messages it hasn't saved to disk.
        /// </summary>
        public static void FlushMessages()
        {
            Task.Run(() => fileLogger.FlushMessagesAsync(LogFileType.All)).Wait();
        }

        /// <summary>
        /// Notifies the <see cref="FileLogger"/> that the app is entering into suspension mode.
        /// </summary>
        /// <returns></returns>
        public static Task OnSuspending()
        {
            return fileLogger.OnSuspending();
        }

        /// <summary>
        /// NOtifies the <see cref="FileLogger"/> that the app is entering into resume mode.
        /// </summary>
        public static void OnResuming()
        {
            fileLogger.OnResuming();
        }

        #region LogInfo
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message"></param>
        public static void LogInfo(string message)
        {
            fileLogger.Log(message, LoggingLevel.Information);
        }

        /// <summary>
        /// Logs an informational message with format.
        /// </summary>
        public static void LogInfo(string format, params object[] args)
        {
            string message = format;
            if(args.Length != 0)
            {
                message = string.Format(CultureInfo.InvariantCulture, format, args);
            }

            LogInfo(message);
        }
        #endregion

        #region LogError

        /// <summary>
        /// Logs an error message with any acompanying exception.
        /// </summary>
        private static void LogError(string caller, string message = null, Exception ex = null, LoggingLevel logLevel = LoggingLevel.Error)
        {
            fileLogger.Log($"AppError[{caller}]: {message ?? string.Empty} | Exception: {ex?.ToString()}", logLevel);
        }

        /// <summary>
        /// Logs an exception only.
        /// </summary>
        public static void LogError(Exception e, [CallerMemberName] string caller = "")
        {
            LogError(caller: caller, message: null, ex: e);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void LogError(string message, [CallerMemberName] string caller = "")
        {
            LogError(caller: caller, message: message, ex: null);
        }

        /// <summary>
        ///  Logs an error message with exception details.
        /// </summary>
        public static void LogError(string message, Exception e, [CallerMemberName] string caller = "")
        {
            LogError(caller: caller, message: message, ex: e);
        }

        #endregion

        /// <summary>
        /// Logs a message before the app goes into a crash.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void LogCrash(Exception e, object sender)
        {
            string message = $"Sender[{sender?.ToString() ?? string.Empty}]";
            LogError(caller: "AppCrash", message: message, ex: e, logLevel: LoggingLevel.Critical);
        }
    }
}

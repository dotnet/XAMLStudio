using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace XamlStudio.Services.Logging
{
    /// <summary>
    /// Provides services to the app for logging messages to a file.
    /// </summary>
    internal class FileLogger
    {
        private const string AppLogSchemaName = "XamlStudioLog";

        private static bool isInitialized = false;
        private static object lockObject = new object();
        private static SemaphoreSlim flushSemaphore = new SemaphoreSlim(1);

        private static readonly Lazy<FileLogger> instance = new Lazy<FileLogger>(
            () => new FileLogger(), LazyThreadSafetyMode.ExecutionAndPublication);

        private ConcurrentQueue<string> plainTextMessages;

        private string logCreationTimeName;

        private StorageFolder logFolder;
        private StorageFile textLogFile;
        private string logFolderName = string.Empty;

        private readonly Guid channelId = Guid.Parse("49DFC983-82A1-4CAD-B647-064361709FDD");
        private readonly Guid groupId = Guid.Parse("E7B6296F-300D-477B-B253-395512BC5F64");
        private LoggingChannel channel;
        private FileLoggingSession fileSession;
        private LoggingLevel logLevel = LoggingLevel.Information;


        /// <summary>
        /// Returns the Singleton instance of <see cref="FileLogger"/>.
        /// </summary>
        public static FileLogger Instance
        {
            get { return instance.Value; }
        }

        private FileLogger()
        {
            plainTextMessages = new ConcurrentQueue<string>();
        }

        /// <summary>
        /// Initializes the instance of <see cref="FileLogger"/>
        /// </summary>
        public void Initialize()
        {
            lock (lockObject)
            {
                if (isInitialized)
                {
                    return;
                }

                // The creation name is a sortable date string
                logCreationTimeName = GetDateTimePath(DateTime.Now);
                logFolderName = $"{AppLogSchemaName}.{logCreationTimeName}.{Guid.NewGuid()}";

                channel = new LoggingChannel("XamlStudioLogChannel", new LoggingChannelOptions(groupId), channelId);
                InitSession();

                // Cleanup log folder and remove old logs files (threshold is set to 20 days)
                CleanupLogsAsync(new TimeSpan(20, 0, 0, 0)).ConfigureAwait(true);

                isInitialized = true;
            }
        }

        /// <summary>
        /// Initializes the <see cref="FileLoggingSession"/> instance.
        /// </summary>
        private void InitSession()
        {
            fileSession = new FileLoggingSession("XamlStudioFileLogSession");
            fileSession.LogFileGenerated += OnLogFileGenerated;
            fileSession.AddLoggingChannel(channel, logLevel);
        }

        /// <summary>
        /// Handles the event of the application entering into suspension.
        /// </summary>
        public Task OnSuspending()
        {
            return FlushMessagesAsync(LogFileType.All);
        }

        /// <summary>
        /// Triggers actions when the application resumes.
        /// </summary>
        public void OnResuming()
        {
            InitSession();
        }

        /// <summary>
        /// Flushes all the messages currently in processing to storage area.
        /// </summary>
        public async Task FlushMessagesAsync(LogFileType logType)
        {
            try
            {
                Task textTask = logType.HasFlag(LogFileType.Text) ? FlushTextFileMessagesAsync() : Task.CompletedTask;
                Task etlTask = logType.HasFlag(LogFileType.EventTraceLog) ? CloseSessionAndSaveLogFile() : Task.CompletedTask;

                await Task.WhenAll(etlTask, textTask);
            }
            catch(Exception ex)
            {
                AppLoggerService.LogError("An error happened while flushing log messages", ex);
            }
        }

        /// <summary>
        /// Logs messages into the plain text message queue if the level is higher than the default set.
        /// </summary>
        public void Log(string message, LoggingLevel level = LoggingLevel.Information)
        {
            channel.LogMessage(message, level);

            if(level >= logLevel)
            {
                plainTextMessages.Enqueue(message);
            }
        }

        /// <summary>
        /// Handles the event when a log file is saved by <see cref="FileLoggingSession"/>
        /// </summary>
        private async void OnLogFileGenerated(IFileLoggingSession sender, LogFileGeneratedEventArgs args)
        {
            await MoveLogFileToLogFolder(args.File);
        }

        /// <summary>
        /// Closes the active log session and grabs the current log file to get it stored.
        /// </summary>
        private async Task CloseSessionAndSaveLogFile()
        {
            StorageFile logfile = await fileSession.CloseAndSaveToFileAsync();
            await MoveLogFileToLogFolder(logfile);
            fileSession.Dispose();
            fileSession = null;
        }

        /// <summary>
        /// Takes the queue of messages and appends then to the text log file.
        /// </summary>
        private async Task FlushTextFileMessagesAsync()
        {
            await flushSemaphore.WaitAsync();

            try
            {
                await EnsureLogFileCreated();
                var messagesToFlush = new Queue<string>();
                while (true)
                {
                    string logMessage;
                    if (plainTextMessages.TryDequeue(out logMessage))
                    {
                        messagesToFlush.Enqueue(logMessage);
                    }
                    else
                    {
                        break;
                    }
                }

                if (messagesToFlush.Any())
                {
                    await FileIO.AppendLinesAsync(textLogFile, messagesToFlush);

#if DEBUG
                    // If debugging write the logs to the Output window.
                    foreach (var msg in messagesToFlush)
                    {
                        System.Diagnostics.Debug.WriteLine(msg);
                    }
#endif
                }
            }
            catch(Exception ex)
            {
                AppLoggerService.LogError("An error happened while flushing the text log.", ex);
            }
            finally
            {
                flushSemaphore.Release();
            }
        }

        /// <summary>
        /// Moves a log file from a log session into the app logging folder.
        /// </summary>
        private async Task MoveLogFileToLogFolder(StorageFile logFile)
        {
            if (logFile != null)
            {
                StorageFolder targetFolder = logFolder;
                if (targetFolder == null)
                {
                    targetFolder = await GetAppLogFolder();
                }

                string logFileName = GenerateLogFileName(LogFileType.EventTraceLog, DateTime.Now);
                await logFile.MoveAsync(targetFolder, logFileName, NameCollisionOption.GenerateUniqueName);
            }
        }

        /// <summary>
        /// Deletes the log files based on an age threshold.
        /// </summary>
        private async Task CleanupLogsAsync(TimeSpan cleanupThreshold)
        {
            try
            {
                var latestDateToKeep = DateTime.Now - cleanupThreshold;
                var logFolders = await ApplicationData.Current.TemporaryFolder.GetFoldersAsync();
                foreach (var tempFolder in logFolders)
                {
                    if (tempFolder.DateCreated.CompareTo(latestDateToKeep) < 0)
                    {
                        await tempFolder.DeleteAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                AppLoggerService.LogError("An error occurred while cleaning up old log files.", ex);
            }
        }

        /// <summary>
        /// Check if the text log file exists if not opens or creates one.
        /// </summary>
        private async Task EnsureLogFileCreated()
        {
            if(textLogFile == null)
            {
                string filename = GenerateLogFileName(LogFileType.Text, DateTime.Now);
                StorageFolder folder = await GetAppLogFolder();
                textLogFile = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            }
        }

        /// <summary>
        /// Returns the current logging folder location used by the app session.
        /// </summary>
        public async Task<StorageFolder> GetAppLogFolder()
        {
            if(logFolder == null)
            {
                logFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(logFolderName, CreationCollisionOption.OpenIfExists);
            }

            return logFolder;
        }

        /// <summary>
        /// Builds a valid path string based on a given date.
        /// </summary>
        private string GetDateTimePath(DateTime date)
        {
            string timeString = string.Empty;

            try
            {
                timeString = date.ToString(new DateTimeFormatInfo().SortableDateTimePattern, CultureInfo.InvariantCulture);

                foreach (var invalidchar in Path.GetInvalidFileNameChars())
                {
                    timeString = timeString.Replace(invalidchar, '_');
                }
            }
            catch
            {
                timeString = $"{date.Year}-{date.Month}-{date.Day}T{date.Hour}:{date.Minute}:{date.Second}";
            }

            return timeString;
        }

        /// <summary>
        /// Generates a new log file name based on date and file type.
        /// </summary>
        private string GenerateLogFileName(LogFileType fileType, DateTime date)
        {
            string fileName = GetDateTimePath(date);
            return $"{AppLogSchemaName}.{fileName}.{GetFileExtension(fileType)}";
        }

        /// <summary>
        /// Gets the file type extension to use based on the <see cref="LogFileType"/>.
        /// </summary>
        private string GetFileExtension(LogFileType fileType)
        {
            switch(fileType)
            {
                case LogFileType.EventTraceLog:
                    return "etl";
                case LogFileType.Text:
                    return "txt";
            }
            return "etl";
        }
    }

    /// <summary>
    /// Defines the supported file formats
    /// that logging can generate
    /// </summary>
    [Flags]
    public enum LogFileType
    {
        None,
        EventTraceLog,
        Text,
        All,
    }
}

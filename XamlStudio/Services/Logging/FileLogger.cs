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

        private static object lockObject = new object();
        private static bool isInitialized = false;

        private static readonly Lazy<FileLogger> instance = new Lazy<FileLogger>(() => new FileLogger(),
                                                                                 LazyThreadSafetyMode.ExecutionAndPublication);

        private StorageFolder logFolder;

        private ConcurrentQueue<string> plainTextMessages;
        private string logCreatedTime;

        private string logFolderName = string.Empty;

        private readonly Guid channelId = Guid.Parse("49DFC983-82A1-4CAD-B647-064361709FDD");
        private readonly Guid groupId = Guid.Parse("E7B6296F-300D-477B-B253-395512BC5F64");
        private LoggingChannel channel;
        private FileLoggingSession fileSession;
        private LoggingLevel logLevel = LoggingLevel.Information;



        public static FileLogger Instance
        {
            get { return instance.Value; }
        }

        public LoggingChannel LogChannel => channel;

        private FileLogger()
        {
            plainTextMessages = new ConcurrentQueue<string>();
        }

        /// <summary>
        /// Initializes the instance of <see cref="FileLogger"/>
        /// </summary>
        public void Initialize()
        {
            lock(lockObject)
            {
                if(isInitialized)
                {
                    return;
                }

                logCreatedTime = GetDateTimePath(DateTime.Now);
                logFolderName = $"{AppLogSchemaName}.{logCreatedTime}.{Guid.NewGuid()}";

                channel = new LoggingChannel("XamlStudioLogChannel", new LoggingChannelOptions(groupId), channelId);
                InitSession();

                CleanupLogsAsync(new TimeSpan(20, 0, 0, 0)).ConfigureAwait(true); // Delete logs 20 days older.

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

        public Task OnSuspending()
        {
            // Flush Messages
        }

        public void OnResuming()
        {
            InitSession();
        }

        /// <summary>
        /// Handles the event when a log file is saved by <see cref="FileLoggingSession"/>
        /// </summary>
        private async void OnLogFileGenerated(IFileLoggingSession sender, LogFileGeneratedEventArgs args)
        {
            await MoveLogFileToLogFolder(args.File);
        }

        /// <summary>
        /// Moves a log file from a log session into the app logging folder.
        /// </summary>
        private async Task MoveLogFileToLogFolder(StorageFile logFile)
        {
            StorageFolder targetFolder = logFolder;
            if(targetFolder == null)
            {
                targetFolder = await GetAppLogFolder();
            }

            string logFileName = GenerateLogFileName(LogFileType.EventTraceLog, DateTime.Now);
            await logFile.MoveAsync(targetFolder, logFileName, NameCollisionOption.GenerateUniqueName);
        }


        private async Task CleanupLogsAsync(TimeSpan cleanupThreshold)
        {
            try
            {
                var latestDateToKeep = DateTime.Now - cleanupThreshold;
                var logFolders = await ApplicationData.Current.TemporaryFolder.GetFoldersAsync();
                foreach(var tempFolder in logFolders)
                {
                    if(tempFolder.DateCreated.CompareTo(latestDateToKeep) < 0)
                    {
                        await tempFolder.DeleteAsync();
                    }
                }
            }
            catch
            {
                // Log in case this fails.
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
                case LogFileType.TextLog:
                    return "txt";
            }
            return "etl";
        }
    }

    /// <summary>
    /// Defines the supported file formats
    /// that logging can generate
    /// </summary>
    public enum LogFileType
    {
        EventTraceLog,
        TextLog,
    }
}

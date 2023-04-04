using System.Configuration;

namespace FileLogger
{
    public sealed class FileLogger
    {
        private FileLogger(){}

        private static readonly Lazy<IFileLogger> lazy = new Lazy<IFileLogger>(()=> new CustomFileLogger());

        public static IFileLogger Instance
        {
            get
            {
                return lazy.Value; 
            }
        }
    }

    public interface IFileLogger : IDisposable
    {
        void Error(Exception comment, string label = "Exception");

        void Information(string comment, string label = "Information");

        void Error(string comment, string label = "Error");

    }

    internal class CustomFileLogger : IFileLogger{
        private StreamWriter? _fileParser;

        private long _logFileName;

        private string ?_logDirectory;

        private bool disposedValue;
        
        internal CustomFileLogger()
        {
            _logFileName = DateTime.Now.ToFileTimeUtc();

            SetLogFolder();

            CreateLogFile();
        }

        private void SetLogFolder()
        {

            string possibleKey = "LogFiles.Folder";

            StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;
            
            try
            {
                _logDirectory = $"{Directory.GetCurrentDirectory()}\\log";

                if (ConfigurationManager.AppSettings.AllKeys.Any(key => string.Compare(key, possibleKey, cmp) == 0))
                {
                    var possibleValue = ConfigurationManager.AppSettings[possibleKey];

                    if (!string.IsNullOrEmpty(possibleValue))
                    {
                        _logDirectory = possibleValue.ToString().Trim();
                    }
                }

                if(!string.IsNullOrEmpty(_logDirectory) && !Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }

            catch(Exception _)
            {
                FileClose(true);

                StartupError.SaveRecord(_, _logFileName);
            }
        }

        private void CreateLogFile()
        {
            try
            {

                string fullPath = _logDirectory + $"\\{_logFileName}.log";

                string comment = $"{DateTime.Now} ; Information ; log begin";
                
                _fileParser = new StreamWriter(fullPath);

                _fileParser.AutoFlush = true;
            
                _fileParser.WriteLine(comment);
                
            }
            catch(Exception ex)
            {
                FileClose(true);

                StartupError.SaveRecord(ex, _logFileName);
            }
        }

        void IFileLogger.Error(string comment, string label)
        {
            this.WriteToLog(comment, label);
        }

        void IFileLogger.Error(Exception comment, string label)
        {
            this.WriteToLog(comment.Message, label);
        }

        void IFileLogger.Information(string message, string type)
        {
            this.WriteToLog(comment, label);
        }

        private void WriteToLog(string comment, string label)
        {
            try
            {
                string record = $"{DateTime.Now} ; {label.Trim()} ; {comment.ToLower()}";

                if(Environment.UserInteractive)
                {
                    Console.WriteLine(record);
                }

                if(_fileParser != null)
                {
                    if (string.IsNullOrEmpty(record))
                    {
                        _fileParser.WriteLine("");
                    }
                    else
                    {
                        _fileParser.WriteLine(record);
                    }
                }

            }
            catch(Exception _)
            {
                FileClose(true);

                StartupError.SaveRecord(_, _logFileName);
            }
        }

        protected virtual void Dispose(bool disposing){
            if (!disposedValue)
            {
                if (disposing)
                {
                    FileClose();
                }

                disposedValue = true;
            }
        }

        private void FileClose(bool error = false)
        {
            if(_fileParser != null)
            {
                if(error) 
                {
                    _fileParser.WriteLine($"{TimeStamp} ; Error ; issue encountered when attempting to log information");
                }
                _fileParser.WriteLine($"{TimeStamp} ; Information ; log end");

                _fileParser.Flush();

                _fileParser.Close();

                _fileParser.Dispose();

                if(string.IsNullOrEmpty(_logDirectory)) 
                {
                    LogFileMaintenance.RemoveRecords(_logDirectory);
                }
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }
    }

    internal static class StartupError
    {
        internal static void SaveRecord(Exception error, long pid)
        {
            string completeFilePath = Directory.GetCurrentDirectory() + $"\\log\\startupError_{pid}.log";
            
            StreamWriter startupErrorFile = new StreamWriter(completeFilePath);

            startupErrorFile.AutoFlush = true;
        
            if(startupErrorFile != null)
            {
                startupErrorFile.WriteLine($"{DateTime.Now} ; {"Exception"} ; {error.Message}");

                if(error.StackTrace != null)
                {
                    int relatedLogFileNameEnd = error.StackTrace.LastIndexOfAny(new char[] { '\\'}) + 1;

                    string relatedLogFileName = error.StackTrace.Substring(relatedLogFileNameEnd);

                    startupErrorFile.WriteLine($"{DateTime.Now} ; {"Source"} ; {relatedLogFileName}");
                }
                if(error.InnerException != null) 
                {
                    startupErrorFile.WriteLine($"{DateTime.Now} ; {"Inner Exception"} ; {error.InnerException.StackTrace}");
                } 
            }
        }
    }

    internal static class LogFileMaintenance{
        internal static void RemoveRecords(string logDirectory)
        {

            string possibleKey = "LogFiles.Lifespan";

            StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;

            int maxFileAge = 0;

            if (ConfigurationManager.AppSettings.AllKeys.Any(key => string.Compare(key, possibleKey, cmp) == 0))
            {
                var possibleValue = ConfigurationManager.AppSettings[possibleKey];

                if (!string.IsNullOrEmpty(possibleValue))
                {
                    bool isNumber = int.TryParse(possibleValue.ToString(), maxFileAge);
                    if(!isNumber || maxFileAge < 1)
                    {
                        return;
                    }
                }
            }

            DirectoryInfo folderDetails = new DirectoryInfo(logDirectory);

            foreach(FileInfo file in folderDetails.GetFiles())
            {
                if(!file.Name.ToLower().Contains("start"))
                {
                    string utcFileName = file.Name.Substring(0,file.Name.LastIndexOf('.'));

                    long utc = long.Parse(utcFileName);

                    DateTime fileTime = DateTime.FromFileTimeUtc(utc);

                    DateTime oldestFileTime = DateTime.Now.AddMonths(-maxFileAge).ToUniversalTime();

                    if(oldestFileTime > fileTime.ToUniversalTime())
                    {
                        file.Delete();
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Linq;

namespace Logging
{


    public sealed class CsvLogger : IDisposable
    {
        // 
        private static readonly Dictionary<string, CsvLogger> loggers = new Dictionary<string, CsvLogger>();

        private static readonly string SessionFolder = $"_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}";


        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;
        private readonly object lockObj = new object();

        private readonly string logFile;
        private List<string> values = new List<string>();

        public List<string> Values { get => values; set => values = value; }

        private CsvLogger(string logFile, string ext = ".csv")
        {
            this.logFile = logFile + ext;
            Directory.CreateDirectory(Path.Join(Application.persistentDataPath, SessionFolder));
            string filePath = Path.Join(Application.persistentDataPath, SessionFolder, this.logFile);
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream);
        }

        public static CsvLogger GetOrCreateLog(string logFile, string ext = ".csv")
        {
            if (!loggers.TryGetValue(logFile, out CsvLogger logger))
            {
                logger = new CsvLogger(logFile, ext);
                loggers[logFile] = logger;
            }
            return logger;
        }

        public static CsvLogger L(string logFile, string ext = ".csv") => GetOrCreateLog(logFile, ext);


        public static bool TryGetLogger(string logFile, out CsvLogger logger) => loggers.TryGetValue(logFile, out logger);

        public static int Count() => loggers.Count;

        public void AddVal(object val) => values.Add(val.ToString());

        public void AddVals(object[] vals) => values.AddRange(vals.Select(v => v.ToString()));

        public bool HasVals() => values.Count > 0;

        public void FlushVals()
        {
            Log(values.ToArray());
            values.Clear();
        }

        public void Log(object[] values) => Log(String.Join(", ", values));

        public void Log(string message)
        {
            var logEntry = message;

            lock (lockObj)
            {
                _streamWriter.WriteLine(logEntry);
                _streamWriter.Flush();
            }
        }

        public void Dispose()
        {
            lock (lockObj)
            {
                _streamWriter?.Dispose();
                _fileStream?.Dispose();
            }
        }
    }

}

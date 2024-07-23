using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization;
using HarmonyLib;
using LBoL.Base.Extensions;

namespace Logging
{


    public sealed class CsvLogger : IDisposable
    {
        private static readonly Dictionary<string, CsvLogger> loggers = new Dictionary<string, CsvLogger>();

        private static readonly string SessionFolder = $"_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}";

        private static ObjectIDGenerator objectAssociations = new ObjectIDGenerator();
        private static int assCount = 0;

        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;
        private readonly object lockObj = new object();
        private bool isEnabled = true;

        public readonly string logFile;
        public readonly string subFolder;
        public readonly string fileDir;

        private List<string> header = new List<string>();
        private Dictionary<string, string> values = new Dictionary<string, string>();
        // key => isConditional
        private Dictionary<string, bool> valsToSanitize = new Dictionary<string, bool>();
        private string emptyValue = "N/A";

        public static int AssCount { get => assCount; }
        public IReadOnlyList<string> Header { get => header; }
        public IReadOnlyDictionary<string, string> Values { get => values; }
        public IReadOnlyDictionary<string, bool> ValsToSanitize { get => valsToSanitize; }

        public string EmptyValue 
        {
            get => emptyValue; 
            set 
            { 
                values.Keys.Where(k => values[k] == emptyValue).Do(k => values[k] = value); 
                emptyValue = value; 
            } 
        }

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        public CsvLogger(string logFile, string ext = ".csv", string subFolder = "", bool isEnabled = true)
        {
            
            this.logFile = logFile + ext;
            this.subFolder = subFolder;
            fileDir = Path.Join(Application.dataPath, "..");
            fileDir = Path.Join(fileDir, subFolder, SessionFolder);
            Directory.CreateDirectory(fileDir);
            string filePath = Path.Join(fileDir, this.logFile);
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream);
            this.subFolder = subFolder;
            this.isEnabled = isEnabled;
        }

        public static CsvLogger GetOrCreateLog(string logFile, string id, string ext = ".csv", string subFolder = "", bool isEnabled = true)
        {
            var fileId = logFile + id;
            if (!loggers.TryGetValue(fileId, out CsvLogger logger))
            {
                logger = new CsvLogger(logFile + "_" + assCount.ToString(), ext, subFolder, isEnabled);
                loggers[fileId] = logger;
            }
            return logger;
        }

        public static (CsvLogger, long) GetOrCreateLog(string logFile, object ass, string ext = ".csv", string subFolder = "", bool isEnabled = true)
        {
            var assId = objectAssociations.GetId(ass, out var firstTime);
            if (firstTime) assCount++;
            var log = GetOrCreateLog(logFile, assId.ToString(), ext, subFolder, isEnabled);
            return (log, assId);
        }




        public void AddHeaderVal(string valKey)
        {
            if (valKey == null || values.ContainsKey(valKey))
                return;
            header.Add(valKey);
            values.Add(valKey, emptyValue);
        }

        public void SetHeader(IEnumerable<string> collumns)
        {
            header.Clear();
            values.Clear();
            collumns.Do(c => AddHeaderVal(c));
        }

        public void LogHead() => Log(header);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="valKey"></param>
        /// <param name="doThrow"></param>
        /// <returns>False if valKey is not in the header</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool SetVal(object val, string valKey, bool doThrow = true)
        {
            if (!isEnabled)
                return false;
            if (values.ContainsKey(valKey))
            {
                values[valKey] = val?.ToString() ?? "null";
                return true;
            }

            if (doThrow)
                throw new ArgumentException($"CsvLogger {logFile} doesn't have column {valKey}");

            return false;
        }

        public bool SetVals(IEnumerable<KeyValuePair<string, string>> valsNKeys) => valsNKeys.Select(kv => SetVal(kv.Value, kv.Key)).Any();

        public bool HasVals() => values.Values.Any(v => v != emptyValue);

        public void FlushVals()
        {
            var toLog = new string[header.Count];
            var i = 0;
            foreach (var k in header)
            {
                var val = values[k];
                if (valsToSanitize.TryGetValue(k, out var condSan))
                     val = SanitizeVal(val, condSan);
                toLog[i] = val;
                values[k] = emptyValue;
                i++;
            }
            Log(toLog);
        }

        public bool SetCollumnToSanitize(string key, bool conditional = true)
        {
            if (!values.ContainsKey(key))
                return false;
            if(!valsToSanitize.TryAdd(key, conditional))
                valsToSanitize[key] = conditional;
            return true;
        }

        public static string SanitizeVal(string val, bool conditional) 
        {
            var doSan = true;
            if (conditional)
                doSan = val.Contains(",");
            return doSan ? $"\"{val}\"" : val;
        }

        public void Log(IEnumerable<object> values) => Log(string.Join(", ", values));

        public void Log(string message)
        {
            if (!IsEnabled)
                return;
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

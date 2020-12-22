using SedWin.Launcher.Utils.Ftp.RecordProcessors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SedWin.Launcher.Utils.Ftp
{
    public class DirectoryListParser : IDirectoryListParser
    {
        private FtpRecord[] _records;

        public FtpRecord[] FullListing
        {
            get
            {
                if (_records == null)
                {
                    throw new Exception("Не был вызван метод обработки строки ответа");
                }

                return _records;
            }
        }

        public FtpRecord[] FileList
        {
            get
            {
                if (_records == null)
                {
                    throw new Exception("Не был вызван метод обработки строки ответа");
                }

                return _records.Where(item => !item.IsDirectory).ToArray();
            }
        }

        public FtpRecord[] DirectoryList
        {
            get
            {
                if (_records == null)
                {
                    throw new Exception("Не был вызван метод обработки строки ответа");
                }

                return _records.Where(item => item.IsDirectory).ToArray();
            }
        }

        private readonly IRecordProcessor _recordProcessor;

        /// <summary>
        /// Конструктор парсера записей FTP-сервера.
        /// </summary>
        /// <param name="processor">Обработчик строки</param>
        public DirectoryListParser(IRecordProcessor processor)
        {
            _recordProcessor = processor;
        }

        /// <summary>
        /// Распознавание ответа FTP-сервера.
        /// </summary>
        /// <param name="responseString">Строка ответа</param>
        public void Parse(string responseString)
        {
            _records = GetRecords(responseString).ToArray();
        }

        private IEnumerable<FtpRecord> GetRecords(string datastring)
        {
            if (_recordProcessor == null)
            {
                yield break;
            }

            foreach (var s in datastring.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)))
            {
                var file = _recordProcessor.ParseRecord(s.Trim());

                if (file != null && !string.IsNullOrEmpty(file.Name) && !new[] { ".", ".." }.Contains(file.Name))
                {
                    yield return file;
                }
            }
        }
    }
}

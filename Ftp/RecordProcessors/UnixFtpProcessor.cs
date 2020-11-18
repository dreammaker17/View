using SedWin.Launcher.Utils.Ftp.RegexUtils;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SedWin.Launcher.Utils.Ftp.RecordProcessors
{
    public class UnixFtpProcessor : IRecordProcessor
    {
        private const string Month = "(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)";
        private const string Space = @"(\040)+";
        private const string Day = "([0-9]|[0-3][0-9])";
        private const string Year = "[1-2][0-9]{3}";
        private const string Time = "[0-9]{1,2}:[0-9]{2}";

        private readonly IRegexFactory _regexFactory;

        public UnixFtpProcessor(IRegexFactory regexFactory)
        {
            _regexFactory = regexFactory;
        }

        public FtpRecord ParseRecord(string record)
        {
            // Server record format:
            // dr-xr-xr-x   1 owner    group    0 Nov 25  2002 bussys
            // -rw-r--r-- 1 ftp ftp              6 Aug 25 14:58 1.txt

            if (!new[] { '-', 'd' }.Contains(record[0]))
            {
                return null;
            }

            var modifiedTime = GetDateTimeString(record);

            return new FtpRecord
            {
                Flags = record.Substring(0, 10),
                IsDirectory = record[0] == 'd',
                Owner = record.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2],
                ModifyTime = ParseDateTime(modifiedTime),
                Name = record
                    .Substring(record.IndexOf(modifiedTime) + modifiedTime.Length)
                    .Trim()
            };
        }

        private string GetDateTimeString(string record)
        {
            return _regexFactory
                .Create($"{Month}{Space}{Day}{Space}({Year}|{Time})", RegexOptions.IgnoreCase)
                .Match(record).Value;
        }

        private DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            DateTime result;

            if (_regexFactory.Create(Year, RegexOptions.None).Match(value).Success)
            {
                return DateTime.TryParse(value, out result) ? (DateTime?)result : null;
            }

            var timeString = _regexFactory.Create(Time, RegexOptions.IgnoreCase).Match(value).Value;

            value = value.Insert(value.IndexOf(timeString), $"{DateTime.Now.Year} ");

            return DateTime.TryParse(value, out result) ? (DateTime?)result : null;
        }
    }
}

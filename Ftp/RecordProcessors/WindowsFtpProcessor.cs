using System;
using System.Globalization;

namespace SedWin.Launcher.Utils.Ftp.RecordProcessors
{
    public class WindowsFtpProcessor : IRecordProcessor
    {
        public FtpRecord ParseRecord(string record)
        {
            // Server record format:
            // 02-03-04  07:46PM       <DIR>     Append
            // 06-25-09  02:41PM            144700153 image34.gif

            var recordParts = record.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            return new FtpRecord
            {
                ModifyTime = ParseDateTime(recordParts[0], recordParts[1]),
                IsDirectory = recordParts[2] == "<DIR>",
                Name = record.Remove(0, record.IndexOf(recordParts[3]))
            };
        }

        private static DateTime? ParseDateTime(string dateStr, string timeStr)
        {
            return DateTime.TryParseExact($"{dateStr} {timeStr}",
                "MM-dd-yy hh:mmtt",
                CultureInfo.GetCultureInfo("en-us"),
                DateTimeStyles.None,
                out var result) ? (DateTime?)result : null;
        }
    }
}

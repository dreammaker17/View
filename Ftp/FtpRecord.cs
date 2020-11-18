using System;

namespace SedWin.Launcher.Utils.Ftp
{
    public class FtpRecord
    {
        public string Flags { get; set; }

        public string Owner { get; set; }

        public bool IsDirectory { get; set; }

        public DateTime? ModifyTime { get; set; }

        public string Name { get; set; }

        public override string ToString() => $"{Name}, IsDirectory: {IsDirectory}, ModifyTime: {ModifyTime}, Flags: {Flags}";
    }
}

namespace SedWin.Launcher.Utils.Ftp
{
    public interface IDirectoryListParser
    {
        FtpRecord[] FullListing { get; }

        FtpRecord[] FileList { get; }

        FtpRecord[] DirectoryList { get; }

        void Parse(string responseString);
    }
}

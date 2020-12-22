namespace SedWin.Launcher.Utils.Ftp.RecordProcessors
{
    public interface IRecordProcessor
    {
        FtpRecord ParseRecord(string record);
    }
}

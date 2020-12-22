using SedWin.Launcher.Utils.Ftp.RecordProcessors;
using System;

namespace SedWin.Launcher.Utils.Ftp
{
    public static class DirectoryListParserFactory
    {
        public static Func<IRecordProcessor, IDirectoryListParser> Create = processor => new DirectoryListParser(processor);
    }
}

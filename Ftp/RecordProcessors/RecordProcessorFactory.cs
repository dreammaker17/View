using SedWin.Launcher.Utils.Ftp.RegexUtils;
using System;

namespace SedWin.Launcher.Utils.Ftp.RecordProcessors
{
    /// <summary>
    /// Абстрактная фабрика обработчика списка файлов
    /// </summary>
    public static class RecordProcessorFactory
    {
        public static Func<FtpServerTypeEnum, IRegexFactory, IRecordProcessor> Create = (serverType, regexFactory) => serverType switch
        {
            FtpServerTypeEnum.Unix => new UnixFtpProcessor(regexFactory),
            FtpServerTypeEnum.Windows => new WindowsFtpProcessor(),
            _ => null,
        };
    }
}

using System;
using System.IO;

namespace SedWin.Launcher.Utils.Ftp.Responses
{
    public interface IFtpWebResponse : IDisposable
    {
        Stream GetResponseStream();
    }
}

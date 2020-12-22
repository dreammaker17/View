using System;
using System.Net;

namespace SedWin.Launcher.Utils.Ftp.Requests
{
    public static class FtpWebRequestFactory
    {
        public static Func<string, IFtpWebRequest> Create = uri => new FtpWebRequestWrapper((FtpWebRequest)WebRequest.Create(uri));
    }
}

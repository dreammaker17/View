using System.IO;
using System.Net;

namespace SedWin.Launcher.Utils.Ftp.Responses
{
    public class FtpWebResponseWrapper : IFtpWebResponse
    {
        private readonly FtpWebResponse _response;

        public FtpWebResponseWrapper(FtpWebResponse response)
        {
            _response = response;
        }

        public void Dispose()
        {
            _response.Dispose();
        }

        public Stream GetResponseStream()
        {
            return _response.GetResponseStream();
        }
    }
}

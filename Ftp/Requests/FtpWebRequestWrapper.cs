using SedWin.Launcher.Utils.Ftp.Responses;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SedWin.Launcher.Utils.Ftp.Requests
{
    public class FtpWebRequestWrapper : IFtpWebRequest
    {
        private readonly FtpWebRequest _request;

        public FtpWebRequestWrapper(FtpWebRequest request)
        {
            _request = request;
        }

        public ICredentials Credentials
        {
            get => _request.Credentials;
            set => _request.Credentials = value;
        }

        public string Method
        {
            get => _request.Method;
            set => _request.Method = value;
        }

        public bool UsePassive
        {
            get => _request.UsePassive;
            set => _request.UsePassive = value;
        }

        public bool EnableSsl
        {
            get => _request.EnableSsl;
            set => _request.EnableSsl = value;
        }

        public long ContentLength
        {
            get => _request.ContentLength;
            set => _request.ContentLength = value;
        }

        public Task<Stream> GetRequestStream()
        {
            return _request.GetRequestStreamAsync();
        }

        public async Task<IFtpWebResponse> GetResponse()
        {
            var response = await _request.GetResponseAsync();

            return new FtpWebResponseWrapper((FtpWebResponse)response);
        }
    }
}

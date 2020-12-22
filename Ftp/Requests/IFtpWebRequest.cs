using SedWin.Launcher.Utils.Ftp.Responses;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SedWin.Launcher.Utils.Ftp.Requests
{
    public interface IFtpWebRequest
    {
        ICredentials Credentials { get; set; }

        string Method { get; set; }

        bool UsePassive { get; set; }

        bool EnableSsl { get; set; }

        long ContentLength { get; set; }

        Task<Stream> GetRequestStream();

        Task<IFtpWebResponse> GetResponse();
    }
}

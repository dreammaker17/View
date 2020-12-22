using System.Collections.Generic;
using System.Threading.Tasks;

namespace SedWin.Launcher.Utils.Ftp
{
    public interface IFtpClient
    {
        string Host { get; set; }

        string UserName { get; set; }

        string Password { get; set; }

        bool UseSsl { get; set; }

        Task<IReadOnlyCollection<FtpRecord>> ListDirectoryFiles(string path);

        Task<IReadOnlyCollection<string>> ListDirectory(string path);

        Task DownloadFile(string path, string fileName, string downloadToPath);

        Task UploadFile(string path, string fileName);

        Task DeleteFile(string path);

        Task CreateDirectory(string path, string folderName);

        Task RemoveDirectory(string path);

        Task AppendFile(string path, string content, bool newLine = false);
    }
}

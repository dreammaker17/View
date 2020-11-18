using SedWin.Launcher.Extensions;
using SedWin.Launcher.Utils.ApplicationConfiguration;
using SedWin.Launcher.Utils.Ftp.RecordProcessors;
using SedWin.Launcher.Utils.Ftp.RegexUtils;
using SedWin.Launcher.Utils.Ftp.Requests;
using SedWin.Launcher.Utils.Ftp.Responses;
using SedWin.Launcher.Utils.Wrappers.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SedWin.Launcher.Utils.Ftp
{
    public class FtpClient : IFtpClient
    {
        private readonly IRegexFactory _regexFactory;
        private readonly IFileWrapper _file;

        /// <summary>
        /// Адрес FTP-сервера
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Использование протокола SSL
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Конструктор FTP-клиента.
        /// </summary>
        /// <param name="configurationManager">Менеджер конфигурации</param>
        /// <param name="regexFactory">Фабрика регулярных выражений</param>
        public FtpClient(IApplicationConfigurationManager configurationManager, IRegexFactory regexFactory, IFileWrapper file)
        {
            UserName = configurationManager.GetSetting("ftpUserName");
            Password = configurationManager.GetSetting("ftpUserPassword");
            Host = configurationManager.GetSetting("ftpHost");

            _regexFactory = regexFactory;
            _file = file;
        }

        private async Task<IFtpWebResponse> InvokeFtpRequest(string method, string path, bool usePassive = false, byte[] requestData = null)
        {
            if (!path.StartsWith("/"))
            {
                path = $"/{path}";
            }

            var ftpRequest = FtpWebRequestFactory.Create($"ftp://{Host}{path}");

            ftpRequest.Credentials = new NetworkCredential(UserName, Password);
            ftpRequest.Method = method;
            ftpRequest.UsePassive = usePassive;
            ftpRequest.EnableSsl = UseSsl;

            if (requestData == null)
            {
                return await ftpRequest.GetResponse();
            }

            ftpRequest.ContentLength = requestData.Length;

            using var writer = await ftpRequest.GetRequestStream();

            writer.Write(requestData, 0, requestData.Length);

            return await ftpRequest.GetResponse();
        }

        /// <summary>
        /// Получение подробного списка файлов на FTP-сервере.
        /// </summary>
        /// <param name="path">Каталог на FTP-сервере</param>
        /// <returns>Подробный список файлов указанного каталога</returns>
        public async Task<IReadOnlyCollection<FtpRecord>> ListDirectoryFiles(string path)
        {
            path.AssertArgumentNotNull(nameof(path));

            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.ListDirectoryDetails, path);

            using var ftpStream = ftpResponse.GetResponseStream();
            using var reader = new StreamReader(ftpStream ?? throw new InvalidOperationException("Не удалось получить ответ сервера"));
            var list = reader.ReadToEnd();

            var serverType = GetFtpServerType(list);
            var processor = RecordProcessorFactory.Create(serverType, _regexFactory);
            var parser = DirectoryListParserFactory.Create(processor);

            parser.Parse(list);

            return parser.FullListing?.ToImmutableArray();
        }

        /// <summary>
        /// Получение списка элементов в указанном каталоге на FTP-сервере.
        /// </summary>
        /// <param name="path">Каталог на FTP-сервере</param>
        /// <returns>Список элементов указанного каталога</returns>
        public async Task<IReadOnlyCollection<string>> ListDirectory(string path)
        {
            var ftpElements = await ListDirectoryFiles(path);

            return ftpElements.Select(r => r.Name).OrderBy(n => n).ToImmutableArray();
        }

        /// <summary>
        /// Скачивание файла с FTP-сервера.
        /// </summary>
        /// <param name="path">Каталог на FTP-сервере</param>
        /// <param name="fileName">Имя скачиваемого файла</param>
        /// <param name="downloadToPath">Путь для сохранения скаченного файла</param>
        public async Task DownloadFile(string path, string fileName, string downloadToPath)
        {
            path.AssertArgumentNotNull(nameof(path));

            if (string.IsNullOrEmpty(downloadToPath))
            {
                throw new InvalidOperationException("Не указан путь для загрузки файла");
            }

            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.DownloadFile, path + fileName);
            using var ftpStream = ftpResponse.GetResponseStream() ?? throw new Exception("не удалось скачать файл");

            using var fileStream = _file.Create(downloadToPath + fileName);

            ftpStream.CopyTo(fileStream);
        }

        /// <summary>
        /// Загрузка файла на FTP-сервер.
        /// </summary>
        /// <param name="ftpPath">Путь файла на FTP-сервере</param>
        /// <param name="localName">Локальный путь файла</param>
        public async Task UploadFile(string ftpPath, string localName)
        {
            var bytes = _file.ReadAllBytes(localName);

            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.UploadFile, ftpPath + Path.GetFileName(localName), requestData: bytes);
        }

        /// <summary>
        /// Удаление файла на FTP-сервере.
        /// </summary>
        /// <param name="path">Путь файла на FTP-Сервере</param>
        public async Task DeleteFile(string path)
        {
            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.DeleteFile, path);
        }

        /// <summary>
        /// Создание каталога на FTP-сервере.
        /// </summary>
        /// <param name="path">Каталог на FTP-cервере</param>
        /// <param name="folderName">Имя нового каталога</param>
        public async Task CreateDirectory(string path, string folderName)
        {
            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.MakeDirectory, path + folderName);
        }

        /// <summary>
        /// Удаление каталога на FTP-сервере.
        /// </summary>
        /// <param name="path">Каталог на FTP-сервере</param>
        public async Task RemoveDirectory(string path)
        {
            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.RemoveDirectory, path);
        }

        /// <summary>
        /// Добавление содержимого к существующему файлу на FTP-сервере.
        /// </summary>
        /// <param name="path">Путь файла на FTP-сервере</param>
        /// <param name="content">Содержимое для добавления в конец файла</param>
        /// <param name="newLine">Добавление содержимого с новой строки</param>
        public async Task AppendFile(string path, string content, bool newLine = false)
        {
            if (newLine)
            {
                content = "\r\n" + content;
            }

            var bytes = Encoding.Default.GetBytes(content);

            using var ftpResponse = await InvokeFtpRequest(WebRequestMethods.Ftp.AppendFile, path, requestData: bytes);
        }

        public static FtpServerTypeEnum GetFtpServerType(string recordList)
        {
            var record = recordList.Split('\n').LastOrDefault(i => !string.IsNullOrWhiteSpace(i));

            if (record == null)
            {
                return FtpServerTypeEnum.Unknown;
            }

            if (Regex.IsMatch(record, "^(-|d)((-|r)(-|w)(-|x)){3}"))
            {
                return FtpServerTypeEnum.Unix;
            }

            return Regex.IsMatch(record, "^[0-9]{2}-[0-9]{2}-[0-9]{2}") ? FtpServerTypeEnum.Windows : FtpServerTypeEnum.Unknown;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SedWin.Common.Abstractions;
using SedWin.Launcher.Utils.ApplicationConfiguration;
using SedWin.Launcher.Utils.Ftp;
using SedWin.Launcher.Utils.Update;
using SedWin.Launcher.Utils.Wrappers.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.SedWin
{
    [TestClass]
    public class ApplicationClientUpdaterTests
    {
        private Mock<IApplicationConfigurationManager> _applicationConfigurationManagerMock;
        private Mock<IFtpClient> _ftpClientMock;
        private Mock<IApplicationLogger> _applicationLoggerMock;
        private Mock<IDirectoryWrapper> _directoryWrapperMock;
        private Mock<IFileWrapper> _fileWrapperMock;

        private bool _forbidUpdates;
        private bool _directoryExist;
        private bool _fileExist;
        private string _downloadToPath;
        private string _ftpUpdatesFolder;
        private IReadOnlyCollection<string> _ftpElements;

        private ApplicationClientUpdater _updater;

        [TestInitialize]
        public void Initialize()
        {
            // initialize params
            _forbidUpdates = false;
            _directoryExist = true;
            _fileExist = false; 
            _downloadToPath = "C:\\anyPath";
            _ftpUpdatesFolder = "//anyValue//";
            _ftpElements = new[] { "f1000.log", "Catalog", "file.txt", "Folder" };
            
            // ApplicationConfigurationManager mock
            _applicationConfigurationManagerMock = new Mock<IApplicationConfigurationManager>(MockBehavior.Strict);

            _applicationConfigurationManagerMock.Setup(x => x.GetSetting<bool>("forbidUpdates")).Returns(() => _forbidUpdates);
            _applicationConfigurationManagerMock.Setup(x => x.GetSetting("ftpUpdatesFolder")).Returns(() => _ftpUpdatesFolder);
            _applicationConfigurationManagerMock.Setup(x => x.GetClientApplicationFolder()).Returns(() => _downloadToPath);

            // FtpClient mock
            _ftpClientMock = new Mock<IFtpClient>(MockBehavior.Strict);

            _ftpClientMock.Setup(x => x.ListDirectory(It.IsAny<string>()))
                .Returns(() => Task.FromResult(_ftpElements));

            _ftpClientMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);

            // FileWrapper mock
            _fileWrapperMock = new Mock<IFileWrapper>(MockBehavior.Strict);

            _fileWrapperMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(() => _fileExist);

            // ApplicationLogger mock
            _applicationLoggerMock = new Mock<IApplicationLogger>(MockBehavior.Strict);

            _applicationLoggerMock.Setup(x => x.LogException(It.IsAny<Exception>(), It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);

            // DirectoryWrapper mock 
            _directoryWrapperMock = new Mock<IDirectoryWrapper>(MockBehavior.Strict);

            _directoryWrapperMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(() => _directoryExist);
            _directoryWrapperMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            // new ApplicationClientUpdater
            _updater = new ApplicationClientUpdater(_applicationConfigurationManagerMock.Object, _ftpClientMock.Object, _applicationLoggerMock.Object, _directoryWrapperMock.Object, _fileWrapperMock.Object);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_SettingIsTrue_ReturnedFalse()
        {
            // arrange
            _forbidUpdates = true;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_ParametersIsCorrect_ReturnedTrue()
        {
            // arrange
            _ftpClientMock.Setup(x => x.DownloadFile("//anyValue//", "f1000.log", "C:\\anyPath\\"))
                .Returns((string path, string fileName, string downloadToPath) => Task.CompletedTask);

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_SettingsIsTrue_ReturnedFalse()
        {
            // arrange
            _forbidUpdates = true;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_ServerDirectoryIsEmpty_ReturnedFalse()
        {
            // arrange
            _ftpElements = new string[0];

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_FilesVersionIsZero_ReturnedFalse()
        {
            // arrange
            _ftpElements = new[] { "Catalog", "file.txt", "Folder" };

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_ListDirectoryIsCorrect_ReturnedTrue()
        {
            // arrange
            _ftpUpdatesFolder = "//anyValue//";

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _ftpClientMock.Verify(x => x.ListDirectory("//anyValue//"), Times.Once);
            _ftpClientMock.Verify(x => x.ListDirectory(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_DirectoryIsNotExists_ReturnedFalse()
        {
            // arrange
            _downloadToPath = null;
            _directoryExist = false;

            _directoryWrapperMock.Setup(x => x.CreateDirectory(null)).Throws(new ArgumentNullException());

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_CreateDirectoryNotBeInvoked_ReturnedTrue()
        {
            // arrange
            _directoryExist = true;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _directoryWrapperMock.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_FileIsExists_ReturnedFalse()
        {
            // arrange
            _downloadToPath = null;
            _fileExist = true;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_GroupByFileNames_ReturnedTrue()
        {
            // arrange
            _ftpElements = new[] { "f1000.log", "f1001.log", "f.log", "Z2000.txt", "Z2001.txt" };

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "f1001.log", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "Z2001.txt", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_SelectFileWithLastVersion_ReturnedTrue()
        {
            // arrange
            _ftpElements = new[] { "f1000.log", "f3000.log", "Folder1", "Folder" };
            _fileExist = false;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "f3000.log", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_DownloadPathIsNull_ReturnedTrue()
        {
            // arrange
            _downloadToPath = null;
            _fileExist = false;

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "f1000.log", "\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_ThrowsException_ReturnedFalse()
        {
            // arrange
            _ftpClientMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_FilesWasDownloaded3Times_ReturnedTrue()
        {
            // arrange
            _ftpElements = new[] { "f1000.log", "f1001.log", "f.log", "Z2000.txt", "Z2001.txt", "X1000.log" };

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "f1001.log", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "Z2001.txt", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile("//anyValue//", "X1000.log", "C:\\anyPath\\"), Times.Once);
            _ftpClientMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_FilesToDownloadIsNull_ReturnedFalse()
        {
            // arrange
            _ftpElements = new[] { "Folder1", "Folder" };

            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsFalse(updateResult);

            _ftpClientMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_FileDownloadToPathIsCorrect_ReturnedTrue()
        {
            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _fileWrapperMock.Verify(x => x.Exists("C:\\anyPath\\f1000.log"), Times.Once);
            _fileWrapperMock.Verify(x => x.Exists(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadLatestVersionFiles_DirectoryDownloadToPathIsCorrect_ReturnedTrue()
        {
            // act
            var updateResult = await _updater.DownloadLatestVersionFiles();

            // assert
            Assert.IsTrue(updateResult);

            _directoryWrapperMock.Verify(x => x.Exists("C:\\anyPath"), Times.Once);
            _directoryWrapperMock.Verify(x => x.Exists(It.IsAny<string>()), Times.Once);
        }
    }
}

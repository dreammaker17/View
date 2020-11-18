using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SedWin.Launcher.Utils.ApplicationConfiguration;
using SedWin.Launcher.Utils.Wrappers.Abstractions;
using System.Collections.Generic;

namespace UnitTests.SedWin
{
    /// <summary>
    /// Unit Tests ApplicationConfigurationManager
    /// </summary>
    [TestClass]
    public class ApplicationConfigurationManagerTests
    {
        private Mock<IConfigurationWrapper> configurationWrapperMock;
        private Mock<IApplicationWrapper> applicationWrapperMock;
        private Mock<IDirectoryWrapper> directoryWrapperMock;

        private string applicationStartupPath = "\\anyPath";
        private Dictionary<string, string> appSettingsDictionary;
        private string[] stubFileNames;

        private ApplicationConfigurationManager applicationConfigurationManager;

        [TestInitialize]
        public void Initialize()
        {
            // initialize dictionary
            appSettingsDictionary = new Dictionary<string, string>();

            // initialize array
            stubFileNames = new[] { "2.txt", "3.txt", "1.txt" };

            // ConfigurationWrapper mock
            configurationWrapperMock = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            configurationWrapperMock.Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => appSettingsDictionary.ContainsKey(key) ? appSettingsDictionary[key] : (string)null);

            // ApplicationWrapper mock
            applicationWrapperMock = new Mock<IApplicationWrapper>(MockBehavior.Strict);
            applicationWrapperMock.Setup(x => x.StartupPath).Returns(applicationStartupPath);

            // DirectoryWrapper mock
            directoryWrapperMock = new Mock<IDirectoryWrapper>(MockBehavior.Strict);

            // new ApplicationConfigurationManager
            applicationConfigurationManager = new ApplicationConfigurationManager(configurationWrapperMock.Object, applicationWrapperMock.Object, directoryWrapperMock.Object);
        }

        [TestMethod]
        public void GetSetting_SettingIsExist_GotSetting()
        {
            // arrange
            appSettingsDictionary["anyKey"] = "anyValue";

            // act
            var anySettingValue = applicationConfigurationManager.GetSetting("anyKey");

            // assert
            Assert.AreEqual("anyValue", anySettingValue);
        }

        [TestMethod]
        public void GetSetting_SettingsIsConvertible_GotConvertibledSetting()
        {
            // arrange
            appSettingsDictionary["anyKey"] = "123458";

            // act
            var anySettingValue = applicationConfigurationManager.GetSetting<int>("anyKey");

            // assert
            Assert.AreEqual(123458, anySettingValue);
        }

        [TestMethod]
        public void GetSetting_SettingIsNotExist_GotStringEmpty()
        {
            // arrange
            appSettingsDictionary["anyKey"] = "anyValue";

            // act
            var anySettingValue = applicationConfigurationManager.GetSetting("notAnyKey");

            // assert
            Assert.IsTrue(string.IsNullOrEmpty(anySettingValue));
        }

        [TestMethod]
        public void GetSetting_SettingsIsNull_GotDefaultValue()
        {
            // arrange
            appSettingsDictionary["anyKey"] = null;

            // act
            var defaultString = applicationConfigurationManager.GetSetting<string>("anyKey");

            // assert
            Assert.IsNull(defaultString);
        }

        [TestMethod]
        public void GetClientApplicationFolder_ConstIsNotStringEmpty_GotSetting()
        {
            // arrange
            appSettingsDictionary["winClientFolder"] = "anyValue";

            // act
            var applicationFolder = applicationConfigurationManager.GetClientApplicationFolder();

            // assert
            Assert.AreEqual($"{applicationStartupPath}\\anyValue", applicationFolder);
        }

        [TestMethod]
        public void GetClientApplicationFolder_ConstIsStringEmpty_GotDefaultSetting()
        {
            // arrange
            appSettingsDictionary["anyKey"] = string.Empty;

            // act
            var applicationFolder = applicationConfigurationManager.GetClientApplicationFolder();

            // assert
            Assert.AreEqual($"{applicationStartupPath}{"\\SedWinClient"}", applicationFolder);
        }

        [TestMethod]
        public void GetClientApplicationFolder_SettingIsContainsValue_GotSimpleSetting()
        {
            // arrange
            appSettingsDictionary["winClientFolder"] = "C:\\anyValue";

            // act
            var applicationFolder = applicationConfigurationManager.GetClientApplicationFolder();

            // assert
            Assert.AreEqual("C:\\anyValue", applicationFolder);
        }

        [TestMethod]
        public void GetClientLogsFolder_ConstIsNotStringEmpty_GotSetting()
        {
            // arrange
            appSettingsDictionary["logsFolder"] = "anyValue";

            // act
            var clientLogsFolder = applicationConfigurationManager.GetClientLogsFolder();

            // assert
            Assert.AreEqual($"{applicationStartupPath}\\anyValue", clientLogsFolder);
        }

        [TestMethod]
        public void GetClientLogsFolder_SettingIsContainsValue_GotSimpleSetting()
        {
            // arrange
            appSettingsDictionary["logsFolder"] = "C:\\anyValue";

            // act
            var clientLogsFolder = applicationConfigurationManager.GetClientLogsFolder();

            // assert
            Assert.AreEqual("C:\\anyValue", clientLogsFolder);
        }

        [TestMethod]
        public void GetClientLogsFolder_ConstIsStringEmpty_GotDefaultSetting()
        {
            // arrange
            appSettingsDictionary["anyKey"] = string.Empty;

            // act
            var clientLogsFolder = applicationConfigurationManager.GetClientLogsFolder();

            // assert
            Assert.AreEqual($"{applicationStartupPath}{"\\Logs"}", clientLogsFolder);
        }

        [TestMethod]
        public void GetClientFileName_DirectoryINotEmpty_GotLastFileName()
        {
            // arrange
            appSettingsDictionary["winClientFolder"] = "anyValue";
            directoryWrapperMock.Setup(x => x.GetFiles("\\anyPath\\anyValue", "SedWinClient*"))
                .Returns(() => stubFileNames);

            // act
            var fileNames = applicationConfigurationManager.GetClientFileName();

            // assert
            Assert.AreEqual("3.txt", fileNames);
        }

        [TestMethod]
        public void GetClientFileName_DirectoryIsEmpty_GotStringEmpty()
        {
            // arrange
            directoryWrapperMock.Setup(x => x.GetFiles(It.IsAny<string>(), "SedWinClient*"))
                .Returns(() => new string[0]);

            // act
            var fileNames = applicationConfigurationManager.GetClientFileName();

            // assert
            Assert.AreEqual(string.Empty, fileNames);
        }
    }
}

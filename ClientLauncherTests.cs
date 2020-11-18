using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SedWin.Common.Abstractions;
using SedWin.Common.Types;
using SedWin.Launcher.Utils.ApplicationConfiguration;
using SedWin.Launcher.Utils.Launcher;
using SedWin.Launcher.Utils.Wrappers;
using SedWin.Launcher.Utils.Wrappers.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UnitTests.SedWin
{
    [TestClass]
    public class ClientLauncherTests
    {
        private Mock<IApplicationConfigurationManager> _applicationConfigurationManagerMock;
        private Mock<IApplicationClientUpdater> _applicationClientUpdaterMock;
        private Mock<IApplicationLogger> _applicationLoggerMock;
        private Mock<IAssemblyWrapper> _assemblyWrapperMock;
        private Mock<ITypeWrapper> _typeWrapperMock;
        private Mock<IConstructorInfoWrapper> _constructorInfoWrapperMock;
        private Mock<IClient> _clientMock;

        private ClientLauncher _clientLauncher;

        private bool _forbidUpdates;
        private bool _isJustUpdated;
        private string _fileName = "C:\\file.txt";
        private Type _type;
        private IConstructorInfoWrapper[] _constructorInfoWrapperArray;
        private ITypeWrapper[] _typeWrapperArray;

        [TestInitialize]
        public void Initialize()
        {
            // initialize 
            _forbidUpdates = false;
            _isJustUpdated = false;
            _type = typeof(IClient);

            // IApplicationConfigurationManager mock
            _applicationConfigurationManagerMock = new Mock<IApplicationConfigurationManager>(MockBehavior.Strict);

            _applicationConfigurationManagerMock.Setup(x => x.GetClientFileName()).Returns(() => _fileName);
            _applicationConfigurationManagerMock.Setup(x => x.GetSetting<bool>("forbidUpdates")).Returns(() => _forbidUpdates);

            // IClient mock
            _clientMock = new Mock<IClient>(MockBehavior.Strict);

            _clientMock.Setup(x => x.Start(It.IsAny<StartOptions>()));

            // IApplicationClientUpdater mock
            _applicationClientUpdaterMock = new Mock<IApplicationClientUpdater>(MockBehavior.Strict);

            // IConstructorInfoWrapper mock
            _constructorInfoWrapperMock = new Mock<IConstructorInfoWrapper>(MockBehavior.Strict);

            _constructorInfoWrapperMock.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(() => _clientMock.Object);

            // initialize constructorInfoWrappersArray
            _constructorInfoWrapperArray = new[] { _constructorInfoWrapperMock.Object };

            // IApplicationLogger mock
            _applicationLoggerMock = new Mock<IApplicationLogger>(MockBehavior.Strict);

            _applicationLoggerMock.Setup(x => x.LogException(It.IsAny<Exception>(), It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);

            // ITypeWrapper mock
            _typeWrapperMock = new Mock<ITypeWrapper>(MockBehavior.Strict);

            _typeWrapperMock.Setup(x => x.GetConstructors()).Returns(() => _constructorInfoWrapperArray);
            _typeWrapperMock.Setup(x => x.GetInterface(It.IsAny<string>())).Returns(() => _type);

            // initialize typeWrappersArray
            _typeWrapperArray = new[] { _typeWrapperMock.Object };

            // IAssemblyWrapper mock
            _assemblyWrapperMock = new Mock<IAssemblyWrapper>(MockBehavior.Strict);

            _assemblyWrapperMock.Setup(x => x.GetTypes()).Returns(() => _typeWrapperArray);

            AssemblyWrapper.LoadFrom = (assemblyFile) =>
                assemblyFile == _fileName ? _assemblyWrapperMock.Object : throw new ArgumentNullException();

            // new ClientLauncher
            _clientLauncher = new ClientLauncher(_applicationConfigurationManagerMock.Object, _applicationClientUpdaterMock.Object, _applicationLoggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            AssemblyWrapper.LoadFrom = (assemblyFile) => new AssemblyWrapper(Assembly.LoadFrom(assemblyFile));
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void Launch_IsJustUpdatedValueSet_ClientStartsWithSpecifiedParameters(bool isJustUpdated)
        {
            // arrange
            _isJustUpdated = isJustUpdated;

            // act
            _clientLauncher.Launch(_isJustUpdated);

            // assert
            _clientMock.Verify(x => x.Start(It.Is<StartOptions>(s => s.ForbidUpdates == _forbidUpdates && s.IsJustUpdated == _isJustUpdated)), Times.Once);
            _clientMock.Verify(x => x.Start(It.IsAny<StartOptions>()), Times.Once);
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void Launch_ForbidUpdatesValueSet_ClientStartsWithSpecifiedParameters3(bool forbidUpdates)
        {
            // arrange
            _forbidUpdates = forbidUpdates;

            // act
            _clientLauncher.Launch(_isJustUpdated);

            // assert
            _clientMock.Verify(x => x.Start(It.Is<StartOptions>(s => s.ForbidUpdates == _forbidUpdates && s.IsJustUpdated == _isJustUpdated)), Times.Once);
            _clientMock.Verify(x => x.Start(It.IsAny<StartOptions>()), Times.Once);
        }

        [TestMethod]
        public void Launch_GetClientFileNameReturnsNull_ThrowsException()
        {
            // arrange
            _applicationConfigurationManagerMock.Setup(x => x.GetClientFileName()).Returns(() => null);

            // act
            var ex = Assert.ThrowsException<Exception>(() => _clientLauncher.Launch());

            // assert
            Assert.AreEqual("Не удалось подключить функциональный модуль", ex.Message);
        }

        [TestMethod]
        public void Launch_GetTypesReturnsEmptyArray_ThrowsException()
        {
            // arrange
            _typeWrapperArray = new ITypeWrapper[0];

            // act
            var ex = Assert.ThrowsException<Exception>(() => _clientLauncher.Launch());

            // assert
            Assert.AreEqual("Не удалось подключить функциональный модуль", ex.Message);
        }

        [TestMethod]
        public void Launch_GetInterfaceReturnsNull_ThrowsException()
        {
            // arrange
            _type = null;

            // act
            var ex = Assert.ThrowsException<Exception>(() => _clientLauncher.Launch());

            // assert
            Assert.AreEqual("Не удалось подключить функциональный модуль", ex.Message);
        }

        [TestMethod]
        public void Launch_GetConstructorsReturnsEmptyArray_ThrowsException()
        {
            // arrange
            _constructorInfoWrapperArray = new IConstructorInfoWrapper[0];

            // act
            var ex = Assert.ThrowsException<Exception>(() => _clientLauncher.Launch());

            // assert
            Assert.AreEqual("Не удалось подключить функциональный модуль", ex.Message);
        }

        [TestMethod]
        public void Launch_GetConstructorsContainsMoreThanOne_ThrowsException()
        {
            // arrange
            _constructorInfoWrapperArray = new[] { _constructorInfoWrapperMock.Object, new Mock<IConstructorInfoWrapper>().Object };

            // act
            var ex = Assert.ThrowsException<Exception>(() => _clientLauncher.Launch());

            // assert
            Assert.AreEqual("Не удалось подключить функциональный модуль", ex.Message);
        }

        [TestMethod]
        public void Launch_AllIsCorrect_InvokeWithCorrectParameters()
        {
            // arrange

            // act
            _clientLauncher.Launch();

            // assert
            _constructorInfoWrapperMock.Verify(x => x.Invoke(It.Is<object[]>(o => o[0] == _applicationClientUpdaterMock.Object && o[1] == _clientLauncher && o[2] == _applicationLoggerMock.Object)), Times.Once);
        }
    }
}

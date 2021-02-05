using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
        [TestMethod]
        public async Task ProcessMessage_ReturnsExpectedResult()
        {
            // Arrange
            var expected = "Hello, world!";

            var mockLogger = Mock.Of<ILogger<IContractService>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            var contractService = new ContractService(mockLogger);
            var actual = await contractService.ProcessMessage(expected);

            // Assert
            actual.Should().Be(expected);
            Mock.Get(mockLogger).Verify();
        }

        [TestMethod]
        public void ProcessMessage_ThrowsException()
        {
            // Arrange
            var expectedToThrowException = "throw-exception \"ExampleSequenceId\":0";
            var mockLogger = Mock.Of<ILogger<IContractService>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            var contractService = new ContractService(mockLogger);
            Func<Task<string>> act = async () => await contractService.ProcessMessage(expectedToThrowException);

            // Assert
            act.Should().Throw<Exception>();
            Mock.Get(mockLogger).Verify();
        }
    }
}
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
using Pds.Contracts.Data.Api.Client.Models;
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
            var dummyContractEvent = new ContractEvent { ContractNumber = "test", ContractVersion = 1 };
            var dummyContract = new Contract();

            var mockLogger = Mock.Of<ILogger<IContractService>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var mockDataService = Mock.Of<IContractsDataService>(MockBehavior.Strict);
            Mock.Get(mockDataService)
                .Setup(s => s.GetContractByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(dummyContract)
                .Verifiable();

            // Act
            var contractService = new ContractService(mockLogger, mockDataService);
            await contractService.ProcessMessage(dummyContractEvent);

            // Assert
            Mock.Get(mockLogger).Verify();
            Mock.Get(mockDataService).Verify();
        }

        [TestMethod]
        public void ProcessMessage_ThrowsException()
        {
            // Arrange
            var dummyContractEvent = new ContractEvent { ContractNumber = "test", ContractVersion = 1 };
            var mockLogger = Mock.Of<ILogger<IContractService>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var mockDataService = Mock.Of<IContractsDataService>(MockBehavior.Strict);
            Mock.Get(mockDataService)
                .Setup(s => s.GetContractByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(default(Contract))
                .Verifiable();

            // Act
            var contractService = new ContractService(mockLogger, mockDataService);
            Func<Task> act = async () => await contractService.ProcessMessage(dummyContractEvent);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
            Mock.Get(mockLogger).Verify();
            Mock.Get(mockDataService).Verify();
        }
    }
}
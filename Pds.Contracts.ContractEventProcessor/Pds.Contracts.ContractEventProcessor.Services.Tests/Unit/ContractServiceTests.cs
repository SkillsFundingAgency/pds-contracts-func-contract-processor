using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
using System;
using System.Threading.Tasks;
using ClientData = Pds.Contracts.Data.Api.Client;

namespace Pds.Contracts.ContractEventProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
        private readonly IContractCreationService _mockContractCreationService
            = Mock.Of<IContractCreationService>(MockBehavior.Strict);

        private readonly ILogger<IContractService> _mockContractServiceLogger
                       = Mock.Of<ILogger<IContractService>>(MockBehavior.Strict);

        private readonly ILogger<IContractApprovalService> _mockContractApprovalServiceLogger
                = Mock.Of<ILogger<IContractApprovalService>>(MockBehavior.Strict);

        private readonly ILogger<IContractCreationService> _mockContractCreationServiceLogger
        = Mock.Of<ILogger<IContractCreationService>>(MockBehavior.Strict);

        private readonly IContractService _mockContractsService
           = Mock.Of<IContractService>(MockBehavior.Strict);

        private readonly IContractApprovalService _mockContractsApprovalService
           = Mock.Of<IContractApprovalService>(MockBehavior.Strict);

        private readonly IContractsDataService _mockContractsDataService
           = Mock.Of<IContractsDataService>(MockBehavior.Strict);

        private readonly IValidationService _mockValidationService
           = Mock.Of<IValidationService>(MockBehavior.Strict);

        [TestMethod]
        public void ProcessMessage_Creation_ReturnsExpectedResult()
        {
            // Arrange
            ContractParentStatus parentStatus = ContractParentStatus.Approved;
            ContractStatus contractStatus = ContractStatus.Approved;
            ContractAmendmentType amendmentType = ContractAmendmentType.Notfication;
            var dummyContractEvent = GetContractEvent(parentStatus, contractStatus, amendmentType);
            ClientData.Models.Contract dummyContract = null;

            SetMockSetup_ServiceLogger(LogLevel.Information);

            MockValidationService_ValidateStatus(ContractEventType.Creation, parentStatus, contractStatus, amendmentType);

            Mock.Get(_mockContractCreationService)
                .Setup(s => s.CreateAsync(It.IsAny<ContractEvent>()))
                .ReturnsAsync(true)
                .Verifiable();

            Mock.Get(_mockContractsDataService)
               .Setup(s => s.TryGetContractAsync(It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(dummyContract)
               .Verifiable();

            var contractService = new ContractService(_mockContractServiceLogger, _mockContractsDataService, _mockContractsApprovalService, _mockValidationService, _mockContractCreationService);


            // Act
            contractService.ProcessMessage(dummyContractEvent).GetAwaiter().GetResult();

            // Assert
            Mock.Get(_mockValidationService)
                .Verify(d => d.GetContractEventType(It.IsAny<ContractEvent>()), Times.Once);
            Mock.Get(_mockContractsDataService)
               .Verify(s => s.TryGetContractAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractCreationService)
              .Verify(s => s.CreateAsync(It.IsAny<ContractEvent>()), Times.Once);
            Mock.Get(_mockContractServiceLogger).VerifyAll();
        }

        [TestMethod]
        public void ProcessMessage_Creation_ReturnsLogAndExitResult()
        {
            // Arrange
            ContractParentStatus parentStatus = ContractParentStatus.Approved;
            ContractStatus contractStatus = ContractStatus.Approved;
            ContractAmendmentType amendmentType = ContractAmendmentType.Notfication;
            var dummyContractEvent = GetContractEvent(parentStatus, contractStatus, amendmentType);
            var dummyContract = GetClientContract();

            SetMockSetup_ServiceLogger(LogLevel.Information);
            SetMockSetup_ServiceLogger(LogLevel.Warning);

            MockValidationService_ValidateStatus(ContractEventType.Creation, parentStatus, contractStatus, amendmentType);

            Mock.Get(_mockContractCreationService)
                .Setup(s => s.CreateAsync(It.IsAny<ContractEvent>()))
                .ReturnsAsync(true)
                .Verifiable();

            Mock.Get(_mockContractsDataService)
               .Setup(s => s.TryGetContractAsync(It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(dummyContract)
               .Verifiable();

            var contractService = new ContractService(_mockContractServiceLogger, _mockContractsDataService, _mockContractsApprovalService, _mockValidationService, _mockContractCreationService);


            // Act
            contractService.ProcessMessage(dummyContractEvent).GetAwaiter().GetResult();

            // Assert
            Mock.Get(_mockValidationService)
                .Verify(d => d.GetContractEventType(It.IsAny<ContractEvent>()), Times.Once);
            Mock.Get(_mockContractsDataService)
               .Verify(s => s.TryGetContractAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractCreationService)
                .Verify(s => s.CreateAsync(It.IsAny<ContractEvent>()), Times.Never);
            Mock.Get(_mockContractServiceLogger).VerifyAll();
        }

        [TestMethod]
        public void ProcessMessage_ThrowsException()
        {
            // Arrange
            ContractParentStatus parentStatus = ContractParentStatus.Approved;
            ContractStatus contractStatus = ContractStatus.Replaced;
            ContractAmendmentType amendmentType = ContractAmendmentType.None;
            var dummyContractEvent = GetContractEvent(parentStatus, contractStatus, amendmentType);

            SetMockSetup_ServiceLogger(LogLevel.Information);

            //SetMockSetup_ServiceLogger(LogLevel.Warning);
            MockValidationService_ValidateStatus_Exception(ContractEventType.Approval, parentStatus, contractStatus, amendmentType);

            Mock.Get(_mockContractCreationService)
                .Setup(s => s.CreateAsync(It.IsAny<ContractEvent>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act
            var contractService = new ContractService(_mockContractServiceLogger, _mockContractsDataService, _mockContractsApprovalService, _mockValidationService, _mockContractCreationService);
            Func<Task> act = async () => await contractService.ProcessMessage(dummyContractEvent);

            // Assert
            act.Should().Throw<ContractExpectationFailedException>();
            Mock.Get(_mockContractServiceLogger).VerifyAll();
        }

        private void MockValidationService_ValidateStatus(ContractEventType requiredEventType, ContractParentStatus parentStatus, ContractStatus contractStatus, ContractAmendmentType amendmentType)
        {
            Mock.Get(_mockValidationService)
                .Setup(x => x.GetContractEventType(It.IsAny<ContractEvent>()))
                .Returns(requiredEventType)
                .Verifiable();
        }

        private void MockValidationService_ValidateStatus_Exception(ContractEventType requiredEventType, ContractParentStatus parentStatus, ContractStatus contractStatus, ContractAmendmentType amendmentType)
        {
            Mock.Get(_mockValidationService)
                .Setup(x => x.GetContractEventType(It.IsAny<ContractEvent>()))
                .Throws(new ContractExpectationFailedException("anumber", 1, "anumber"))
                .Verifiable();
        }

        private ClientData.Models.Contract GetClientContract()
        {
            return new ClientData.Models.Contract()
            {
                Id = 1,
                ContractNumber = "ABC-0000",
                ContractVersion = 1
            };
        }

        private ContractEvent GetContractEvent(ContractParentStatus parentStatus, ContractStatus contractStatus, ContractAmendmentType amendmentType)
        {
            return new ContractEvent()
            {
                ParentStatus = parentStatus,
                Status = contractStatus,
                AmendmentType = amendmentType,
                ContractNumber = "ABC-0000",
                ContractVersion = 1,
                ContractEventXml = "sample-blob-file.xml"
            };
        }

        private void SetMockSetup_ServiceLogger(LogLevel logLevel)
        {
            Mock.Get(_mockContractServiceLogger)
            .Setup(logger => logger.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        private void SetMockSetup_ApprovalServiceLogger(LogLevel logLevel)
        {
            Mock.Get(_mockContractApprovalServiceLogger)
            .Setup(logger => logger.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        private void SetMockSetup_CreationServiceLogger(LogLevel logLevel)
        {
            Mock.Get(_mockContractCreationServiceLogger)
            .Setup(logger => logger.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
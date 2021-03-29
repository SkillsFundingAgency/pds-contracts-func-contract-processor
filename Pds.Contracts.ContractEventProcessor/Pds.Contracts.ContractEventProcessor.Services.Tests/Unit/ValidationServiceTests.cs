using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Models;
using System;
using ClientEnums = Pds.Contracts.Data.Api.Client.Enumerations;
using LocalEnums = Pds.Contracts.ContractEventProcessor.Common.Enums;

namespace Pds.Contracts.ContractEventProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ValidationServiceTests
    {
        private readonly ILogger<IValidationService> _mockLogger
              = Mock.Of<ILogger<IValidationService>>(MockBehavior.Strict);

        [TestMethod]
        [DataRow(
            LocalEnums.ContractParentStatus.Draft,
            LocalEnums.ContractStatus.PublishedToProvider,
            LocalEnums.ContractAmendmentType.None)]
        [DataRow(
            LocalEnums.ContractParentStatus.Draft,
            LocalEnums.ContractStatus.PublishedToProvider,
            LocalEnums.ContractAmendmentType.Variation)]
        [DataRow(
            LocalEnums.ContractParentStatus.Approved,
            LocalEnums.ContractStatus.Approved,
            LocalEnums.ContractAmendmentType.Notfication)]
        [DataRow(
            LocalEnums.ContractParentStatus.Approved,
            LocalEnums.ContractStatus.Modified,
            LocalEnums.ContractAmendmentType.Notfication)]
        [DataRow(
            LocalEnums.ContractParentStatus.Approved,
            LocalEnums.ContractStatus.UnderTermination,
            LocalEnums.ContractAmendmentType.Notfication)]
        public void GetContractEventType_Creation_ExpectedResult_Test(LocalEnums.ContractParentStatus parentStatus, LocalEnums.ContractStatus contractStatus, LocalEnums.ContractAmendmentType amendmentType)
        {
            //Arrange
            SetupLogger_LogInformation();
            var expectedEventType = LocalEnums.ContractEventType.Creation;
            var contractEvent = GetContractEvent(parentStatus, contractStatus, amendmentType);
            var service = new ValidationService(_mockLogger);

            //Act
            var actResult = service.GetContractEventType(contractEvent);

            //Assert
            actResult.Should().Be(expectedEventType);
        }

        [TestMethod]
        [DataRow(
            LocalEnums.ContractParentStatus.Draft,
            LocalEnums.ContractStatus.WithdrawnByAgency,
            LocalEnums.ContractAmendmentType.None)]
        public void GetContractEventType_ExpectedException_Test(LocalEnums.ContractParentStatus parentStatus, LocalEnums.ContractStatus contractStatus, LocalEnums.ContractAmendmentType amendmentType)
        {
            //Arrange
            SetupLogger_LogInformation();
            var contractEvent = GetContractEvent(parentStatus, contractStatus, amendmentType);
            var service = new ValidationService(_mockLogger);

            //Act
            Action act = () => service.GetContractEventType(contractEvent);

            //Assert
            act.Should().Throw<ContractExpectationFailedException>();
        }

        private void SetupLogger_LogInformation()
        {
            Mock.Get(_mockLogger)
               .Setup(logger => logger.Log(
               It.Is<LogLevel>(l => l == LogLevel.Information),
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        private Contract GetContract(ClientEnums.ContractStatus contractStatus)
        {
            return new Contract()
            {
                ContractNumber = "ABC-0001",
                ContractVersion = 1,
                Status = contractStatus
            };
        }

        private ContractEvent GetContractEvent(
            LocalEnums.ContractParentStatus parentStatus,
            LocalEnums.ContractStatus contractStatus,
            LocalEnums.ContractAmendmentType amendmentType)
        {
            return new ContractEvent()
            {
                ParentStatus = parentStatus,
                Status = contractStatus,
                AmendmentType = amendmentType,
                ContractEventXml = "sample-blob-file.xml",
                ContractNumber = "ABC-0001",
                ContractVersion = 1
            };
        }
    }
}
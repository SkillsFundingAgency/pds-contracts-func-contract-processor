using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Func.Tests.Unit
{
    [TestClass]
    public class ContractEventProcessorFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var state = new SessionWorkflowState();
            var dummyMessage = new Message { MessageId = "1" };

            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            Mock.Get(mockSession)
                .SetupGet(s => s.SessionId)
                .Returns("session-1");

            var mockContractEventSessionManager = Mock.Of<IContractEventSessionManager>(MockBehavior.Strict);
            Mock.Get(mockContractEventSessionManager)
                .Setup(m => m.ProcessSessionMessageAsync(mockSession, dummyMessage))
                .ReturnsAsync(state)
                .Verifiable();

            var mockLogger = Mock.Of<ILogger>(MockBehavior.Strict);

            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var function = new ContractEventProcessorFunction(mockContractEventSessionManager);

            // Act
            Func<Task> act = async () => { await function.Run(dummyMessage, mockSession, mockLogger); };

            // Assert
            act.Should().NotThrow();
            Mock.Get(mockContractEventSessionManager).Verify();

            Mock.Get(mockLogger)
                .Verify(
                    l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Exactly(2));

            Mock.Get(mockSession)
                .VerifyGet(s => s.SessionId, Times.Exactly(2));
        }
    }
}
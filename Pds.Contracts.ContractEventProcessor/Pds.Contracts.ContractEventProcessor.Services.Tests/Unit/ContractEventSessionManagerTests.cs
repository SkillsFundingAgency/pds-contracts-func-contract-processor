using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.ContractEventProcessor.Services.Implementations;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractEventSessionManagerTests
    {
        [TestMethod]
        public async Task ProcessSessionMessageAsync_ReturnsExpectedResultTest()
        {
            // Arrange
            var expectedBodyMessage = "test";
            var expected = new SessionWorkflowState();
            var dummyMessage = GetDummyMessage(expectedBodyMessage);

            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            var mockLogger = Mock.Of<ILogger<IContractEventSessionManager>>(MockBehavior.Strict);
            var mockContractService = Mock.Of<IContractService>(MockBehavior.Strict);
            Mock.Get(mockContractService)
                .Setup(c => c.ProcessMessage(expectedBodyMessage))
                .ReturnsAsync(expectedBodyMessage)
                .Verifiable();

            var mockStateManager = Mock.Of<IWorkflowStateManager>(MockBehavior.Strict);
            Mock.Get(mockStateManager)
                .Setup(s => s.GetWorkflowStateAsync(mockSession))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var sessionManager = new ContractEventSessionManager(mockStateManager, mockContractService, mockLogger);
            var actual = await sessionManager.ProcessSessionMessageAsync(mockSession, dummyMessage);

            // Assert
            actual.Should().Be(expected);
            Mock.Get(mockContractService).Verify();
            Mock.Get(mockStateManager).Verify();
        }

        [TestMethod]
        public async Task ProcessSessionMessageAsync_SessionStateIsFaultedAndNotFailedMessage_ForwardsMessageToDLQTest()
        {
            // Arrange
            var expectedBodyMessage = "test";
            var dummyMessage = GetDummyMessage(expectedBodyMessage);
            var initialState = new SessionWorkflowState
            {
                IsFaulted = true,
                FailedMessageId = "Not-same-as-dummy-message"
            };

            var expectedState = new SessionWorkflowState
            {
                IsFaulted = true,
                FailedMessageId = "Not-same-as-dummy-message",
                PostponedMessages = new List<string> { dummyMessage.MessageId }
            };

            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            Mock.Get(mockSession)
                .Setup(s => s.DeadLetterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(mockSession)
                .SetupGet(s => s.SessionId)
                .Returns("session-id")
                .Verifiable();

            var mockLogger = Mock.Of<ILogger<IContractEventSessionManager>>(MockBehavior.Strict);
            var mockContractService = Mock.Of<IContractService>(MockBehavior.Strict);
            var mockStateManager = Mock.Of<IWorkflowStateManager>(MockBehavior.Strict);
            Mock.Get(mockStateManager)
                .Setup(s => s.GetWorkflowStateAsync(mockSession))
                .ReturnsAsync(initialState)
                .Verifiable();

            Mock.Get(mockStateManager)
                .Setup(s => s.SetWorkflowStateAsync(mockSession, initialState))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var sessionManager = new ContractEventSessionManager(mockStateManager, mockContractService, mockLogger);
            var actual = await sessionManager.ProcessSessionMessageAsync(mockSession, dummyMessage);

            // Assert
            actual.Should().BeEquivalentTo(expectedState);
            actual.PostponedMessages.Should().Contain(dummyMessage.MessageId);
            Mock.Get(mockSession).Verify();
            Mock.Get(mockStateManager).Verify();
            Mock.Get(mockContractService)
                .Verify(c => c.ProcessMessage(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task ProcessSessionMessageAsync_SessionStateIsFaultedAndIsFailedMessage_ResetsSessionStateTest()
        {
            // Arrange
            var expectedBodyMessage = "test";
            var dummyMessage = GetDummyMessage(expectedBodyMessage);
            var initialState = new SessionWorkflowState
            {
                IsFaulted = true,
                FailedMessageId = dummyMessage.MessageId
            };

            var expectedState = new SessionWorkflowState();
            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            var mockLogger = Mock.Of<ILogger<IContractEventSessionManager>>(MockBehavior.Strict);
            var mockContractService = Mock.Of<IContractService>(MockBehavior.Strict);
            Mock.Get(mockContractService)
                .Setup(c => c.ProcessMessage(expectedBodyMessage))
                .ReturnsAsync(expectedBodyMessage)
                .Verifiable();

            var mockStateManager = Mock.Of<IWorkflowStateManager>(MockBehavior.Strict);
            Mock.Get(mockStateManager)
                .Setup(s => s.GetWorkflowStateAsync(mockSession))
                .ReturnsAsync(initialState)
                .Verifiable();

            Mock.Get(mockStateManager)
                .Setup(s => s.ResetWorkflowStateAsync(mockSession))
                .ReturnsAsync(expectedState)
                .Verifiable();

            // Act
            var sessionManager = new ContractEventSessionManager(mockStateManager, mockContractService, mockLogger);
            var actual = await sessionManager.ProcessSessionMessageAsync(mockSession, dummyMessage);

            // Assert
            actual.Should().Be(expectedState);
            Mock.Get(mockSession).VerifyNoOtherCalls();
            Mock.Get(mockStateManager).Verify();
            Mock.Get(mockContractService).Verify();
        }

        [TestMethod]
        public void ProcessSessionMessageAsync_DoesNotSetSessionStateAndThrowsException()
        {
            // Arrange
            var expectedBodyMessage = "test";
            var dummyMessage = GetDummyMessage(expectedBodyMessage);
            var initialState = new SessionWorkflowState();
            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            var mockLogger = Mock.Of<ILogger<IContractEventSessionManager>>(MockBehavior.Strict);
            var mockContractService = Mock.Of<IContractService>(MockBehavior.Strict);
            Mock.Get(mockContractService)
                .Setup(c => c.ProcessMessage(expectedBodyMessage))
                .Throws<Exception>();

            var mockStateManager = Mock.Of<IWorkflowStateManager>(MockBehavior.Strict);
            Mock.Get(mockStateManager)
                .Setup(s => s.GetWorkflowStateAsync(mockSession))
                .ReturnsAsync(initialState)
                .Verifiable();

            // Act
            var sessionManager = new ContractEventSessionManager(mockStateManager, mockContractService, mockLogger);
            Func<Task<SessionWorkflowState>> act = async () => await sessionManager.ProcessSessionMessageAsync(mockSession, dummyMessage);

            // Assert
            act.Should().Throw<Exception>();
            Mock.Get(mockSession).VerifyNoOtherCalls();
            Mock.Get(mockStateManager).Verify();
            Mock.Get(mockContractService).Verify();
        }

        [TestMethod]
        public void ProcessSessionMessageAsync_SetsSessionStateAndThrowsException()
        {
            // Arrange
            var expectedBodyMessage = "test";
            var dummyMessage = GetDummyMessage(expectedBodyMessage, 10);
            var initialState = new SessionWorkflowState();
            var expectedState = new SessionWorkflowState
            {
                IsFaulted = true,
                FailedMessageId = dummyMessage.MessageId
            };

            var mockSession = Mock.Of<IMessageSession>(MockBehavior.Strict);
            Mock.Get(mockSession)
                .SetupGet(s => s.SessionId)
                .Returns("session-id")
                .Verifiable();

            var mockLogger = Mock.Of<ILogger<IContractEventSessionManager>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var mockContractService = Mock.Of<IContractService>(MockBehavior.Strict);
            Mock.Get(mockContractService)
                .Setup(c => c.ProcessMessage(expectedBodyMessage))
                .Throws<Exception>();

            var mockStateManager = Mock.Of<IWorkflowStateManager>(MockBehavior.Strict);
            Mock.Get(mockStateManager)
                .Setup(s => s.GetWorkflowStateAsync(mockSession))
                .ReturnsAsync(initialState)
                .Verifiable();

            Mock.Get(mockStateManager)
                .Setup(s => s.SetWorkflowStateAsync(mockSession, It.Is<SessionWorkflowState>(s => s.IsFaulted && s.FailedMessageId == dummyMessage.MessageId)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var sessionManager = new ContractEventSessionManager(mockStateManager, mockContractService, mockLogger);
            Func<Task<SessionWorkflowState>> act = async () => await sessionManager.ProcessSessionMessageAsync(mockSession, dummyMessage);

            // Assert
            act.Should().Throw<Exception>();

            Mock.Get(mockSession).Verify();
            Mock.Get(mockStateManager).Verify();
            Mock.Get(mockContractService).Verify();
            Mock.Get(mockLogger).Verify();
        }

        private static Message GetDummyMessage(string expectedBodyMessage, int deliveryCount = 1)
        {
            Message dummyMessage = new Message
            {
                MessageId = "1",
                Body = Encoding.UTF8.GetBytes(expectedBodyMessage)
            };

            //Workaround to set internal get properties
            var systemProperties = new Message.SystemPropertiesCollection();
            typeof(Message.SystemPropertiesCollection).GetProperty(nameof(systemProperties.SequenceNumber)).SetValue(systemProperties, 1);
            typeof(Message.SystemPropertiesCollection).GetProperty(nameof(systemProperties.DeliveryCount)).SetValue(systemProperties, deliveryCount);
            typeof(Message.SystemPropertiesCollection).GetProperty(nameof(systemProperties.EnqueuedTimeUtc)).SetValue(systemProperties, DateTime.Now);
            typeof(Message.SystemPropertiesCollection).GetProperty(nameof(systemProperties.LockedUntilUtc)).SetValue(systemProperties, DateTime.Now);
            typeof(Message).GetProperty(nameof(dummyMessage.SystemProperties)).SetValue(dummyMessage, systemProperties);
            return dummyMessage;
        }
    }
}
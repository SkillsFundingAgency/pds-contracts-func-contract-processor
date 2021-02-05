using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// Contract events session message manager.
    /// </summary>
    /// <seealso cref="Pds.Contracts.ContractEventProcessor.Services.Interfaces.IContractEventSessionManager" />
    public class ContractEventSessionManager : IContractEventSessionManager
    {
        private readonly IWorkflowStateManager _stateManager;
        private readonly IContractService _contractService;
        private readonly ILogger<IContractEventSessionManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventSessionManager" /> class.
        /// </summary>
        /// <param name="stateManager">The session workflow state manager.</param>
        /// <param name="contractService">The contract service.</param>
        /// <param name="logger">The logger.</param>
        public ContractEventSessionManager(IWorkflowStateManager stateManager, IContractService contractService, ILogger<IContractEventSessionManager> logger)
        {
            _stateManager = stateManager;
            _contractService = contractService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SessionWorkflowState> ProcessSessionMessageAsync(IMessageSession session, Message message)
        {
            var state = await _stateManager.GetWorkflowStateAsync(session);
            if (state.IsFaulted && state.FailedMessageId != message.MessageId)
            {
                state.PostponedMessages.Add(message.MessageId);
                await session.DeadLetterAsync(message.SystemProperties.LockToken, $"Previous message failed, hence dropping all messages in session", $"Previous message {state.FailedMessageId} failed, hence dropping all messages in session {session.SessionId}");
                await _stateManager.SetWorkflowStateAsync(session, state);
            }
            else
            {
                if (state.IsFaulted && state.FailedMessageId == message.MessageId)
                {
                    state = await _stateManager.ResetWorkflowStateAsync(session);
                }

                try
                {
                    var contractEvent = Encoding.UTF8.GetString(message.Body);
                    await _contractService.ProcessMessage(contractEvent);
                }
                catch
                {
                    if (message.SystemProperties.DeliveryCount > 9)
                    {
                        state.FailedMessageId = message.MessageId;
                        state.IsFaulted = true;
                        await _stateManager.SetWorkflowStateAsync(session, state);
                        _logger.LogInformation($"Message delivery count exceeded saving workflow state as faulted with failed message : {message.MessageId} for session: {session.SessionId}.");
                    }

                    throw;
                }
            }

            return state;
        }
    }
}
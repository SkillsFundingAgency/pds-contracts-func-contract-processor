using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Func
{
    /// <summary>
    /// Example ServiceBus queue triggered Azure Function.
    /// </summary>
    public class ContractEventProcessorFunction
    {
        private readonly IContractEventSessionManager _contractEventSessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventProcessorFunction" /> class.
        /// </summary>
        /// <param name="contractEventSessionManager">The contract event session manager.</param>
        public ContractEventProcessorFunction(IContractEventSessionManager contractEventSessionManager)
        {
            _contractEventSessionManager = contractEventSessionManager;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="contractEvent">The queue item that triggered this function to run.</param>
        /// <param name="messageSession">The message session.</param>
        /// <param name="log">The logger.</param>
        /// <returns>
        /// Async Task.
        /// </returns>
        [FunctionName(nameof(ContractEventProcessorFunction))]
        public async Task Run(
            [ServiceBusTrigger("%ContractEventsSessionQueue%", Connection = "ServiceBusConnection", IsSessionsEnabled = true)] Message contractEvent,
            IMessageSession messageSession,
            ILogger log)
        {
            var runId = Guid.NewGuid();
            using (log?.BeginScope($"[{nameof(ContractEventProcessorFunction)}] - RunId: [{runId}], message: [{contractEvent.MessageId}],  and session: [{messageSession.SessionId}] attempt: [{contractEvent.SystemProperties.DeliveryCount}]"))
            {
                log?.LogInformation($"Start: Processing message: {contractEvent.MessageId} from session: {messageSession.SessionId}.");

                contractEvent.UserProperties.Add("ContractEventProcessorRunId", runId);
                var state = await _contractEventSessionManager.ProcessSessionMessageAsync(messageSession, contractEvent);

                if (state.IsFaulted)
                {
                    log?.LogWarning($"[{nameof(ContractEventProcessorFunction)}] - End: Message: {contractEvent.MessageId} moved to DLQ from session: {messageSession.SessionId}.");
                }
                else
                {
                    log?.LogInformation($"[{nameof(ContractEventProcessorFunction)}] - End: Successfully processed message: {contractEvent.MessageId} from session: {messageSession.SessionId}.");
                }
            }
        }
    }
}
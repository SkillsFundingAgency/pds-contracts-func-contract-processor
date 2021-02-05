using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// Contract service.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly ILogger<IContractService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ContractService(ILogger<IContractService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> ProcessMessage(string contractEvent)
        {
            _logger.LogInformation($"Processing message {contractEvent}");

            if (contractEvent.ToLower().Contains("throw-exception") && contractEvent.Contains("\"ExampleSequenceId\":0"))
            {
                throw new System.Exception("Testing faulty session.");
            }

            return await Task.FromResult(contractEvent);
        }
    }
}
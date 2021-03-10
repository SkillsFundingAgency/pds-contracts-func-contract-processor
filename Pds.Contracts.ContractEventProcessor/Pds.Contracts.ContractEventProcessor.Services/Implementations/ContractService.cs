using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// Contract service.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly ILogger<IContractService> _logger;
        private readonly IContractsDataService _contractsDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="contractsDataService">The contracts data service.</param>
        public ContractService(ILogger<IContractService> logger, IContractsDataService contractsDataService)
        {
            _logger = logger;
            _contractsDataService = contractsDataService;
        }

        /// <inheritdoc/>
        public async Task ProcessMessage(ContractEvent contractEvent)
        {
            _logger.LogInformation($"Processing message for contract event : {contractEvent.BookmarkId}");

            // Added sample implementation of contracts data api - remove this comment during full implementation.
            var existingContract = await _contractsDataService.GetContractByContractNumberAndVersionAsync(contractEvent.ContractNumber, contractEvent.ContractVersion);
            if (existingContract is null)
            {
                throw new ArgumentOutOfRangeException(nameof(contractEvent));
            }

            _logger.LogInformation($"Found contract with contract number [{existingContract.ContractNumber}], version [{existingContract.ContractVersion}] and Id [{existingContract.Id}]");
        }
    }
}
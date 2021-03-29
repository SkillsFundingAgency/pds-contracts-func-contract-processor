using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
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
        private readonly IContractApprovalService _contractApprovalService;
        private readonly IValidationService _validationService;
        private readonly IContractCreationService _contractCreationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="contractsDataService">The contracts data service.</param>
        /// <param name="contractApprovalService">The contract approval service.</param>
        /// <param name="validationService">The validation service.</param>
        /// <param name="contractCreationService">The contract creation service.</param>
        public ContractService(
            ILogger<IContractService> logger,
            IContractsDataService contractsDataService,
            IContractApprovalService contractApprovalService,
            IValidationService validationService,
            IContractCreationService contractCreationService)
        {
            _logger = logger;
            _contractsDataService = contractsDataService;
            _contractApprovalService = contractApprovalService;
            _validationService = validationService;
            _contractCreationService = contractCreationService;
        }

        /// <inheritdoc/>
        public async Task ProcessMessage(ContractEvent contractEvent)
        {
            _logger.LogInformation($"[{nameof(ProcessMessage)}] Processing message for contract event : {contractEvent.BookmarkId}");

            var eventType = _validationService.GetContractEventType(contractEvent);

            var contract = await _contractsDataService.TryGetContractAsync(contractEvent.ContractNumber, contractEvent.ContractVersion);

            switch (eventType)
            {
                case ContractEventType.Creation:
                    if (contract != null)
                    {
                        _logger.LogWarning($"Contract with contract number [{contract.ContractNumber}], version [{contract.ContractVersion}] and Id [{contract.Id}] already exists.");
                    }
                    else
                    {
                        await _contractCreationService.CreateAsync(contractEvent);
                    }

                    break;
                case ContractEventType.Approval:
                    if (contract is null)
                    {
                        _logger.LogWarning($"Unable to find contract with contract number [{contract.ContractNumber}], version [{contract.ContractVersion}] and Id [{contract.Id}]");
                    }
                    else
                    {
                        await _contractApprovalService.Approve(contractEvent);
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
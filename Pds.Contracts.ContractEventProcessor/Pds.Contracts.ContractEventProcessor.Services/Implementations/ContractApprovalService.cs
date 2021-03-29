using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Common;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
using System;
using System.Threading.Tasks;

#pragma warning disable S1135 // Track uses of "TODO" tags

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractApprovalService : IContractApprovalService
    {
        private readonly ILogger<IContractService> _logger;

        private readonly IContractsDataService _contractsDataService;

        private readonly IValidationService _validationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractApprovalService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="contractsDataService">The contracts data service.</param>
        /// <param name="validationService">The validation service.</param>
        public ContractApprovalService(ILogger<IContractService> logger, IContractsDataService contractsDataService, IValidationService validationService)
        {
            _logger = logger;
            _contractsDataService = contractsDataService;
            _validationService = validationService;
        }

        /// <inheritdoc/>
        public Task Approve(ContractEvent contractEvent)
        {
            throw new NotImplementedException();
        }
    }
}
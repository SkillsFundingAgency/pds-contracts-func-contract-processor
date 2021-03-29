using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Configurations;
using Pds.Contracts.ContractEventProcessor.Services.DocumentServices;
using Pds.Contracts.ContractEventProcessor.Services.Extensions;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Interfaces;
using Pds.Contracts.Data.Api.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// The contract creation service.
    /// </summary>
    public class ContractCreationService : IContractCreationService
    {
        private readonly ILogger<IContractCreationService> _logger;
        private readonly IContractsDataService _contractsDataService;
        private readonly ISharePointClientService _sharePointClientService;
        private readonly IDocumentManagementService _documentManagementService;
        private readonly IContractProcessorService _contractProcessorService;
        private readonly SPClientServiceConfiguration _spConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractCreationService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="contractsDataService">The contracts data service.</param>
        /// <param name="sharePointClientService">The share point client service.</param>
        /// <param name="documentManagementService">The Aspose pdf document management service.</param>
        /// <param name="contractProcessorService">The contract processor service.</param>
        /// <param name="spClientServiceConfiguration">The SharePoint Client Service configuration.</param>
        public ContractCreationService(ILogger<IContractCreationService> logger, IContractsDataService contractsDataService, ISharePointClientService sharePointClientService, IDocumentManagementService documentManagementService, IContractProcessorService contractProcessorService, IOptions<SPClientServiceConfiguration> spClientServiceConfiguration)
        {
            _logger = logger;
            _contractsDataService = contractsDataService;
            _sharePointClientService = sharePointClientService;
            _documentManagementService = documentManagementService;
            _contractProcessorService = contractProcessorService;
            _spConfig = spClientServiceConfiguration.Value;
        }

        /// <summary>
        /// It will create a new contract.
        /// </summary>
        /// <param name="contractEvent">New contract Data.</param>
        /// <returns>Returns true when a new contract created else false.</returns>
        public async Task<bool> CreateAsync(ContractEvent contractEvent)
        {
            _logger.LogInformation($"[{nameof(CreateAsync)}] - Processing message for contract creation. ContractNumber: {contractEvent.ContractNumber}, ContractVersion: {contractEvent.ContractVersion}");

            if (contractEvent.ContractAllocations == null || contractEvent.ContractAllocations.Count() <= 0)
            {
                _logger.LogError($"[{nameof(CreateAsync)}] - Contract allocation not found. ContractNumber: {contractEvent.ContractNumber}, ContractVersion: {contractEvent.ContractVersion}");
                throw new ContractEventExpectationFailedException(contractEvent.ContractNumber, contractEvent.ContractVersion, "Contract allocation not found.");
            }

            var createRequest = _contractProcessorService.GetCreateRequest(contractEvent);
            var fileName = _contractProcessorService.GetFileNameForContractDocument(contractEvent.UKPRN, contractEvent.ContractNumber, contractEvent.ContractVersion);
            var folderName = _contractProcessorService.GetFolderNameForContractDocument(contractEvent.FundingType.ToString("G"), contractEvent.ContractPeriodValue, _spConfig.PublicationFolderSuffix);
            var urlSafeFolderName = _contractProcessorService.GetUrlSafeFolderNameForContractDocument(folderName);
            var pdfDoc = await _sharePointClientService.GetDocument(fileName, urlSafeFolderName);
            var pdfADoc = _documentManagementService.ConvertToPdfA(pdfDoc);
            createRequest.ContractContent = _contractProcessorService.GetContractContent(pdfADoc, fileName);
            await _contractsDataService.CreateContractAsync(createRequest);
            return true;
        }
    }
}

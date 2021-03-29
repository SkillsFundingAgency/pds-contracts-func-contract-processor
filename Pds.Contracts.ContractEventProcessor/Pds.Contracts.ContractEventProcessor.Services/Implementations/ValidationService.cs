using Microsoft.Extensions.Logging;
using Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers;
using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Models;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <summary>
    /// Contract validation service.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly ILogger<IValidationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ValidationService(ILogger<IValidationService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public ContractEventType GetContractEventType(ContractEvent contractEvent)
        {
            _logger.LogInformation($"[{nameof(GetContractEventType)}] Start processing contract event type for contract number: {contractEvent.ContractNumber}, contract version: {contractEvent.ContractVersion}, parent status: {contractEvent.ParentStatus}, status: {contractEvent.Status} and ammendment type: {contractEvent.AmendmentType}.");
            var eventType = (contractEvent.ParentStatus, contractEvent.Status, contractEvent.AmendmentType) switch
            {
                (ContractParentStatus.Draft, ContractStatus.PublishedToProvider, ContractAmendmentType.None) => ContractEventType.Creation,
                (ContractParentStatus.Draft, ContractStatus.PublishedToProvider, ContractAmendmentType.Variation) => ContractEventType.Creation,
                (ContractParentStatus.Approved, ContractStatus.Approved, ContractAmendmentType.Notfication) => ContractEventType.Creation,
                (ContractParentStatus.Approved, ContractStatus.Modified, ContractAmendmentType.Notfication) => ContractEventType.Creation,
                (ContractParentStatus.Approved, ContractStatus.UnderTermination, ContractAmendmentType.Notfication) => ContractEventType.Creation,

                (ContractParentStatus.Approved, ContractStatus.Approved, ContractAmendmentType.None) => ContractEventType.Approval,
                (ContractParentStatus.Approved, ContractStatus.Approved, ContractAmendmentType.Variation) => ContractEventType.Approval,
                (ContractParentStatus.Approved, ContractStatus.Modified, ContractAmendmentType.None) => ContractEventType.Approval,
                (ContractParentStatus.Approved, ContractStatus.Modified, ContractAmendmentType.Variation) => ContractEventType.Approval,
                (ContractParentStatus.Approved, ContractStatus.UnderTermination, ContractAmendmentType.None) => ContractEventType.Approval,
                (ContractParentStatus.Approved, ContractStatus.UnderTermination, ContractAmendmentType.Variation) => ContractEventType.Approval,

                _ => throw new ContractExpectationFailedException(contractEvent.ContractNumber, contractEvent.ContractVersion, "Contract Event Type"),
            };
            _logger.LogInformation($"[{nameof(GetContractEventType)}] - Contract messsage event for {eventType} - ContractNumber: {contractEvent.ContractNumber}, version: {contractEvent.ContractVersion}, event status: {contractEvent.Status}, event amendment type: {contractEvent.AmendmentType}.");
            return eventType;
        }
    }
}

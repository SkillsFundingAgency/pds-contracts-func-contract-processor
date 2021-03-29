using System;

namespace Pds.Contracts.ContractEventProcessor.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract event expectation failed exception.
    /// </summary>
    public class ContractEventExpectationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventExpectationFailedException"/> class.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="versionNumber">Contract version number.</param>
        /// <param name="contractId">Contract internal identifier.</param>
        /// <param name="failedExpectation">Failed validation expression.</param>
        public ContractEventExpectationFailedException(string contractNumber, int versionNumber, string failedExpectation)
            : base($"Expectation: {failedExpectation} failed in contract event with contract number:{contractNumber}, version: {versionNumber}.")
        {
            ContractNumber = contractNumber;
            VersionNumber = versionNumber;
            FailedExpectation = failedExpectation;
        }

        /// <summary>
        /// Gets ContractNumber.
        /// </summary>
        public string ContractNumber { get; }

        /// <summary>
        /// Gets VersionNumber.
        /// </summary>
        public int VersionNumber { get; }

        /// <summary>
        /// Gets Predicate.
        /// </summary>
        public string FailedExpectation { get; }
    }
}

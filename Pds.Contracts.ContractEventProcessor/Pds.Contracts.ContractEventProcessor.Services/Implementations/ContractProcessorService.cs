using Pds.Contracts.ContractEventProcessor.Common.Enums;
using Pds.Contracts.ContractEventProcessor.Services.Extensions;
using Pds.Contracts.ContractEventProcessor.Services.Interfaces;
using Pds.Contracts.ContractEventProcessor.Services.Models;
using Pds.Contracts.Data.Api.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pds.Contracts.ContractEventProcessor.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractProcessorService : IContractProcessorService
    {
        /// <summary>
        /// Organization Name Abbreviated.
        /// </summary>
        private const string OrganizationNameAbbreviated = "ESFA";

        /// <summary>
        /// CreatedBy.
        /// </summary>
        private const string CreatedBy = "Feed-ContractEventProcessor";

        /// <inheritdoc/>
        public CreateRequest GetCreateRequest(ContractEvent contractEvent)
        {
            string contractTitle = CreateContractTitle(contractEvent);

            var createRequest = new CreateRequest();
            createRequest.AmendmentType = (Data.Api.Client.Enumerations.ContractAmendmentType)contractEvent.AmendmentType;
            createRequest.ContractFundingStreamPeriodCodes = GetContractFundingStreamPeriodCodes(contractEvent.ContractAllocations);
            createRequest.ContractNumber = contractEvent.ContractNumber;
            createRequest.Year = FormatPeriod(contractEvent.ContractPeriodValue, contractEvent.StartDate, contractEvent.EndDate);
            createRequest.Type = (Data.Api.Client.Enumerations.ContractType)Enum.Parse(typeof(ContractType), contractEvent.ContractType);
            createRequest.ContractVersion = contractEvent.ContractVersion;
            createRequest.EndDate = contractEvent.EndDate;
            createRequest.FundingType = (Data.Api.Client.Enumerations.ContractFundingType)contractEvent.FundingType;
            createRequest.ParentContractNumber = contractEvent.ParentContractNumber;
            createRequest.StartDate = contractEvent.StartDate;
            createRequest.UKPRN = contractEvent.UKPRN;
            createRequest.Value = contractEvent.Value;
            createRequest.ContractData = contractEvent.ContractEventXml;
            createRequest.Title = contractTitle;
            createRequest.ContractAllocationNumber = contractEvent.ContractAllocations.First().ContractAllocationNumber;
            createRequest.CreatedBy = CreatedBy;
            createRequest.PageCount = 0;
            createRequest.SignedOn = contractEvent.SignedOn;
            return createRequest;
        }

        /// <inheritdoc/>
        public string CreateContractTitle(ContractEvent contractEvent)
        {
            var fundingType = contractEvent.FundingType;

            var fundingTypeName = fundingType.GetEnumDisplayName();

            var lepArea = string.Empty;
            var specification = string.Empty;

            var contractAllocation = contractEvent.ContractAllocations?.FirstOrDefault();
            if (fundingType == ContractFundingType.Esf && contractAllocation != null)
            {
                lepArea = contractAllocation.LEPArea;
                specification = contractAllocation.TenderSpecTitle;
            }

            var variation = string.Empty;
            if (contractEvent.ContractVersion > 1)
            {
                variation = "variation ";
            }

            var contractPeriod = FormatPeriod(contractEvent.ContractPeriodValue, contractEvent.StartDate, contractEvent.EndDate);

            switch (fundingType)
            {
                case ContractFundingType.Mainstream:
                    if (GetStartingYear(contractEvent.ContractPeriodValue) >= 17)
                    {
                        return
                            $"Skills and adult education contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";
                    }

                    // All mainstream contracts are effectively variations so they always display variation in the friendly name.
                    return $"{fundingTypeName} contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.Age:
                    return $"{fundingTypeName} Grant contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.TwentyFourPlusLoan:
                    return $"Advanced Learner Loans contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.Eop:
                case ContractFundingType.Eof:
                    return $"{GetFriendlyYear(contractEvent.ContractPeriodValue)} {fundingTypeName} contract variation version {contractEvent.ContractVersion}";

                case ContractFundingType.Esf:
                    return $"{fundingTypeName} {specification} contract {variation}for {lepArea} version {contractEvent.ContractVersion}";

                case ContractFundingType.Levy:
                    return $"{OrganizationNameAbbreviated} {fundingTypeName} {DateTime.Now.ToFullMonthAndFullYearDisplay()} version {contractEvent.ContractVersion}";

                case ContractFundingType.Ncs:
                    return $"{fundingTypeName} contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.NonLevy:
                    return $"{OrganizationNameAbbreviated} apprenticeship contract for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.SixteenNineteenFunding:
                    return $"{fundingTypeName} contract for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.Aebp:
                    return $"{fundingTypeName} contract for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.Nla:
                    return $"{fundingTypeName} version {contractEvent.ContractVersion}";

                case ContractFundingType.AdvancedLearnerLoans:
                    return $"Advanced Learner Loans contract {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.ProcuredNineteenToTwentyFourTraineeship:
                    return $"{fundingTypeName} contract for {contractPeriod} version {contractEvent.ContractVersion}";

                case ContractFundingType.EducationAndSkillsFunding:
                case ContractFundingType.NonLearningGrant:
                case ContractFundingType.SixteenEighteenForensicUnit:
                case ContractFundingType.DanceAndDramaAwards:
                case ContractFundingType.CollegeCollaborationFund:
                case ContractFundingType.FurtherEducationConditionAllocation:
                    return $"{fundingTypeName} {variation}for {contractPeriod} version {contractEvent.ContractVersion}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(fundingType));
            }
        }

        /// <inheritdoc/>
        public string FormatPeriod(string periodValue, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (periodValue.Length != 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            var startsWith20 = periodValue.Substring(0, 2) == "20";

            if (startsWith20)
            {
                var startYear = startDate.HasValue ? startDate.Value.Year : (int?)null;
                var endYear = endDate.HasValue ? endDate.Value.Year : (int?)null;

                if (startYear == endYear)
                {
                    return periodValue;
                }
            }

            return "20" + periodValue.Substring(0, 2) + " to 20" + periodValue.Substring(2, 2);
        }

        /// <inheritdoc/>
        public CreateContractCode[] GetContractFundingStreamPeriodCodes(IEnumerable<ContractAllocation> contractAllocations)
        {
            return contractAllocations
                    .Select(a => a.FundingStreamPeriodCode)
                    .Distinct()
                    .Select(c => new CreateContractCode { Code = c })
                    .ToArray();
        }

        /// <inheritdoc/>
        public int GetStartingYear(string periodValue)
        {
            if (periodValue.Length != 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return int.Parse(periodValue.Substring(0, 2));
        }

        /// <inheritdoc/>
        public string GetFriendlyYear(string year)
        {
            if (year.Length < 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (year.Length == 4)
            {
                return "20" + year.Substring(0, 2) + "/" + year.Substring(2, 2);
            }

            if (year.Substring(0, 2) == "20")
            {
                return year;
            }

            throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc/>
        public CreateContractRequestDocument GetContractContent(byte[] pdfADoc, string fileName)
        {
            var contractContent = new CreateContractRequestDocument();
            contractContent.FileName = fileName;
            contractContent.Size = pdfADoc.Length;
            contractContent.Content = pdfADoc;
            return contractContent;
        }

        /// <inheritdoc/>
        public string GetFileNameForContractDocument(int? ukprn, string contractNumber, int version)
        {
            return $"{ukprn}_{contractNumber}_v{version}.pdf";
        }

        /// <inheritdoc/>
        public string GetFolderNameForContractDocument(string fundingTypeShortName, string period, string suffix)
        {
            return $"{fundingTypeShortName}-{period}-{suffix}";
        }

        /// <inheritdoc/>
        public string GetUrlSafeFolderNameForContractDocument(string folderName)
        {
            Regex pattern = new Regex("[-,+]");
            folderName = pattern.Replace(folderName, string.Empty);
            return System.Web.HttpUtility.UrlPathEncode(folderName);
        }
    }
}

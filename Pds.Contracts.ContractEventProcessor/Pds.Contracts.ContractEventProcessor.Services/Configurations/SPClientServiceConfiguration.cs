using Pds.Core.ApiClient;

namespace Pds.Contracts.ContractEventProcessor.Services.Configurations
{
    /// <summary>
    /// Structure to hold configuration for the SharePoint Rest API.
    /// </summary>
    public class SPClientServiceConfiguration : BaseApiClientConfiguration
    {
        /// <summary>
        /// Gets or Sets Resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or Sets the relative sharepoint site URL.
        /// </summary>
        public string RelativeSiteURL { get; set; }

        /// <summary>
        /// Gets or Sets the Publication Folder Suffix.
        /// </summary>
        public string PublicationFolderSuffix { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether should throw error if the contract Pdf Not Found in the sharepoint.
        /// </summary>
        public bool ShouldErrorPdfNotFound { get; set; } = true;
    }
}
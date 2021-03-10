namespace Pds.Contracts.ContractEventProcessor.Services.Models
{
    /// <summary>
    /// Contract event from atom feed.
    /// </summary>
    public class ContractEvent
    {
        /// <summary>
        /// Gets or sets contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets contract version.
        /// </summary>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets bookmark id.
        /// </summary>
        public string BookmarkId { get; set; }
    }
}
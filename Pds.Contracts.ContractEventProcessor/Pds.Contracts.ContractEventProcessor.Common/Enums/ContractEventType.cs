namespace Pds.Contracts.ContractEventProcessor.Common.Enums
{
    /// <summary>
    /// Contract Event Type.
    /// </summary>
    public enum ContractEventType
    {
        /// <summary>
        /// Contract event type has not yet been defined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Contract creation event.
        /// </summary>
        Creation,

        /// <summary>
        /// Contract approval event.
        /// </summary>
        Approval,

        /// <summary>
        /// Contract manual approval event.
        /// </summary>
        ManualApproval
    }
}

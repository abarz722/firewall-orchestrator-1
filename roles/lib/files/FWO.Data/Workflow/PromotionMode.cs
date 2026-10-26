namespace FWO.Data.Workflow
{
    /// <summary>
    /// Defines which workflow object can be promoted explicitly in a phase.
    /// </summary>
    public enum PromotionMode
    {
        Ticket = 0,
        Task = 1,
        Both = 2
    }
}

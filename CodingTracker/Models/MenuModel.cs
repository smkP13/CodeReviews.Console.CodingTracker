namespace CodingTracker.Models
{
    internal class MenuModel
    {
        internal string? Project { get; set; }

        internal bool IsCodingSessionRunning {  get; set; }
        internal string? CurrentCodingSession { get; set; }
        internal string? SqlCommandText { get; set; }
        internal CodingSession? CurrentData { get; set; }
    }
}
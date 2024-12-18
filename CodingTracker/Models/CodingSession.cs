namespace CodingTracker.Models
{
    internal class CodingSession
    {
        internal string? Project { get; set; }
        internal string? StartDate { get; set; }
        internal string? EndDate { get; set; }
        internal string? StartTime { get; set; }
        internal string? EndTime { get; set; }
        internal string? Duration { get; set; }
        // Duration of a single session
        internal int rowid { get; set; }
        internal int Id { get; set; }
        internal string? DurationCount { get; set; }
        // DurationCount use the 'Duration' column to count the total number of sessions
        internal string? TotalDuration { get; set; }
        // Addition of all session durations to give the total time working on a project
        internal string? Average { get; set; } 
    }
}
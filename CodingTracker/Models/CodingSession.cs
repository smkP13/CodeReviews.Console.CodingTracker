namespace CodingTracker.Models
{
    internal class CodingSession
    {
        internal string? Project { get; set; }
        internal DateTime Start {  get; set; }
        internal DateTime End { get; set; }
        internal TimeSpan Duration { get; set; }
        // Duration of a single session
        internal int Id { get; set; }
        internal int DurationCount { get; set; }
        // DurationCount use the 'Duration' column to count the total number of sessions
        internal string? TotalDuration { get; set; }
        // Addition of all session durations to give the total time working on a project
        internal string? Average { get; set; } 
    }
}
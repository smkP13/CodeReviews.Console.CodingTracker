using CodingTracker.Controllers;
using CodingTracker.Models;
using Spectre.Console;

namespace CodingTracker.Views
{
    internal class DataOutput
    {
        public static void PrintProjectData(DataTools dataTools, string Project = "test_project", string ascDesc = "Asc", string option = "All data") 
        {
            Table table = new();
            table.Centered();
            table.Title(Project);

            List<CodingSession> projectData;

            if (option == "All data")
            {
                table.AddColumns("Id", "Start Date", "Start Time", "End Date", "End Time", "Duration");
                projectData = dataTools.GetProjectData(Project, ascDesc);
                TimeSpan totalDuration = new();
                if (projectData != null)
                {
                    foreach (CodingSession data in projectData)
                    {
                        totalDuration.Add(data.Duration);
                        table.AddRow(data.Id.ToString(), data.Start.ToString("yyyy.MM.dd"), data.Start.ToString("HH:mm:ss"), data.End.ToString("yyyy.MM.dd"), data.End.ToString("HH:mm:ss"), data.Duration.ToString());
                    }
                    AnsiConsole.Write(table);
                    table = new();
                    table.Centered();
                    table.AddColumns("First Session", "Last Session", "Total session", "Total Duration");

                    Console.WriteLine();
                }
                else
                {
                    table = new();
                    table.Centered();
                    table.AddColumn($"{Project} data is empty");
                    AnsiConsole.Write(table);
                    Console.WriteLine();
                }
            }
            else
            {
                projectData = dataTools.GetProjectData(Project, ascDesc, option);
                if (projectData != null)
                {
                    switch (option)
                    {
                        case "Weekly":
                            table.AddColumns("Year", "Month (n°)", "Week Start", "Week end", "Number of session", "Total duration");
                            foreach (CodingSession data in projectData)
                            {
                                if (data.Project == "Final Result")
                                {
                                    AnsiConsole.Write(table);
                                    table = new();
                                    table.Centered();
                                    table.AddColumns("First session", "Last session", "Total session n°", "Final duration", "Average Duration");
                                    table.AddRow(data.End.Year.ToString(), data.Start.Year.ToString(), data.DurationCount.ToString(), data.TotalDuration, data.Average);
                                    AnsiConsole.Write(table);
                                }
                                else
                                {
                                    table.AddRow(data.Start.Year.ToString(), data.Start.Month.ToString(),
                                    data.Start.ToString("yyyy.MM.dd"), data.End.ToString("yyyy.MM.dd"), data.DurationCount.ToString(), data.TotalDuration);
                                }
                            }
                            Console.WriteLine();

                            break;

                        case "Monthly":
                            table.AddColumns("Year", "Month (n°)", "Number of session", "Total duration");
                            foreach (CodingSession data in projectData)
                            {
                                if (data.Project == "Final Result")
                                {
                                    AnsiConsole.Write(table);
                                    table = new();
                                    table.Centered();
                                    table.AddColumns("First session", "Last session", "Total session n°", "Final duration", "Average Duration");
                                    table.AddRow(data.End.ToString("yyyy.MM.dd"), data.Start.ToString("yyyy.MM.dd"), data.DurationCount.ToString(), data.TotalDuration, data.Average);
                                    AnsiConsole.Write(table);
                                }
                                else
                                {
                                    table.AddRow(data.Start.Year.ToString(), data.Start.Month.ToString(), data.DurationCount.ToString(), data.TotalDuration);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case "Yearly":
                            table.AddColumns("Year", "Number of session", "Total duration");
                            foreach (CodingSession data in projectData)
                            {
                                if (data.Project == "Final Result")
                                {
                                    AnsiConsole.Write(table);
                                    table = new();
                                    table.Centered();
                                    table.AddColumns("First session", "Last session", "Total session n°", "Final duration", "Average Duration");
                                    table.AddRow(data.End.ToString("yyyy.MM.dd"), data.Start.ToString("yyyy.MM.dd"), data.DurationCount.ToString(), data.TotalDuration, data.Average);
                                    AnsiConsole.Write(table);
                                }
                                else
                                {
                                    table.AddRow(data.Start.Year.ToString(), data.DurationCount.ToString(), data.TotalDuration);
                                }
                            }
                            Console.WriteLine();
                            break;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"The project [red bold]{Project}[/] contains no data");
                }
            }
        }

        public static void ShowGoal(CodingSession session)
        {
            try
            {
                if (session != null)
                {
                    Table table = new Table();
                    table.Centered();
                    table.AddColumns("Project", "Start", "End", "Estimate time", "Estimate time per day");
                    table.AddRow(session.Project, session.Start.ToString("yyyy.MM.dd"), session.End.ToString("yyyy.MM.dd"), session.TotalDuration, session.Duration.ToString());
                    AnsiConsole.Write(table);
                }
                else
                {
                    AnsiConsole.WriteLine("Goals seems to not have been correctly set, please delete this Coding Goal project and create a new one.");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
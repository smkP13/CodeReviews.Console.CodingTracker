using CodingTracker.Models;
using Spectre.Console;
using System.Globalization;

namespace CodingTracker.Controllers
{
    internal class UserInputs
    {
        public static string GetStringInput(string message = "")
        {
            string? readResult;
            readResult = AnsiConsole.Prompt(
                new TextPrompt<string>(message)
                .Validate(result => result.IndexOfAny(new char[] { '/', '-', '\\', '\'', '"', '(', '[', '{', '?', '!', '&', '>', '<', '=', ',', '.', ' ' }) == -1)
                .ValidationErrorMessage("[red bold]Invalid input[/] format, please rewrite project name using in a [blue]valid[/] format"));
            return readResult;
        }

        public static string GetDateTimeInput(string message, string type = "Date")
        {
                bool validInput = false;
                string readResult;
                do
                {
                    if (type == "Date")
                    {
                        DateTime dateTime;
                        readResult = AnsiConsole.Prompt(
                            new TextPrompt<string>(message)
                            .Validate(result => result.IndexOfAny(new char[] { '/', '\\', '\'', '"', '(', '[', '{', '?', '!', '&', '>', '<', '=', ',', '_' }) == -1)
                            .Validate(result => DateTime.TryParseExact(result, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            .ValidationErrorMessage("[red bold]Invalid input[/] format, please rewrite date name using a [blue]valid[/] format: [green]yyyy.MM.dd[/]"));
                        return readResult;
                    }
                    else
                    {
                        DateTime dateTime;
                        readResult = AnsiConsole.Prompt(
                            new TextPrompt<string>(message)
                            .Validate(result => result.IndexOfAny(new char[] { '/', '\\', '\'', '"', '(', '[', '{', '?', '!', '&', '>', '<', '=', ',', '_' }) == -1)
                            .Validate(result => DateTime.TryParseExact(result, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            .ValidationErrorMessage("[red bold]Invalid input[/] format, please rewrite time using a [blue]valid[/] format: [green]HH:mm[/]"));
                        return readResult;
                    }
                }
                while (!validInput);
        }

        public static string GetDurationEstimation()
        {
            TimeSpan durationEstimation = new();
            string readResult;
            readResult = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter a time estimation: (d.HH:MM.ss)")
                    .Validate(result => result.IndexOfAny(new char[] { '/', '\\', '\'', '"', '(', '[', '{', '?', '!', '&', '>', '<', '=', ',' }) == -1)
                    .Validate(result => TimeSpan.TryParseExact(result, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None, out durationEstimation))
                    .ValidationErrorMessage("[red bold]Invalid input[/] format, please rewrite estimation time using in a [blue]valid[/] format: [green]d.HH:MM:ss[/]"));
            return durationEstimation.ToString();
        }

        public static string SelectExistingProject(DataTools dataTools, bool newProject = false, bool Goal = false)
        {
            SelectionPrompt<string> prompt = new();
            List<string> projects = dataTools.GetTables();
            prompt.AddChoice("Cancel");
            if (newProject) prompt.AddChoice("Add new Project");

            if (projects != null)
            {
                if (Goal)
                {
                    foreach (string project in projects)
                    {
                        if (project.IndexOf("_CGoal") != -1) prompt.AddChoice(project);
                    }
                }
                else
                {
                    foreach (string project in projects)
                    {
                        if (project.IndexOf("_CGoal") == -1) prompt.AddChoice(project);
                    }
                }
            }

            prompt.Title("Select a Project:");
            prompt.WrapAround(true);
            string? selected = AnsiConsole.Prompt(
                prompt
                );
            return selected == "Cancel" ? null : selected;
        }

        public static CodingSession GetSpecificData(List<CodingSession> datas)
        {
            try
            {
                SelectionPrompt<CodingSession> prompt = new();
                if (datas != null)
                {
                    prompt.AddChoice(new CodingSession { Average = "Cancel" });
                    foreach (CodingSession data in datas)
                    {
                        prompt.AddChoice(data);
                        prompt.Converter = data => $"{data.Start} | {data.End} | {data.Duration}{data.Average}";
                    }
                }
                prompt.Title("Select a Data:");
                prompt.WrapAround(true);
                CodingSession selected = AnsiConsole.Prompt(
                    prompt
                    );
                if (selected.Average != "Cancel") return selected;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                Console.Read();
            }
            return null;
        }

        public static List<CodingSession> GetMultipeData(List<CodingSession> datas)
        {
            try
            {
                MultiSelectionPrompt<CodingSession> prompt = new();
                if (datas != null)
                {
                    prompt.AddChoice(new CodingSession { Average = "Cancel" });
                    foreach (CodingSession data in datas)
                    {
                        prompt.AddChoice(data);
                        prompt.Converter = data => $"{data.Start} {data.End} | {data.Duration}{data.Average}";
                    }
                }
                prompt.Title("Select data(s)");
                prompt.WrapAround(true);
                List<CodingSession> selected = AnsiConsole.Prompt(prompt);
                if (selected[0].Id != 0) return selected;
            }
            catch (Exception ex) { AnsiConsole.Markup(ex.Message); }
            return null;
        }

        public static bool ValidateInput(string message = "", bool defVal = true, string choice1 = "y", string choice2 = "n")
        {
            bool validation = AnsiConsole.Prompt(
                                new TextPrompt<bool>(message)
                                .AddChoice(true)
                                .AddChoice(false)
                                .DefaultValue(defVal)
                                .WithConverter(choice => choice ? choice1 : choice2));
            return validation;
        }

        public static string SelectAscDesc()
        {
            SelectionPrompt<string> prompt = new SelectionPrompt<string>();
            prompt.Title("Select output order:");
            prompt.AddChoices("Ascendant", "Descendant");
            return AnsiConsole.Prompt(prompt).Substring(0, 3);
        }

        public static bool CompareDates(CodingSession session)
        {
            bool invalidDates = session.Start > session.End ;
            if (invalidDates) AnsiConsole.MarkupLine("Start Date/Time [red]greater[/] than End Date/Time.");
            return invalidDates;
        }
    }
}
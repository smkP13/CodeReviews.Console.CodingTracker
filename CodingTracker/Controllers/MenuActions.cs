using CodingTracker.Models;
using CodingTracker.Views;
using Spectre.Console;
using System.Configuration;

namespace CodingTracker.Controllers
{
    internal class MenuActions
    {
        internal MenuModel MenuModel { get; set; }
        internal DataTools DataTools { get; set; }

        internal MenuActions(DataTools dataTools)
        {
            DataTools = dataTools;
            MenuModel = new();
            SetMenuModel();
        }

        internal string SelectOption()
        {
            return AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("Choose an [yellow bold]option[/] below:")
                    .AddChoices("Start/End new coding session", "Insert new data", "Delete data",
                    "Update data", "Delete project", "Print single project report", "Print all data", "Set/Show Coding Goals", "See current session duration", "Exit", "Fill database for testing purpose")
                    .WrapAround(true)
                    );
        }

        internal void EndCurrentSession()
        {
            MenuModel.CurrentData = new();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MenuModel.IsCodingSessionRunning = false;
            config.AppSettings.Settings["newCodingSession"].Value = "false";
           
            MenuModel.CurrentData.Start = DateTime.Parse($"{config.AppSettings.Settings["currentStartDate"].Value} {config.AppSettings.Settings["currentStartTime"].Value}");
            MenuModel.CurrentData.End = DateTime.Now;
            MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
            MenuModel.CurrentData.Project = config.AppSettings.Settings["currentCodingSession"].Value;
            
            MenuModel.SqlCommandText = $@"INSERT INTO {MenuModel.CurrentData.Project} (Id,Start,End,Duration) VALUES((SELECT ifnull(Max(Id)+1,1) FROM {MenuModel.CurrentData.Project} ), $start, $end, $duration );";
            DataTools.ExecuteQuery(MenuModel.SqlCommandText, start: MenuModel.CurrentData.Start.ToString("yyyy-MM-dd HH:mm:ss"), end: MenuModel.CurrentData.End.ToString("yyyy-MM-dd HH:mm:ss"), duration: MenuModel.CurrentData.Duration.ToString());
            DataTools.UpdateIds(MenuModel.CurrentData,"insert");
            config.Save(ConfigurationSaveMode.Full);
        }

        internal void BeginNewCodingSession()
        {
            MenuModel.IsCodingSessionRunning = UserInputs.ValidateInput("Begin a new coding session?");
            
            if (MenuModel.IsCodingSessionRunning)
            {
                MenuModel.Project = UserInputs.SelectExistingProject(DataTools,newProject: true);
                if (MenuModel.Project != null)
                {
                    if (MenuModel.Project == "Add new Project")
                    {
                        MenuModel.Project = UserInputs.GetStringInput("Enter project [blue]name[/] (only [red]letter,numbers and '_'[/] are authorized):");
                        DataTools.CreateNewTable(MenuModel.Project);
                    }
                    MenuModel.CurrentCodingSession = MenuModel.Project;

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["newCodingSession"].Value = "true";
                    config.AppSettings.Settings["currentCodingSession"].Value = MenuModel.CurrentCodingSession;
                    config.AppSettings.Settings["currentStartDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                    config.AppSettings.Settings["currentStartTime"].Value = DateTime.Now.ToString("HH:mm");
                    config.Save(ConfigurationSaveMode.Full);

                    AnsiConsole.MarkupLine($"A new coding session has begun for [blue]{MenuModel.CurrentCodingSession}[/]");
                    Console.ReadKey();
                }
                else { MenuModel.IsCodingSessionRunning = false; }
            }
            else { MenuModel.IsCodingSessionRunning = false; }
        }

        internal void InsertNewData()
        {
            MenuModel.Project = UserInputs.SelectExistingProject(DataTools,newProject: true);
            if (MenuModel.Project != null)
            {
                if (MenuModel.Project == "Add new Project")
                {
                    MenuModel.CurrentData.Project = UserInputs.GetStringInput("Enter project [blue]name[/] (only [red]letter,numbers and '_'[/] are authorized):");
                    DataTools.CreateNewTable(MenuModel.Project);
                }
                MenuModel.CurrentData = new();
                MenuModel.CurrentData.Project = MenuModel.Project;
                MenuModel.CurrentData.Start = DateTime.Parse($"{UserInputs.GetDateTimeInput($"Pease enter a starting date for {MenuModel.Project} (format [green]yyyy.MM.dd[/]):")} {UserInputs.GetDateTimeInput($"Pease enter a starting time for {MenuModel.Project} (format [green]HH:mm[/]):", type: "Time")}");

                bool invalidDates;
                do 
                {
                    MenuModel.CurrentData.End = DateTime.Parse($"{UserInputs.GetDateTimeInput($"Pease enter an ending date for {MenuModel.Project} (format [green]yyyy.MM.dd[/]):")} {UserInputs.GetDateTimeInput($"Pease enter an ending time for {MenuModel.Project} (format [green]HH:mm[/]):", type: "Time")}");
                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                } while (invalidDates);

                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
                MenuModel.SqlCommandText = $@"INSERT INTO {MenuModel.Project} (Id,Start,End,Duration) VALUES((SELECT ifnull(Max(Id)+1,1) FROM {MenuModel.Project}),$start,$end,$duration);";
                DataTools.ExecuteQuery(MenuModel.SqlCommandText, start: MenuModel.CurrentData.Start.ToString("yyyy-MM-dd HH:mm:ss"), end: MenuModel.CurrentData.End.ToString("yyyy-MM-dd HH:mm:ss"), duration:MenuModel.CurrentData.Duration.ToString(), id: MenuModel.CurrentData.Id.ToString());
                
                MenuModel.CurrentData.Id = DataTools.GetAddedDataId(MenuModel.Project);
                DataTools.UpdateIds(MenuModel.CurrentData,"insert");
            }
        }
        internal void DeleteData()
        {
            MenuModel.Project = UserInputs.SelectExistingProject(DataTools);
            if (MenuModel.Project != null)
            {
                List<CodingSession> projectData = DataTools.GetProjectData(MenuModel.Project);
                if (projectData != null)
                {
                    List<CodingSession> selectedDatas = UserInputs.GetMultipeData(projectData);
                    if (selectedDatas != null)
                    {
                        foreach (CodingSession session in selectedDatas)
                        {
                            MenuModel.SqlCommandText = $"DELETE FROM {session.Project} WHERE Id = $id";
                            DataTools.ExecuteQuery(MenuModel.SqlCommandText, id: session.Id.ToString());
                            DataTools.UpdateIds(session);
                        }
                    }
                }
                else
                {
                    Console.ReadKey();
                }
            }
        }

        internal void UpdateData()
        {
            MenuModel.Project = UserInputs.SelectExistingProject(DataTools);
            if (MenuModel.Project != null)
            {
                List<CodingSession> projectData = DataTools.GetProjectData(MenuModel.Project);
                if (projectData != null)
                {
                    MenuModel.CurrentData = UserInputs.GetSpecificData(projectData);
                    if (MenuModel.CurrentData != null)
                    {
                        MenuModel.CurrentData.Project = MenuModel.Project;
                        string dataId = MenuModel.CurrentData.Id.ToString();
                        string dataToModify = AnsiConsole.Prompt(
                            new SelectionPrompt<string>().AddChoices("Start Date","Start Time","End Date","End Time"));
                        bool invalidDates;
                        switch (dataToModify)
                        {
                            case "Start Date":
                                DateTime updateIdSetting = MenuModel.CurrentData.Start;
                                string modifiedData;
                                do
                                {
                                    modifiedData = UserInputs.GetDateTimeInput($"Pease enter a starting date to replace (format [green]yyyy.MM.dd[/]):");
                                    MenuModel.CurrentData.Start = DateTime.Parse($"{modifiedData} {MenuModel.CurrentData.Start.ToString("HH:mm:ss")}");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);

                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET Start = $start , Duration = $duration WHERE Id = $id ";
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, start: MenuModel.CurrentData.Start.ToString("yyyy-MM-dd HH:mm:ss"), duration: MenuModel.CurrentData.Duration.ToString(),id:dataId);
                                
                                string updateOption = UpdateIdOption2(MenuModel.CurrentData.Start, updateIdSetting);
                                DataTools.UpdateIds(MenuModel.CurrentData, updateOption);
                                break;

                            case "Start Time":
                                updateIdSetting = MenuModel.CurrentData.Start;
                                do
                                {
                                    modifiedData = UserInputs.GetDateTimeInput($"Pease enter a starting time to replace (format [green]HH:mm[/]):", type: "Time");
                                    MenuModel.CurrentData.Start = DateTime.Parse($"{MenuModel.CurrentData.Start.ToString("yyyy.MM.dd")} {modifiedData}");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);

                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET Start = $start , Duration = $duration WHERE Id = $id ";
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, start: MenuModel.CurrentData.Start.ToString("yyyy-MM-dd HH:mm:ss"), duration: MenuModel.CurrentData.Duration.ToString(), id: dataId);
                                
                                updateOption = UpdateIdOption2(MenuModel.CurrentData.Start, updateIdSetting);
                                DataTools.UpdateIds(MenuModel.CurrentData, updateOption);
                                break;

                            case "End Date":
                                updateIdSetting = MenuModel.CurrentData.End;
                                do
                                {
                                    modifiedData = UserInputs.GetDateTimeInput($"Pease enter a starting date to replace (format [green]yyyy.MM.dd[/]):");
                                    MenuModel.CurrentData.End = DateTime.Parse($"{modifiedData} {MenuModel.CurrentData.Start.ToString("HH:mm:ss")}");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);

                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET End = $end , Duration = $duration WHERE Id = $id ";
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, end: MenuModel.CurrentData.End.ToString("yyyy-MM-dd HH:mm:ss"), duration: MenuModel.CurrentData.Duration.ToString(), id: dataId);
                                break;

                            case "End Time":
                                updateIdSetting = MenuModel.CurrentData.End;
                                do
                                {
                                    modifiedData = UserInputs.GetDateTimeInput($"Pease enter a starting time to replace (format [green]HH:mm[/]):", type: "Time");
                                    MenuModel.CurrentData.End = DateTime.Parse($"{MenuModel.CurrentData.Start.ToString("yyyy.MM.dd")} {modifiedData}");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);

                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET End = $end , Duration = $duration WHERE Id = $id ";
                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.Start, MenuModel.CurrentData.End);
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, end: MenuModel.CurrentData.End.ToString("yyyy-MM-dd HH:mm:ss"), duration: MenuModel.CurrentData.Duration.ToString(), id: dataId);
                                break;
                        }
                    }
                }
                else Console.ReadKey();
            }
        }

        internal void DeleteProject()
        {
            string projectType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Choose a type to delete: ")
                .AddChoices("Normal Project", "Goal Project"));

            if (projectType == "Normal Project") { MenuModel.Project = UserInputs.SelectExistingProject(DataTools); }
            else MenuModel.Project = UserInputs.SelectExistingProject(DataTools, Goal: true);
            
            if (MenuModel.Project != null)
            {
                bool validation = UserInputs.ValidateInput($"Are you sure you want to delete [red]{MenuModel.Project}[/]?", defVal: false);
                if (validation)
                {
                    MenuModel.SqlCommandText = $"DROP TABLE {MenuModel.Project}";
                    DataTools.ExecuteQuery(MenuModel.SqlCommandText);
                }
            }
        }

        internal void PrintSingleProjectReport()
        {
            MenuModel.Project = UserInputs.SelectExistingProject(DataTools);
            if (MenuModel.Project != null)
            {
                SelectionPrompt<string> prompt = new();

                // By getting total duration through sqlite, time calculation made by it can go off if reports are other than weekly(see DataOutput)
                prompt.Title("What kind of report do you need? (For more precise total duration report, select 'Weekly')");
                prompt.AddChoices("All data", "Weekly", "Monthly", "Yearly");
                string option = AnsiConsole.Prompt(prompt);
                string ascDesc = UserInputs.SelectAscDesc();
                DataOutput.PrintProjectData(DataTools,MenuModel.Project, ascDesc, option);
                Console.ReadKey();
            }
        }

        internal void PrintAllData()
        {
            string ascDesc = UserInputs.SelectAscDesc();
            List<string> allProjects = DataTools.GetTables();
            AnsiConsole.Clear();
            if (allProjects != null)
            {
                foreach (string project in allProjects) DataOutput.PrintProjectData(DataTools,project, ascDesc);
            }
            else
            {
                AnsiConsole.MarkupLine("There is [red bold]no[/] project in the data base");
            }
            Console.ReadKey();
        }

        internal void SeeShowGoals()
        {
            string setSee = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .AddChoices("Set", "Show"));
            if (setSee == "Set")
            {
                MenuModel.Project = UserInputs.GetStringInput("Enter a project name(_CGoal suffix will be add to it's name): ");
                DataTools.SetGoals(MenuModel.Project);
            }
            else
            {
                MenuModel.Project = UserInputs.SelectExistingProject(DataTools, Goal: true);
                if (MenuModel.Project != null) { DataOutput.ShowGoal(DataTools.GetGoalToShow(MenuModel.Project)); }
            }
            Console.ReadKey();
        }

        internal void ShowCurrentSession()
        {
            if (MenuModel.IsCodingSessionRunning)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                AnsiConsole.MarkupLine($"{DataTools.GetCurrentSessionDuration(config.AppSettings.Settings["currentStartDate"].Value, config.AppSettings.Settings["currentStartTime"].Value, DateTime.Now.ToString("yyyy.MM.dd"), DateTime.Now.ToString("HH:mm"))}");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No[/] coding session running.");
            }
            Console.ReadKey();
        }

        internal void SetMenuModel()
        {

            Configuration config1 = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            bool isSessionRunning;
            bool.TryParse(config1.AppSettings.Settings["newCodingSession"].Value, out isSessionRunning);
            MenuModel.IsCodingSessionRunning = isSessionRunning;
            MenuModel.CurrentCodingSession = config1.AppSettings.Settings["currentCodingSession"].Value;
        }

        private string UpdateIdOption(string sessionDateTime, string dateTime,string option = "date")
        {
            DateTime newDateTime = DateTime.Parse(dateTime);
            if(option == "date")
            {
                if (DateTime.Parse(sessionDateTime) > newDateTime)
                { return "updateUp"; }
                else return "updateDown";
            }
            else
            {
                if (DateTime.Parse(sessionDateTime) > newDateTime)
                { return "updateUp"; }
                else return "updateDown";
            }
        }

        private string UpdateIdOption2(DateTime newStart, DateTime oldStart,string option = "date")
        {
            if (option == "date")
            {
                if (oldStart > newStart)
                { return "updateUp"; }
                else return "updateDown";
            }
            else
            {
                if (newStart > oldStart)
                { return "updateUp"; }
                else return "updateDown";
            }
        }
    }
}
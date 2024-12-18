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
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MenuModel.IsCodingSessionRunning = false;
            config.AppSettings.Settings["newCodingSession"].Value = "false";

            MenuModel.StartDate = config.AppSettings.Settings["currentStartDate"].Value;
            MenuModel.StartTime = config.AppSettings.Settings["currentStarttime"].Value;
            MenuModel.EndDate = DateTime.Now.ToString("yyyy.MM.dd");
            MenuModel.EndTime = DateTime.Now.ToString("HH:mm");
            MenuModel.Duration = DataTools.GetDuration(MenuModel.StartDate, MenuModel.StartTime, MenuModel.EndDate, MenuModel.EndTime);

            MenuModel.SqlCommandText = @$"INSERT INTO {MenuModel.CurrentCodingSession} (Id,StartDate,EndDate,StartTime,EndTime,Duration)
                                       VALUES((SELECT Ifnull(Max(Id)+1,1) Id FROM {MenuModel.Project}),$startDate,$endDate,$startTime,$endTime,$duration)";
            DataTools.ExecuteQuery(MenuModel.SqlCommandText, startDate: MenuModel.StartDate, endDate: MenuModel.EndDate, startTime: MenuModel.StartTime, endTime: MenuModel.EndTime, duration: MenuModel.Duration);
            
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
                    MenuModel.Project = UserInputs.GetStringInput("Enter project [blue]name[/] (only [red]letter,numbers and '_'[/] are authorized):");
                    DataTools.CreateNewTable(MenuModel.Project);
                }
                MenuModel.CurrentData = new();
                MenuModel.CurrentData.Project = MenuModel.Project;
                MenuModel.CurrentData.StartDate = UserInputs.GetDateTimeInput($"Pease enter a starting date for {MenuModel.Project} (format [green]yyyy.MM.dd[/]):");
                MenuModel.CurrentData.StartTime = UserInputs.GetDateTimeInput($"Pease enter a starting time for {MenuModel.Project} (format [green]HH:mm[/]):", type: "Time");
                
                bool invalidDates;
                do {
                    MenuModel.CurrentData.EndDate = UserInputs.GetDateTimeInput($"Pease enter an ending date for {MenuModel.Project} (format [green]yyyy.MM.dd[/]):");
                    MenuModel.CurrentData.EndTime = UserInputs.GetDateTimeInput($"Pease enter an ending time for {MenuModel.Project} (format [green]HH:mm[/]):", type: "Time");
                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                } while (invalidDates);

                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.StartDate, MenuModel.CurrentData.StartTime, MenuModel.CurrentData.EndDate, MenuModel.CurrentData.EndTime);
                MenuModel.SqlCommandText =  @$"INSERT INTO {MenuModel.Project} (Id,StartDate,EndDate,StartTime,EndTime,Duration)
                                            VALUES((SELECT Ifnull(Max(Id)+1,1) Id FROM {MenuModel.Project}),$startDate,$endDate,$startTime,$endTime,$duration)";
                DataTools.ExecuteQuery(MenuModel.SqlCommandText, startDate: MenuModel.CurrentData.StartDate, endDate: MenuModel.CurrentData.EndDate, startTime: MenuModel.CurrentData.StartTime, endTime: MenuModel.CurrentData.EndTime, duration: MenuModel.CurrentData.Duration);
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
                            new SelectionPrompt<string>().AddChoices("Start Date", "Start Time", "End Date", "End Time"));
                        bool invalidDates;
                        switch (dataToModify)
                        {
                            case "Start Date":
                                string updateIdSetting;
                                updateIdSetting = MenuModel.CurrentData.StartDate;
                                do
                                {
                                    MenuModel.CurrentData.StartDate = UserInputs.GetDateTimeInput($"Pease enter a starting date to replace (format [green]yyyy.MM.dd[/]):");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET StartDate = $startDate , Duration = $duration WHERE Id = $id ";
                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.StartDate, MenuModel.CurrentData.StartTime, MenuModel.CurrentData.EndDate, MenuModel.CurrentData.EndTime);
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, startDate: MenuModel.CurrentData.StartDate, duration: MenuModel.CurrentData.Duration, id: dataId);
                                string updateOption = UpdateIdOption(MenuModel.CurrentData.StartDate, updateIdSetting);
                                DataTools.UpdateIds(MenuModel.CurrentData,updateOption);
                                break;

                            case "Start Time":
                                updateIdSetting = MenuModel.CurrentData.StartTime;
                                do
                                {
                                    MenuModel.CurrentData.StartTime = UserInputs.GetDateTimeInput($"Pease enter a starting time to replace (format [green]HH:mm[/]):", type: "Time");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET StartTime = $startTime , Duration = $duration WHERE Id = $id ";
                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.StartDate, MenuModel.CurrentData.StartTime, MenuModel.CurrentData.EndDate, MenuModel.CurrentData.EndTime);
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, startTime: MenuModel.CurrentData.StartTime, duration: MenuModel.CurrentData.Duration, id: dataId);
                                updateOption = UpdateIdOption(MenuModel.CurrentData.StartTime, updateIdSetting);
                                DataTools.UpdateIds(MenuModel.CurrentData ,updateOption);
                                break;

                            case "End Date":
                                do
                                {
                                    MenuModel.CurrentData.EndDate = UserInputs.GetDateTimeInput($"Pease enter an ending date to replace (format [green]yyyy.MM.dd[/]):");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET EndDate = $endDate , Duration = $duration WHERE Id = $id ";
                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.StartDate, MenuModel.CurrentData.StartTime, MenuModel.CurrentData.EndDate, MenuModel.CurrentData.EndTime);
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, endDate: MenuModel.CurrentData.EndDate, duration: MenuModel.CurrentData.Duration, id: dataId);
                                break;

                            case "End Time":
                                do
                                {
                                    MenuModel.CurrentData.EndTime = UserInputs.GetDateTimeInput($"Pease enter an ending time to replace (format [green]HH:mm[/]):", type: "Time");
                                    invalidDates = UserInputs.CompareDates(MenuModel.CurrentData);
                                } while (invalidDates);
                                MenuModel.SqlCommandText = $"UPDATE {MenuModel.Project} SET EndTime = $endTime , Duration = $duration WHERE Id = $id";
                                MenuModel.CurrentData.Duration = DataTools.GetDuration(MenuModel.CurrentData.StartDate, MenuModel.CurrentData.StartTime, MenuModel.CurrentData.EndDate, MenuModel.CurrentData.EndTime);
                                DataTools.ExecuteQuery(MenuModel.SqlCommandText, endTime: MenuModel.CurrentData.EndTime, duration: MenuModel.CurrentData.Duration, id: dataId);
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
                AnsiConsole.MarkupLine($"{DataTools.GetDuration(config.AppSettings.Settings["currentStartDate"].Value, config.AppSettings.Settings["currentStartTime"].Value, DateTime.Now.ToString("yyyy.MM.dd"), DateTime.Now.ToString("HH:mm"))}");
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
    }
}
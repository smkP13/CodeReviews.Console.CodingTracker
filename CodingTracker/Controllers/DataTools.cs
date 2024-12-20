using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Configuration;
using Dapper;
using System.Globalization;
using System.Data;
using CodingTracker.Models;

namespace CodingTracker.Controllers
{
    internal class DataTools
    {
        internal string? ConnectionString { get; set; }

        internal void Initialize()
        {
            try
            { 
                if (ConfigurationManager.AppSettings.Get("connectionString") == "")
                {
                    string? appFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    int pathLength = appFolderPath.Length - 16;
                    string dbFolderPath = appFolderPath.Substring(0, pathLength);
                    ConfigurationManager.AppSettings.Set("dbPath", dbFolderPath);
                    ConfigurationManager.AppSettings.Set("connectionString", $"Data Source= {dbFolderPath}CodingTracker.db");
                }
                ConnectionString = ConfigurationManager.AppSettings.Get("connectionString");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                Console.Read();
            }
        }

        internal List<CodingSession> GetProjectData(string project, string ascDesc = "Asc", string option = "All data")
        {
            try
            {
                string? sqlCommand;
                CodingSession currentData = new();
                List<CodingSession> reports = new();
                string[] dateStr = GetOppositeDates(project);
                if (dateStr[0] != null)
                {
                    dateStr[0] = $"{dateStr[0].Substring(0, 10)} 23:59:59";
                    dateStr[1] = $"{dateStr[1].Substring(0, 10)} 00:00:00";
                }
                else return null;
                DateTime firstDate;
                DateTime lastDate;

                bool haveData = DateTime.TryParseExact(dateStr[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out firstDate);
                haveData = DateTime.TryParseExact(dateStr[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastDate);
                DateTime secondDate = firstDate;
                DateTime currentDate = firstDate;
                if (option == "All data")
                {
                    if (project.IndexOf("_CGoal") == -1)
                    {
                        try
                        {
                            sqlCommand = $"SELECT * from {project} ";
                            using (SqliteConnection connection = new(ConnectionString))
                            {
                                IDataReader reader = connection.ExecuteReader(sqlCommand);
                                while (reader.Read())
                                {
                                    reports.Add(new CodingSession
                                    {
                                        Id = int.Parse(reader["Id"].ToString()),
                                        Start = DateTime.Parse(reader["Start"].ToString()),
                                        End = DateTime.Parse(reader["End"].ToString()),
                                        Duration = TimeSpan.Parse(reader["Duration"].ToString()),
                                        Project = project
                                    });
                                }
                                reader.Close();
                                return ascDesc == "Asc" ? reports : ReverseList(reports);
                            }
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    else return null;
                }
                else if (option == "Weekly")
                {
                    secondDate = firstDate.DayOfWeek.ToString() switch
                    {
                        "Monday" => firstDate.AddDays(-1).AddSeconds(1),
                        "Tuesday" => firstDate.AddDays(-2).AddSeconds(1),
                        "Wednesday" => firstDate.AddDays(-3).AddSeconds(1),
                        "Thursday" => firstDate.AddDays(-4).AddSeconds(1),
                        "Friday" => firstDate.AddDays(-5).AddSeconds(1),
                        "Saturday" => firstDate.AddDays(-6).AddSeconds(1),
                        "Sunday" => firstDate.AddDays(-7).AddSeconds(1),
                        _ => firstDate,
                    };
                }
                else if (option == "Monthly") { secondDate = firstDate.AddDays(-firstDate.Day + 1); }
                else if (option == "Yearly") { secondDate = firstDate.AddDays(-firstDate.DayOfYear + 1); }

                int sessionCount = 0;
                TimeSpan finalDuration = TimeSpan.ParseExact("00:00:00", "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                do
                {
                    try
                    {
                        using (SqliteConnection connection = new(ConnectionString))
                        {
                            sqlCommand = @$"SELECT 	
                                         printf('%.2i',(avg(strftime('%M', Duration)) / 60 + avg(strftime('%H', Duration))) % 24) as avgHours,
	                                     printf('%.2i',avg(strftime('%M',Duration))) as avgMinutes,
                                         printf('%.2i',sum(strftime('%H',Duration)) / 24) as days,
	                                     printf('%.2i',sum(strftime('%M', Duration)) / 60 + sum(strftime('%H', Duration))) % 24 as hours,
                                         printf('%.2i', sum(strftime('%M', Duration)) % 60) as minutes,
                                         count(Duration) as DurationCount
                                         FROM {project} WHERE Start <= '{currentDate.ToString("yyyy-MM-dd HH:mm:ss")}' AND Start >= '{secondDate.ToString("yyyy-MM-dd HH:mm:ss")}'";
                            currentData = connection.Query<CodingSession>(sqlCommand).ToList()[0];
                            currentData.Project = project;
                            IDataReader reader = connection.ExecuteReader(sqlCommand);
                            reader.Read();
                            currentData.TotalDuration = $"{reader["days"]:D2}.{reader["hours"]:D2}:{reader["minutes"]:D2}:00";
                            currentData.DurationCount = int.Parse(reader["DurationCount"].ToString());
                            reader.Close();
                            if (currentData.DurationCount != 0)
                            {
                                currentData.Start = secondDate;
                                currentData.End = currentDate;
                                sessionCount += currentData.DurationCount;
                                finalDuration = finalDuration.Add(TimeSpan.ParseExact(currentData.TotalDuration, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None));
                                reports.Add(currentData);
                            }

                            currentDate = secondDate.AddSeconds(-1);
                            secondDate = option switch
                            {
                                "Weekly" => secondDate.AddDays(-7),
                                "Monthly" => secondDate.AddMonths(-1),
                                "Yearly" => secondDate.AddYears(-1)
                            };
                        }

                        if (currentDate < lastDate)
                        {
                            reports.Add(new CodingSession
                            {
                                Project = "Final Result",
                                Start = firstDate,
                                End = lastDate,
                                TotalDuration = finalDuration.ToString(),
                                DurationCount = sessionCount,
                                Average = (finalDuration / sessionCount).ToString().Substring(0, 5),
                            });
                            return ascDesc == "Asc" ? reports : ReverseList(reports, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                } while (haveData);

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                Console.Read();
            }
            return null;
        }

        public void SetGoals(string project)
        {
            try
            {
                using (SqliteConnection connection = new(ConnectionString))
                {
                    string? sqlCommand;
                    sqlCommand = $"CREATE TABLE IF NOT EXISTS \"{project}_CGoal\" (Project TEXT,Start DATE,End DATE,DurationEstimation TIME,DurationPerDay TIME,NumberOfDays INTEGER)";
                    ExecuteQuery(sqlCommand);
                    sqlCommand = $"SELECT * FROM {project}_CGoal";
                    IDataReader reader = connection.ExecuteReader(sqlCommand);
                    bool goalNotSet = reader.Depth > 0 ? false : true;
                    reader.Close();
                    if (goalNotSet)
                    {
                        DateTime start = DateTime.Parse(UserInputs.GetDateTimeInput("Choose a starting date"));
                        DateTime end;
                        do
                        {
                            end = DateTime.Parse(UserInputs.GetDateTimeInput("Choose an ending date"));
                        } while (start > end);
                        string durationEstimation = UserInputs.GetDurationEstimation();
                        int days = GetNumberOfDays(start.ToString("yyyy.MM.dd"), end.ToString("yyyy.MM.dd"));
                        TimeSpan timePerDay = TimeSpan.ParseExact(durationEstimation, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                        timePerDay = timePerDay / days;
                        sqlCommand = $"INSERT INTO {project}_CGoal (Project,Start,End,DurationEstimation,DurationPerDay,NumberOfDays) VALUES('{project}','{start.ToString("yyyy-MM-dd")}','{end.ToString("yyyy-MM-dd")}','{durationEstimation}','{timePerDay}',{days})";
                        ExecuteQuery(sqlCommand);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("This projec already have goal set");
                        Console.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine(ex.Message);
                Console.Read();
            }
        }

        public CodingSession GetGoalToShow(string project)
        {
            try
            {
                using (SqliteConnection connection = new(ConnectionString))
                {
                    string sqlCommand = $"SELECT Start,End,DurationEstimation,DurationPerDay FROM {project}";
                    IDataReader reader = connection.ExecuteReader(sqlCommand);
                    reader.Read();
                    CodingSession session = new CodingSession
                    {
                        Project = project,
                        Start = DateTime.Parse(reader["Start"].ToString()),
                        End = DateTime.Parse(reader["End"].ToString()),
                        TotalDuration = reader["DurationEstimation"].ToString(),
                        Duration = TimeSpan.Parse(reader["DurationPerDay"].ToString())
                    };
                    reader.Close();
                    return session;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"The project [blue bold]{project} is not set as a Coding Goal");
                AnsiConsole.WriteLine(ex.Message);
                return null;
            }
        }

        public string GetCurrentSessionDuration(string StartDate, string StartTime, string EndDate, string EndTime)
        {

            DateTime start = DateTime.Parse($"{StartDate} {StartTime}");
            DateTime end = DateTime.Parse($"{EndDate} {EndTime}");
            string duration = end.Subtract(start).ToString();
            return duration.Substring(0, duration.Length - 3);
        }

        internal TimeSpan GetDuration (DateTime start,DateTime end)
        {
            // The string conversion and parse is made to always have a format of d.HH:mm:ss
            TimeSpan duration = end.Subtract(start);
            string durationString = duration.ToString();
            durationString = durationString.Substring(0,8).Contains('.') ? durationString.Substring(0,10) : durationString.Substring(0,8);
            duration = TimeSpan.Parse(durationString);
            return duration;
        }

        public int GetNumberOfDays(string Start, string End)
        {
            DateTime start = DateTime.Parse(Start);
            DateTime end = DateTime.Parse(End);
            return end.Subtract(start).Days;
        }

        public List<string> GetTables()
        {
            using (SqliteConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string sqlCommand = $"SELECT name FROM sqlite_schema WHERE type='table' ORDER BY name";
                    return connection.Query<string>(sqlCommand).ToList();
                }
                catch
                {
                    AnsiConsole.MarkupLine("Database doesn't contain any table/project yet.");
                    Console.Read();
                    return null;
                }
            }
        }

        public void CreateNewTable(string project)
        {
            using (SqliteConnection connection = new(ConnectionString))
            {
                string sqlCommand2 = $"CREATE TABLE IF NOT EXISTS \"{project}\" (Id INTEGER PRIMARY KEY,Start TEXT,End TEXT,Duration TEXT) WITHOUT ROWID;"; 
                connection.Query(sqlCommand2);
            }
        }

        private string[] GetOppositeDates(string project)
        {
            using (SqliteConnection connection = new(ConnectionString))
            {
                string sqlCommand = $"SELECT MAX(Start) FROM {project}";
                List<string> firstDate = connection.Query<string>(sqlCommand).ToList();

                sqlCommand = $"SELECT MIN(Start) FROM {project}";
                List<string> lastDate = connection.Query<string>(sqlCommand).ToList();
                return [firstDate[0], lastDate[0]];
            }
        }

        private List<CodingSession> ReverseList(List<CodingSession> list, bool allData = true)
        {
            int r = allData == true ? list.Count - 1 : list.Count - 2;
            CodingSession tempo;
            for (int l = 0; l < r; l++)
            {
                tempo = list[l];
                list[l] = list[r];
                list[r] = tempo;
                r--;
            }
            return list;
        }

        public void ExecuteQuery(string sqlCommand, string startDate = "", string endDate = "", string startTime = "", string endTime = "", string duration = "", string id = "",string start = "", string end = "")
        {
            try
            {
                using (SqliteConnection connection = new(ConnectionString))
                {
                    connection.Query(sqlCommand,
                        new { id, startDate, startTime, endDate, endTime, duration, start, end });
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                Console.Read();
            }
        }

        internal void AddDataToDB()
        {
            try
            {
                using (SqliteConnection connection = new(ConnectionString))
                {
                    CreateNewTable("test_table");
                    string? sqlCommand = "SELECT * FROM test_table";
                    DateTime date = DateTime.Now.AddDays(-50);
                    DateTime startDate = date.AddDays(-1).AddHours(15).AddMinutes(30);
                    CreateNewTable("test_table");
                    object? reader = connection.ExecuteScalar(sqlCommand);
                    bool tableNotFilled = reader == null;
                    if (tableNotFilled)
                    {
                        for (int i = 1; i < 51; i++)
                        {
                            sqlCommand = "INSERT INTO test_table (Id,Start,End,Duration) VALUES($id,$start,$end,$duration);";
                            TimeSpan Duration = GetDuration(startDate, date);
                            ExecuteQuery(sqlCommand, start: startDate.ToString("yyyy-MM-dd HH:mm:ss"),end: date.ToString("yyyy-MM-dd HH:mm:ss"),duration:Duration.ToString(),id:i.ToString());
                            startDate = startDate.AddDays(1);
                            date = date.AddDays(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine(ex.Message);
                AnsiConsole.WriteLine(ex.Source);
            }
        }

        internal void UpdateIds(CodingSession session,string option = "")
        {
            string sqlCommand = "";
            if (option == "insert")
            {
                sqlCommand = $@"ALTER TABLE {session.Project} ADD COLUMN tempoId;
                                UPDATE {session.Project} SET tempoId = Id + 1 WHERE Start > $start;
                                UPDATE {session.Project} SET Id = Id + (SELECT MAX(Id) from {session.Project}) WHERE tempoId IS NOT NULL;
                                UPDATE {session.Project} SET Id = (SELECT ifnull(Min(tempoId)-1,Id) FROM {session.Project}) WHERE Id = $id;
                                UPDATE {session.Project} SET Id = tempoId WHERE tempoId IS NOT NULL;
                                ALTER TABLE {session.Project} DROP COLUMN tempoId;";
            }
            else if (option == "updateUp")
            {
                sqlCommand = $@"ALTER TABLE {session.Project} ADD COLUMN tempoId;
                                UPDATE {session.Project} SET tempoId = Id + 1 WHERE Start > $start AND Id < $id;
                                UPDATE {session.Project} SET Id = Id + (SELECT MAX(Id) from {session.Project}) WHERE tempoId IS NOT NULL;
                                UPDATE {session.Project} SET Id = (SELECT ifnull(Min(tempoId)-1,$id) FROM {session.Project} WHERE tempoId IS NOT NULL) WHERE Id = $id;
                                UPDATE {session.Project} SET Id = IIF((SELECT Min(tempoId)-1 FROM {session.Project}) = {session.Id},tempoId-1,tempoId) WHERE tempoId is NOT NULL;
                                ALTER TABLE {session.Project} DROP COLUMN tempoId;";
            }
            else if (option == "updateDown")
            {
                sqlCommand = $@"ALTER TABLE {session.Project} ADD COLUMN tempoId;
                                UPDATE {session.Project} SET tempoId = Id - 1 WHERE Start < $start AND Id > $id;
                                UPDATE {session.Project} SET Id = Id + (SELECT MAX(Id) from {session.Project}) WHERE tempoId IS NOT NULL;
                                UPDATE {session.Project} SET Id = (SELECT ifnull(MAX(tempoId)+1,$id) FROM {session.Project} WHERE tempoId IS NOT NULL) WHERE Id = $id;
                                UPDATE {session.Project} SET Id = IIF((SELECT Max(tempoId)+1 FROM {session.Project}) = {session.Id},tempoId+1,tempoId) WHERE tempoId is NOT NULL;
                                ALTER TABLE {session.Project} DROP COLUMN tempoId;";
            }
            else
            {
                sqlCommand = $@"ALTER TABLE {session.Project} ADD COLUMN tempoId;
                                   UPDATE {session.Project} SET tempoId = Id-1 WHERE Id > $id;
                                   UPDATE {session.Project} SET Id = Id + (SELECT MAX(Id) FROM {session.Project}) WHERE Id > $id;
                                   UPDATE {session.Project} SET Id = tempoId WHERE Id > $id;
                                   ALTER TABLE {session.Project} DROP COLUMN tempoId;";
            }
            ExecuteQuery(sqlCommand, id: session.Id.ToString(), start: session.Start.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        internal int GetAddedDataId(string project)
        {
            using(SqliteConnection connection = new(ConnectionString))
            {
                return connection.Query<int>($"SELECT MAX(Id) from {project}").ToList()[0];
            }
        }
    }

}
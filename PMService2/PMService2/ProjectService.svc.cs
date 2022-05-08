using PMService2.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace PMService2
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ProjectService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ProjectService.svc or ProjectService.svc.cs at the Solution Explorer and start debugging.
    public class ProjectService : IProjectService
    {
        public static String DBName { get; set; } = "PMService2DB.db";
        public static List<Message> messagesCache = new List<Message>();
        public static void Log(string v)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("G")}] >> {v}");
            Console.ResetColor();
        }

        public bool RequestState(string DeviceID)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Log($"PC Client {DeviceID} Requested Server State");
            return true;
        }

        public void IntializeDatabaseService()
        {
            //Directory.CreateDirectory("ProfilePics");
            if (!File.Exists(DBName))
                File.Create(DBName);

            Log("Database Path Set");

            try
            {
                using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
                {
                    con.Open();
                    string dbScript = "CREATE TABLE IF NOT EXISTS " +
                    "message ( " +
                        "MessageContent TEXT, " +
                        "isSticker int, " +
                        "sender TEXT, " +
                        "receiver TEXT, " +
                        "Time TEXT, " +
                        "MentionedUsers TEXT ); " +
                    "CREATE TABLE IF NOT EXISTS " +
                    "project ( " +
                        "ProjectID TEXT, " +
                        "CreatedOn TEXT, " +
                        "CreatedBy TEXT, " +
                        "Title TEXT, " +
                        "ProjectManager TEXT, " +
                        "Members TEXT, " +
                        "Description TEXT, " +
                        "Category TEXT, " +
                        "StartDate TEXT, " +
                        "EndDate TEXT, " +
                        "Status TEXT );";

                    SQLiteCommand SQLiteCommand = new SQLiteCommand(dbScript, con);
                    SQLiteCommand.ExecuteNonQuery();
                    Log("Server Database Initialized");
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log(ex.ToString());
            }

            FetchAllMessages();

        }

        public List<Message> FindDirectMessagesFor(string user, DateTime lastMessage)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Log($"Requested Direct messages For the relation {user}");

            Log("FetchingMessages from the database");
            var allMessagesForUser = new List<Message>();
            var newMessages = new List<Message>();
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT MessageContent, isSticker, sender, receiver, Time, MentionedUsers FROM message WHERE sender = @sender OR receiver = @rsender";
                SQLiteCommand sqliteCommand = new SQLiteCommand(dbScript, con);
                sqliteCommand.Parameters.AddWithValue("@sender", user);
                sqliteCommand.Parameters.AddWithValue("@rsender", user);

                SQLiteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    var MentionedUserList = new List<string>();
                    var mentionedUsers = reader.GetString(5);
                    if (mentionedUsers.Contains(','))
                    {
                        MentionedUserList = mentionedUsers.Split(',').ToList();
                    }
                    else if (mentionedUsers.Length > 0)
                    {
                        MentionedUserList.Add(mentionedUsers);
                    }
                    else
                    {
                        MentionedUserList = new List<string>();
                    }

                    allMessagesForUser.Add(new Message()
                    {
                        MessageContent = reader.GetString(0),
                        isSticker = reader.GetInt32(1) == 0 ? false : true,
                        sender = reader.GetString(2),
                        receiver = reader.GetString(3),
                        Time = DateTime.Parse(reader.GetString(4)),
                        MentionedUsers = MentionedUserList
                    }) ;
                }
                Log($"Found {allMessagesForUser.Count} Messages For the Requsted Relation");
                con.Close();
            }

            if (lastMessage != null)
            {
                Log($"Filtering Messages From {lastMessage.ToString("g")}");
                foreach (var message in allMessagesForUser)
                {
                    if (message.Time > lastMessage)
                    {
                        newMessages.Add(message);
                    }
                    
                }

            }
            else
            {
                newMessages = allMessagesForUser;
                
            }
            Log($"Found {newMessages.Count} new messages");

            return newMessages;

            // Last message

            // if the last message is null, Send the whole list.
            // if the last message is not null, send the messages with a larger timestamp. 

        }

        public List<Message> FetchAllMessages()
        {
            messagesCache.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Log($"Updating Message Cache");

            Log("Fetching Messages from the database");
            var allMessagesForUser = new List<Message>();
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT MessageContent, isSticker, sender, receiver, Time, MentionedUsers FROM message";
                SQLiteCommand sqliteCommand = new SQLiteCommand(dbScript, con);

                SQLiteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    var MentionedUserList = new List<string>();
                    var mentionedUsers = reader.GetString(5);
                    if (mentionedUsers.Contains(','))
                    {
                        MentionedUserList = mentionedUsers.Split(',').ToList();
                    }
                    else if (mentionedUsers.Length > 0)
                    {
                        MentionedUserList.Add(mentionedUsers);
                    }
                    else
                    {
                        MentionedUserList = new List<string>();
                    }

                    messagesCache.Add(new Message()
                    {
                        MessageContent = reader.GetString(0),
                        isSticker = reader.GetInt32(1) == 0 ? false : true,
                        sender = reader.GetString(2),
                        receiver = reader.GetString(3),
                        Time = DateTime.Parse(reader.GetString(4)),
                        MentionedUsers = MentionedUserList
                    });
                }
                Log($"Found {allMessagesForUser.Count} Total Messages");
                con.Close();
            }

            

            return messagesCache;

            // Last message

            // if the last message is null, Send the whole list.
            // if the last message is not null, send the messages with a larger timestamp. 

        }

        public bool CheckNewMessagesFor(string username, DateTime latestMessageTime)
        {
            //Console.ForegroundColor = ConsoleColor.Blue;
            //Log($"Requested Count Of New Messages For {username} From {latestMessageTime.ToShortTimeString()}");

            //Log("FetchingMessages from the database");
            var newMessages = 0;

            foreach (var message in messagesCache)
            {
                if ((message.sender == username || message.receiver == username) && message.Time > latestMessageTime)
                {
                    newMessages++;
                }
            }

            if (newMessages > 0)
            {
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Log($"{newMessages} New Message(s) For {username} as {latestMessageTime.ToString("G")}");
                return true;
            }

            return false;
        }

        public bool DeleteMessagesFrom(string sender, string receiver)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Log($"Requested Delete Direct messages For the relation {sender} and {receiver}");

            try
            {
                Log("Deleting Messages from the database");
                var allMessagesForUser = new List<Message>();
                var newMessages = new List<Message>();
                using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();

                    string dbScript = "DELETE FROM message WHERE (sender=@sender AND receiver=@receiver) OR (sender=@sreceiver AND receiver=@rsender);";
                    SQLiteCommand sqliteCommand = new SQLiteCommand(dbScript, con);
                    sqliteCommand.Parameters.AddWithValue("@sender", sender);
                    sqliteCommand.Parameters.AddWithValue("@receiver", receiver);
                    sqliteCommand.Parameters.AddWithValue("@rsender", sender);
                    sqliteCommand.Parameters.AddWithValue("@sreceiver", receiver);

                    var affRows = sqliteCommand.ExecuteNonQuery();

                    Log($"Deleted {affRows} Messages From the Database");
                    con.Close();
                    FetchAllMessages();
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log(ex.ToString());
                return false;
            }
            

            // Last message

            // if the last message is null, Send the whole list.
            // if the last message is not null, send the messages with a larger timestamp. 
        }

        public bool NewMessage(Message message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Log("New Message Invoked");
            var CommandString = "INSERT INTO message VALUES(@messageContent, @isSticker, @sender, @receiver, @time, @mentionedUsers);";

            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    con.Open();
                    Log($"Saving Message From {message.sender}");
                    SQLiteCommand command = con.CreateCommand();
                    command.CommandText = CommandString;
                    command.Parameters.AddWithValue("@messageContent", message.MessageContent);
                    command.Parameters.AddWithValue("@isSticker", message.isSticker ? 1 : 0);
                    command.Parameters.AddWithValue("@sender", message.sender);
                    command.Parameters.AddWithValue("@receiver", message.receiver);
                    command.Parameters.AddWithValue("@time", message.Time.ToString("G"));
                    if (message.MentionedUsers != null)
                    {
                        Log("Saving Mentioned Users");
                        if (message.MentionedUsers.Count > 1)
                        {
                            Log($"Found {message.MentionedUsers.Count} Mentioned Users");
                            StringBuilder mentionUserString = new StringBuilder();
                            foreach (var user in message.MentionedUsers)
                            {
                                mentionUserString.Append(user + ",");
                            }
                            command.Parameters.AddWithValue("@mentionedUsers", mentionUserString.ToString().Substring(0, mentionUserString[mentionUserString.Length - 1]));
                        }
                        else if(message.MentionedUsers.Count > 0)
                        {
                            Log($"Found {message.MentionedUsers.Count} Mentioned Users");
                            command.Parameters.AddWithValue("@mentionedUsers", message.MentionedUsers.ToArray()[0]);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@mentionedUsers", "");
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@mentionedUsers", "");
                    }

                    var affRows = command.ExecuteNonQuery();

                    if (affRows > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Log("Successfully saved the message");
                        con.Close();
                        FetchAllMessages();
                        return true;
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }
            FetchAllMessages();
            return false;
        }

        /// <summary>
        /// Creates anew project in the database
        /// </summary>
        /// <param name="project">Project object with all details</param>
        /// <returns>True if successfull creation, False if something fails. Look at the Server console for more information</returns>
        public bool CreateProject(Project project)
        {
            // Update the serve console
            Console.ForegroundColor = ConsoleColor.Blue;
            Log($"Create Project Invoked by {project.CreatedBy}");


            // Create the SQLite Connection
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    // open the connection
                    con.Open();

                    // Create the SQLite Command
                    Log("Saving Project");
                    SQLiteCommand CMDSaveProject = new SQLiteCommand();
                    CMDSaveProject.CommandText = 
                        "INSERT INTO project " +
                        "VALUES( " +
                            "@projectId, " +
                            "@createdOn, " +
                            "@createdBy, " +
                            "@title, " +
                            "@projectManager, " +
                            "@members, " +
                            "@description, " +
                            "@category, " +
                            "@startDate, " +
                            "@endDate, " +
                            "@status" +
                        " );";

                    // Assignees are sent as an list of string objects
                    // we have to create a single string to save it in the database.
                    StringBuilder assigneesString = new StringBuilder();

                    if (project.Assignees != null && project.Assignees.Count > 1)
                    {
                        foreach (var assignee in project.Assignees)
                        {
                            // append the value with a comma for the string bulder.
                            // this comma can be used to detect and create the list string back from the string.
                            assigneesString.Append(assignee+",");
                        }
                    }
                    else
                    {
                        // of there is no more than 1 value, there is no need to seperate. Just append it to the builder.
                        // of the count not one, it means, it dont have any values, in that case, append the string ans an empty value.
                        if(project.Assignees.Count == 1)
                            assigneesString.Append(project.Assignees.ToArray()[0]);
                        else
                            assigneesString.Append("");
                    }
                   
                    // set the parameters
                    CMDSaveProject.Parameters.AddWithValue("@projectId", project.ProjectId);
                    CMDSaveProject.Parameters.AddWithValue("@createdOn", project.CreatedOn.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@createdBy", project.CreatedBy);
                    CMDSaveProject.Parameters.AddWithValue("@title", project.Title);
                    CMDSaveProject.Parameters.AddWithValue("@projectManager", project.ProjectManager);
                    CMDSaveProject.Parameters.AddWithValue("@members", assigneesString.ToString());
                    CMDSaveProject.Parameters.AddWithValue("@description", project.Description);
                    CMDSaveProject.Parameters.AddWithValue("@category", project.Category);
                    CMDSaveProject.Parameters.AddWithValue("@startDate", project.StartDate.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@endDate", project.EndDate.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@status", project.Status);

                    // set the connection for the command
                    CMDSaveProject.Connection = con;

                    // extecute the command aget the affected row count
                    var affRows = CMDSaveProject.ExecuteNonQuery();

                    // if the affected rows count is larger than 0 that means it is successfully recorded in the database
                    // if not that means there is ban isuue occured.
                    if (affRows > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Log($"Completedly Added project {project.Title}");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Log($"Failed to add project {project.Title}");
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    // incase of an error, show the error on the server console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }

            // Continuing to gete means abouve functions must have generates and uncaought error
            // this can be an something to dfo with input data
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Something Went Wrong");
            return false;
        }

        public bool UpdateProject(Project project, string username)
        {
            // Update the serve console
            Console.ForegroundColor = ConsoleColor.Blue;
            Log($"Update Project Invoked by {project.CreatedBy}");


            // Create the SQLite Connection
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    // open the connection
                    con.Open();

                    // Create the SQLite Command
                    Log("Updating Project");
                    SQLiteCommand CMDSaveProject = new SQLiteCommand();
                    CMDSaveProject.CommandText =
                        "UPDATE project " +
                        "SET " +
                            "CreatedOn=@createdOn, " +
                            "CreatedBy=@createdBy, " +
                            "Title=@title, " +
                            "ProjectManager=@projectManager, " +
                            "Members=@members, " +
                            "Description=@description, " +
                            "Category=@category, " +
                            "StartDate=@startDate, " +
                            "EndDate=@endDate, " +
                            "Status=@status" +
                        " WHERE " +
                            "ProjectID=@projectId;";

                    // Assignees are sent as an list of string objects
                    // we have to create a single string to save it in the database.
                    StringBuilder assigneesString = new StringBuilder();

                    if (project.Assignees != null && project.Assignees.Count > 1)
                    {
                        foreach (var assignee in project.Assignees)
                        {
                            // append the value with a comma for the string bulder.
                            // this comma can be used to detect and create the list string back from the string.
                            assigneesString.Append(assignee + ",");
                        }
                    }
                    else
                    {
                        // of there is no more than 1 value, there is no need to seperate. Just append it to the builder.
                        // of the count not one, it means, it dont have any values, in that case, append the string ans an empty value.
                        if (project.Assignees.Count == 1)
                            assigneesString.Append(project.Assignees.ToArray()[0]);
                        else
                            assigneesString.Append("");
                    }

                    // set the parameters
                    CMDSaveProject.Parameters.AddWithValue("@projectId", project.ProjectId);
                    CMDSaveProject.Parameters.AddWithValue("@createdOn", project.CreatedOn.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@createdBy", project.CreatedBy);
                    CMDSaveProject.Parameters.AddWithValue("@title", project.Title);
                    CMDSaveProject.Parameters.AddWithValue("@projectManager", project.ProjectManager);
                    CMDSaveProject.Parameters.AddWithValue("@members", assigneesString.ToString());
                    CMDSaveProject.Parameters.AddWithValue("@description", project.Description);
                    CMDSaveProject.Parameters.AddWithValue("@category", project.Category);
                    CMDSaveProject.Parameters.AddWithValue("@startDate", project.StartDate.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@endDate", project.EndDate.ToString("G"));
                    CMDSaveProject.Parameters.AddWithValue("@status", project.Status);

                    // set the connection for the command
                    CMDSaveProject.Connection = con;

                    // extecute the command aget the affected row count
                    var affRows = CMDSaveProject.ExecuteNonQuery();

                    // if the affected rows count is larger than 0 that means it is successfully recorded in the database
                    // if not that means there is ban isuue occured.
                    if (affRows > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Log($"Completedly Updated project {project.Title}");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Log($"Failed to update project {project.Title}");
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    // incase of an error, show the error on the server console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }

            // Continuing to gete means abouve functions must have generates and uncaought error
            // this can be an something to dfo with input data
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Something Went Wrong");
            return false;

        }

        public bool DeleteProject(string projectID, string username)
        {
            // Update the serve console
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log($"Delete Project Invoked by {projectID}");


            // Create the SQLite Connection
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    // open the connection
                    con.Open();

                    // Create the SQLite Command
                    Log("Deleting Project");
                    SQLiteCommand CMDSaveProject = new SQLiteCommand();
                    CMDSaveProject.CommandText =
                        "DELETE FROM project " +
                        "WHERE " +
                            "ProjectID=@projectId;";

                    // set the parameters
                    CMDSaveProject.Parameters.AddWithValue("@projectId", projectID);

                    // set the connection for the command
                    CMDSaveProject.Connection = con;

                    // extecute the command aget the affected row count
                    var affRows = CMDSaveProject.ExecuteNonQuery();

                    // if the affected rows count is larger than 0 that means it is successfully recorded in the database
                    // if not that means there is ban isuue occured.
                    if (affRows > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Log($"Completedly Deleted Project {projectID}");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Log($"Failed to delete project {projectID}");
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    // incase of an error, show the error on the server console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }

            // Continuing to gete means abouve functions must have generates and uncaought error
            // this can be an something to dfo with input data
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Something Went Wrong");
            return false;
        }

        public List<Project> FetchAllProjects(string username)
        {
            // store the projects temporary
            List<Project> allProjects = new List<Project>();

            // Update the serve console
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log($"Fetch Projects Invoked by {username}");


            // Create the SQLite Connection
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    // open the connection
                    con.Open();

                    // Create the SQLite Command
                    Log("Fetching Projects");
                    SQLiteCommand CMDSaveProject = new SQLiteCommand();
                    CMDSaveProject.CommandText =
                        "SELECT " +
                        "ProjectID, CreatedOn, CreatedBy, Title, ProjectManager, Members, Description, Category, StartDate, EndDate, Status " +
                        "FROM project WHERE CreatedBy=@username OR ProjectManager=@username OR (Members LIKE @lusername);";
                    
                    CMDSaveProject.Parameters.AddWithValue("@username", username);
                    CMDSaveProject.Parameters.AddWithValue("@lusername", "%" + username + "%");
                    // set the connection for the command
                    CMDSaveProject.Connection = con;

                    // extecute the command aget the affected row count
                    var reader = CMDSaveProject.ExecuteReader();

                    // Colelct the data
                    while (reader.Read())
                    {
                        var assigneesString = reader.GetString(5);
                        var assignees = assigneesString.Substring(0, assigneesString.Length-1).Split(',');

                        allProjects.Add(new Project()
                        {
                            ProjectId = reader.GetString(0),
                            CreatedOn = DateTime.Parse(reader.GetString(1)),
                            CreatedBy = reader.GetString(2),
                            Title = reader.GetString(3),
                            ProjectManager = reader.GetString(4),
                            Assignees = assignees.ToList(),
                            Description = reader.GetString(6),
                            Category = reader.GetString(7),
                            StartDate = DateTime.Parse(reader.GetString(8)),
                            EndDate = DateTime.Parse(reader.GetString(9)),
                            Status = reader.GetString(10)
                        });
                    }

                    Log($"Found {allProjects.Count()} Projects in the organization");
                    return allProjects;
                }
                catch (Exception ex)
                {
                    // incase of an error, show the error on the server console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }

            // Continuing to gete means abouve functions must have generates and uncaought error
            // this can be an something to dfo with input data
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Something Went Wrong");
            return null;
        }

        public List<Project> SyncAllProjects(string username)
        {
            // store the projects temporary
            List<Project> allProjects = new List<Project>();

            // Update the serve console
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log($"Project Sync Invoked by {username}");


            // Create the SQLite Connection
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DBName}; Version=3;"))
            {
                try
                {
                    // open the connection
                    con.Open();

                    // Create the SQLite Command
                    SQLiteCommand CMDSaveProject = new SQLiteCommand();
                    CMDSaveProject.CommandText =
                        "SELECT " +
                        "ProjectID, CreatedOn, CreatedBy, Title, ProjectManager, Members, Description, Category, StartDate, EndDate, Status " +
                        "FROM project WHERE CreatedBy=@username OR ProjectManager=@username OR Members LIKE @lusername;";

                    CMDSaveProject.Parameters.AddWithValue("@username", username);
                    CMDSaveProject.Parameters.AddWithValue("@lusername", "%" + username + "%");

                    // set the connection for the command
                    CMDSaveProject.Connection = con;

                    // extecute the command aget the affected row count
                    var reader = CMDSaveProject.ExecuteReader();

                    // Colelct the data
                    while (reader.Read())
                    {
                        var assigneesString = reader.GetString(5);
                        var assignees = assigneesString.Substring(0, assigneesString.Length - 1).Split(',');

                        allProjects.Add(new Project()
                        {
                            ProjectId = reader.GetString(0),
                            CreatedOn = DateTime.Parse(reader.GetString(1)),
                            CreatedBy = reader.GetString(2),
                            Title = reader.GetString(3),
                            ProjectManager = reader.GetString(4),
                            Assignees = assignees.ToList(),
                            Description = reader.GetString(6),
                            Category = reader.GetString(7),
                            StartDate = DateTime.Parse(reader.GetString(8)),
                            EndDate = DateTime.Parse(reader.GetString(9)),
                            Status = reader.GetString(10)
                        });
                    }

                    Log($"Found {allProjects.Count()} Projects in the organization");
                    return allProjects;
                }
                catch (Exception ex)
                {
                    // incase of an error, show the error on the server console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }

            // Continuing to gete means abouve functions must have generates and uncaought error
            // this can be an something to dfo with input data
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Something Went Wrong");
            return null;
        }
    }
}

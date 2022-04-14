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
        private static string pathTODB = "";
        public static void Log(string v)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("G")}] >> {v}");
            Console.ResetColor();
        }

        public void IntializeDatabaseService()
        {
            //Directory.CreateDirectory("ProfilePics");
            var dbFile = File.Open(DBName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ProjectService)).CodeBase);
            pathTODB = Path.Combine(path, DBName);
            Log("Database Path Set to " + pathTODB);

            dbFile.Close();
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
                        "MentionedUsers TEXT ); ";

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

        }

        public List<Message> FindDirectMessagesFor(string loggedUser, Message lastMessage)
        {
            Log($"Requested Direct messages For {loggedUser}");

            Log("Generating Dummy Messages");
            var messages = new List<Message>();
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "Test From server 1", receiver = "Amoeher", sender = "Server", Time = DateTime.Now });
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "If received, Server is healthy. and runnding on test mode. 😀😁", receiver = "Amoeher", sender = "Server", Time = DateTime.Now });
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "This is a personal message for LiliKi From the server 😉", receiver = "LiliKi", sender = "Server", Time = DateTime.Now });

            var allMessagesForUser = messages.Where(message => message.sender == loggedUser || message.receiver == loggedUser).ToList();
            var newMessages = new List<Message>();
            if (lastMessage != null)
            {
                Log($"Filtering Messages From {lastMessage.Time.ToString("g")}");
                foreach (var message in allMessagesForUser)
                {
                    if (message.Time >= lastMessage.Time)
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
                        else
                        {
                            Log($"Found {message.MentionedUsers.Count} Mentioned Users");
                            command.Parameters.AddWithValue("@mentionedUsers", message.MentionedUsers.ToArray()[0]);
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@mentionedUsers", "");
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Log("Successfully saved the message");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log(ex.ToString());
                }
            }
            return false;
        }
    }
}

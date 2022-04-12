using Microsoft.Data.Sqlite;
using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PCClient.Server
{
    public class PMServer2
    {
        public static String DBName { get; set; } = "PMService2DB.db";

        public async static void IntializeDatabaseService1()
        {
            try
            {
                await ApplicationData.Current.LocalFolder.CreateFileAsync(DBName, CreationCollisionOption.OpenIfExists);
                string pathTODB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathTODB}"))
                {
                    con.Open();
                    string dbScript = "CREATE TABLE IF NOT EXISTS " +
                    "message (" +
                        "Email TEXT, " +
                        "Name TEXT, " +
                        "ImagePath TEXT, " +
                        "Password TEXT ); ";

                    SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                    sqliteCommand.ExecuteNonQuery();

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString(), "ERROR");
            }

        }

        public static List<Message> FindDirectMessagesFor(User loggedUser, Message lastMessage)
        {
            Debug.WriteLine($"Fetching Direct messages For {loggedUser.Name}");

            var messages = new List<Message>();
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "Test From server 1", receiver = "Amoeher", sender = "Server", Time = DateTime.Now });
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "If received, Server is healthy. and runnding on test mode. 😀😁", receiver = "Amoeher", sender = "Server", Time = DateTime.Now });
            messages.Add(new Message() { isSticker = false, MentionedUsers = null, MessageContent = "This is a personal message for LiliKi From the server 😉", receiver = "LiliKi", sender = "Server", Time = DateTime.Now });

            var allMessagesForUser = messages.Where(message => message.sender == loggedUser.Name || message.receiver == loggedUser.Name).ToList();
            var newMessages = new List<Message>();
            if (lastMessage != null)
            {
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

            return newMessages;

            // Last message

            // if the last message is null, Send the whole list.
            // if the last message is not null, send the messages with a larger timestamp. 

        }


        public static bool NewMessage(Message message)
        {

            return false;
        }

    }
}

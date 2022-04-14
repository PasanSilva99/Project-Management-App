using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projent.Model
{
    public class Converter
    {
        public static DataStore.Status ToClientStatus(PMServer1.Status sstatus)
        {

            switch (sstatus)
            {
                case PMServer1.Status.Busy: return DataStore.Status.Busy;
                case PMServer1.Status.Invisible: return DataStore.Status.Invisible;
                case PMServer1.Status.Idle: return DataStore.Status.Idle;
                case PMServer1.Status.Online: return DataStore.Status.Online;
                default: return DataStore.Status.Offline; // if none other its offline

            }

        }

        public static PMServer1.Status ToServerStatus(DataStore.Status? sstatus)
        {

            switch (sstatus)
            {
                case DataStore.Status.Busy: return PMServer1.Status.Busy;
                case DataStore.Status.Invisible: return PMServer1.Status.Invisible;
                case DataStore.Status.Idle: return PMServer1.Status.Idle;
                case DataStore.Status.Online: return PMServer1.Status.Online;
                default: return PMServer1.Status.Offline; // if none other its offline 
            }

        }

        public static PMServer1.User ToServerUser(User user)
        {
            if (user != null)
            {
                return new PMServer1.User() { Email = user.Email, Name = user.Name, Password = user.Password };
            }
            return null;
        }

        public static User ToLocalUser(PMServer1.User suser)
        {
            if (suser != null)
            {
                return new User() { Email = suser.Email, Name = suser.Name, Password = suser.Password };
            }
            return null;
        }

        public static PMServer2.Message ToServerMessage(Message message)
        {
            return new PMServer2.Message() {
                isSticker = message.isSticker,
                MentionedUsers = new System.Collections.ObjectModel.ObservableCollection<string>(message.MentionedUsers),
                MessageContent = message.MessageContent,
                receiver = message.receiver,
                sender = message.sender,
                Time = message.Time};
        }

        public static Message ToLocalMessage(PMServer2.Message smessage)
        {
            return new Message()
            {
                isSticker = smessage.isSticker,
                MessageContent = smessage.MessageContent,
                MentionedUsers = smessage.MentionedUsers.ToList(),
                receiver = smessage.receiver,
                sender = smessage.sender,
                Time = smessage.Time
            };
        }

        public static List<Message> GetLocalMessageList(List<PMServer2.Message> messages)
        {
            List<Message> messagesList = new List<Message>();
            foreach (PMServer2.Message message in messages)
            {
                messagesList.Add(ToLocalMessage(message));
            }
            return messagesList;
        }

    }
}

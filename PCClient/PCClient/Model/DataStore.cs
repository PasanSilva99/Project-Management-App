using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PCClient.Model
{
    public class DataStore
    {
        public static String ApplicationName_Acronym { get; set; } = "Projent";
        public static String ApplicationName_full { get; set; } = "Collaborative Project Mnagement Platform";

        public static bool CheckConnectivity()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async static void IntializeLocalDatabase()
        {

        }

        public async static void SyncServerDatabase()
        {

        }

        public static bool ValidateUser(string email, string password)
        {
            return false;
        }

        public static User FindUser(string email, string password)
        {
            return null;
        }
    }
}

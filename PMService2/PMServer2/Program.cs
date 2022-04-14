using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PMServer2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(PMService2.ProjectService)))
            {
                ShowStartScreen();
                host.Open();
                Log("Project Server Started");
                PMService2.ProjectService mainService = new PMService2.ProjectService();
                mainService.IntializeDatabaseService();

                Console.ReadLine();
            }
        }

        public static void Log(string v)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("G")}] >> {v}");
        }
        static void ShowStartScreen()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("" +
                "===================================================================\n" +
                "||  ██████╗ ██████╗  ██████╗      ██╗███████╗███╗   ██╗████████╗ ||\n" +
                "||  ██╔══██╗██╔══██╗██╔═══██╗     ██║██╔════╝████╗  ██║╚══██╔══╝ ||\n" +
                "||  ██████╔╝██████╔╝██║   ██║     ██║█████╗  ██╔██╗ ██║   ██║    ||\n" +
                "||  ██╔═══╝ ██╔══██╗██║   ██║██   ██║██╔══╝  ██║╚██╗██║   ██║    ||\n" +
                "||  ██║     ██║  ██║╚██████╔╝╚█████╔╝███████╗██║ ╚████║   ██║    ||\n" +
                "||  ╚═╝     ╚═╝  ╚═╝ ╚═════╝  ╚════╝ ╚══════╝╚═╝  ╚═══╝   ╚═╝    ||\n" +
                "||                                                               ||\n" +
                "||     █▀█ █▀█ █▀█ ░░█ █▀▀ █▀▀ ▀█▀   █▀ █▀▀ █▀█ █░█ █▀▀ █▀█      ||\n"+
                "||     █▀▀ █▀▄ █▄█ █▄█ ██▄ █▄▄ ░█░   ▄█ ██▄ █▀▄ ▀▄▀ ██▄ █▀▄      ||\n"+
                "===================================================================\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(":::Project Server:::");

            Console.ResetColor();

        }
    }
}

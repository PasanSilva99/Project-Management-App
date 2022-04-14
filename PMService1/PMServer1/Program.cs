using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PMServer1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(PMService1.MainService)))
            {
                ShowStartScreen();

                host.Open();
                Log("Main Server Started");
                PMService1.MainService mainService = new PMService1.MainService();
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
                "||           █▀▄▀█ ▄▀█ █ █▄░█   █▀ █▀▀ █▀█ █░█ █▀▀ █▀█           ||\n" +
                "||           █░▀░█ █▀█ █ █░▀█   ▄█ ██▄ █▀▄ ▀▄▀ ██▄ █▀▄           ||\n" +
                "===================================================================\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(":::Main Server:::");

            Console.ResetColor();

        }
    }
}

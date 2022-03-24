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
                host.Open();
                Console.WriteLine("Server 1 Started");

                Console.ReadLine();
            }
        }
    }
}

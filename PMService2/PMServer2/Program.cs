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
                host.Open();
                Console.WriteLine("Project Server Started");

                Console.ReadLine();
            }
        }
    }
}

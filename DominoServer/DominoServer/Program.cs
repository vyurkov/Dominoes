using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace DominoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //ServiceHost service = new ServiceHost(typeof(Dominoes), new Uri("net.tcp://10.6.0.105:7557/Dominoes"));
                ServiceHost service = new ServiceHost(typeof(Dominoes), new Uri("net.tcp://localhost:7557/Dominoes"));
                ServiceMetadataBehavior mtdb = new ServiceMetadataBehavior();
                service.Description.Behaviors.Add(mtdb);
                service.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "Mex");
                service.AddServiceEndpoint(typeof(IDominoes), new NetTcpBinding(), "");
                service.Open();
                Console.Write("Для завершения нажмите Enter");
                Console.ReadLine();
                service.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

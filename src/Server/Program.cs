using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Launcher.Server;

namespace LauncherHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = new Uri("net.tcp://0.0.0.0:8000/LauncherService");
            using (ServiceHost host = new ServiceHost(typeof(LauncherService), baseAddress))
            {
                var binding = new NetTcpBinding
                {
                    MaxReceivedMessageSize = 20 * 1024 * 1024,
                    Security = { Mode = SecurityMode.None }
                };
                host.AddServiceEndpoint(typeof(ILauncherService), binding, "");
                var smb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (smb == null)
                {
                    host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
                }
                Console.WriteLine("Запуск сервиса LauncherService на " + baseAddress);
                host.Open();
                Console.WriteLine("Нажмите Enter для остановки...");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}

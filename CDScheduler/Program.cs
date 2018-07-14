using DasMulli.Win32.ServiceUtils;
using System;

namespace CDScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var monitorService = new MonitorService();
            var serviceHost = new Win32ServiceHost(monitorService);
            serviceHost.Run();
        }
    }
    class MonitorService : IWin32Service
    {
        public string ServiceName => "Monitor Service";

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            // Start coolness and return
        }

        public void Stop()
        {
            // shut it down again
        }
    }
}

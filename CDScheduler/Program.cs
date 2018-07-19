using Chroniton;
using Chroniton.Jobs;
using Chroniton.Schedules;
using DasMulli.Win32.ServiceUtils;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

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
    public class MonitorService : IWin32Service
    {
        public string ServiceName => "Monitor Service";
        private Settings settings;
        private string service_content_path = "C:/MonitorService/";
        private Singularity singularity;

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            ImportSettings();
            singularity = Singularity.Instance;
            var job = new SimpleJob((scheduledTime) =>
                GetAndCompareCommits());
            var schedule = new EveryXTimeSchedule(TimeSpan.FromSeconds(12));
            var scheduledJob = singularity.ScheduleJob(schedule, job, true);

            singularity.Start();
            
        }
        
        public void ImportSettings()
        {
            try
            {
                string json = File.ReadAllText(service_content_path + "config.json");
                settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
                if (!File.Exists(service_content_path + "last_commit"))
                {
                    File.CreateText(service_content_path + "last_commit");
                    File.WriteAllText(service_content_path + "create.bat", "cmd /c dotnet pack " + settings.csproj_location);
                }
            }
            catch(Exception e)
            {
                if (!File.Exists(service_content_path + "log.txt"))
                    File.CreateText(service_content_path + "log.txt");
                File.AppendAllText(service_content_path + "log.txt", 
                     DateTime.Now + "\tError occurred while importing settings: " + e.Message + "\n");
            }            
        }

        public void GetAndCompareCommits()
        {
                               
            Repository repo = new Repository(settings.repo);
            System.Collections.Generic.IEnumerator<Commit> enumerator = repo.Commits.GetEnumerator();
            enumerator.MoveNext();
            DateTime lastUpdated = enumerator.Current.Committer.When.ToUniversalTime().DateTime;
            try
            {
                if (File.ReadAllText(service_content_path + "last_commit").Length == 0)
                {
                    File.AppendAllText(service_content_path + "last_commit",
                        lastUpdated.ToString());

                }
                else
                {
                    if (File.ReadAllText(service_content_path + "last_commit").Equals(lastUpdated))
                    {
                        if (!File.Exists(service_content_path + "log.txt"))
                            File.CreateText(service_content_path + "log.txt");
                        File.AppendAllText(service_content_path + "log.txt",
                             DateTime.Now + "\tAlready updated: " + "\n");
                        return;
                    }
                    File.WriteAllText(service_content_path + "last_commit",
                        lastUpdated.ToString());
                    CreatePackage();
                }
                }catch(Exception e)
                {
                    if (!File.Exists(service_content_path + "log.txt"))
                        File.CreateText(service_content_path + "log.txt");
                    File.AppendAllText(service_content_path + "log.txt",
                         DateTime.Now + "\tError occurred while updating last commit: " + e.Message + "\n");
                }
           
        }

        public void CreatePackage()
        {
            string package_id = GetIdFromCsproj();
            Process.Start(service_content_path + "create.bat");
            Thread.Sleep(1000);
            PushPackage(package_id);
        }

        private void PushPackage(string package_id)
        {
            string push_command = "cmd /c nuget push " + settings.package_output + package_id+".nupkg " + settings.api_password + " -Source " +settings.push_location;
            File.AppendAllText(service_content_path + "log.txt",
                         DateTime.Now + "\tPush command: "+ push_command + "\n");
            File.WriteAllText(service_content_path + "push.bat", push_command);
            Thread.Sleep(200);
            Process.Start(service_content_path + "push.bat");
        }

        public string GetIdFromCsproj()
        {
            string xml = File.ReadAllText(settings.csproj_location);
            XDocument xDocument = XDocument.Parse(xml);
            return xDocument.Root.Elements("PropertyGroup").Elements("id").First().Value + "."
                + xDocument.Root.Elements("PropertyGroup").Elements("version").First().Value;
        }

        public void Stop()
        {
            // shut it down again
            if(singularity!=null)
                singularity.Stop();
        }
    }
}

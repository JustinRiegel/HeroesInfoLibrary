using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using HeroesInfoLibrary.Models;
using System.Threading;

namespace HeroesInfoLibrary
{
    public partial class HeroesInfoBotService : ServiceBase
    {
        #region Service enum and struct

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        #endregion

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private int eventId = 1;

        public HeroesInfoBotService(string[] args)
        {
            InitializeComponent();
            
            string eventSourceName = "HeroesInfoBotSource";
            string logName = "HeroesInfoBotLog";

            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            eventLog = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog.Source = eventSourceName;
            eventLog.Log = logName;

            //eventLog.WriteEntry("End of constructor.");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("In OnStart");

            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //var context = new HeroDataDbContext();
            //eventLog.WriteEntry("Creating database for Heroes info...");

            //HeroesInfoBotContextSeeder.SetUpHeroDataList(context);

            eventLog.WriteEntry("Database created. Hopefully.");

            // Set up a timer to trigger every minute.  
            //System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Interval = 60000; // 60 seconds  
            //timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            //timer.Start();

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("In OnStop.");

            //var dbFile = new FileInfo(@"D:\GitHub Repos\heroes-talents\hero");
            //dbFile.Delete();
            //
            //eventLog.WriteEntry("Database file deleted.");
        }

        protected override void OnContinue()
        {
            eventLog.WriteEntry("In OnContinue.");
        }

        
    }
}

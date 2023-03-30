using AudioSwitcher.AudioApi.CoreAudio;
using LibreHardwareMonitor.Hardware;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace WindowsService_HostAPI
{
    public class FloatFormatConverter : JsonConverter<float>
    {
        public override void Write(Utf8JsonWriter writer, float value,
                                        JsonSerializerOptions serializer)
        {
            writer.WriteNumberValue(Math.Round(value, 2, MidpointRounding.AwayFromZero));
        }

        public override float Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions serializer)
        {
            throw new NotImplementedException();
        }
    }


    partial class SelfHostService : ServiceBase
    {
        public const string INFO = "info";
        public const string WARNING = "warn";
        public const string ERROR = "error";

        public static EventLog EventLog1 { get; set; }

        public static int EventId { get; set; } = 1;

        public static Computer Computer { get; set; }

        public static CoreAudioController AudioController { get; set; }
        public SelfHostService()
        {
            InitializeComponent();
            EventLog1 = new EventLog();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource(
                     "MySource", "MyNewLog");
            }
            EventLog1.Source = "MySource";
            EventLog1.Log = "MyNewLog";
            Computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsStorageEnabled = true,
                IsMemoryEnabled = true,
            };

            AudioController = new CoreAudioController();

        }

        protected override void OnStart(string[] args)
        {
            LogWriter.LogWrite(LogWriter.INFO, "SelfHostService", "Starting service");
            EventLog1.WriteEntry("StartingService");
            try
            {
                var config = new HttpSelfHostConfiguration("http://localhost:8080");
                config.MapHttpAttributeRoutes();

                // config.Routes.MapHttpRoute(
                //    name: "API",
                //    routeTemplate: "{controller}/{action}",
                //    defaults: new {  }
                //);
                HttpSelfHostServer server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ERROR, "SelfHostService", "!!!!!!! OnStart 1 - " + ex.ToString());
                EventLog1.WriteEntry(ex.Message, EventLogEntryType.Error, 1, (short)21, ex.StackTrace.ToCharArray().Select(c => (byte)c).ToArray());
                this.Stop();
                return;
            }

            try
            {
                Computer.Open();
                //Multimedia.UpdateAll();
                Timer timer = new Timer { Interval = 1000 };
                timer.Elapsed += new ElapsedEventHandler(this.UpdateSensors);
                timer.Start();

                Timer timerMultimedia = new Timer { Interval = 60000 };
                timerMultimedia.Elapsed += new ElapsedEventHandler(this.UpdateMultimedia);
                timerMultimedia.Start();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ERROR, "SelfHostService", "!!!!!!! OnStart 2 - " + ex.ToString());
                EventLog1.WriteEntry(ex.Message, EventLogEntryType.Error, 1, (short)22, ex.StackTrace.ToCharArray().Select(c => (byte)c).ToArray());
                this.Stop();
                return;
            }
            //LogWriter.LogWrite("SelfHostService", "Service started successfull");
            //System.Diagnostics.Debugger.Launch();
            EventLog1.WriteEntry("Service started successfull");
        }
        protected void UpdateSensors(object sender, ElapsedEventArgs args)
        {
            Computer.Open();
            foreach (IHardware hw in Computer.Hardware)
            {
                hw.Update();
            }
        }
        protected void UpdateMultimedia(object sender, ElapsedEventArgs args)
        {
            //try
            //{
            //    audioController = new CoreAudioController();
            //} catch (Exception ex)
            //{

            //    LogWriter.LogWrite("!!!!!!! UpdateMultimedia - " + ex.ToString());
            //}
        }

        protected override void OnStop()
        {
            Computer.Close();
            EventLog1.WriteEntry("In OnStop.");
            //LogWriter.LogWrite(LogLevel.INFO, nameof(SelfHostService), "Service stoped successfull");
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
        protected override void OnContinue()
        {
            EventLog1.WriteEntry("In OnContinue.");
        }
    }
}

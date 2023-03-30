using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web.Http;

namespace WindowsService_HostAPI
{
    public class SensorCollectData
    {
        public String Name { get; set; }
        public String Sensor { get; set; }
        [JsonConverter(typeof(FloatFormatConverter))]
        private float _value;

        public float Value
        {
            get { return _value; }
            set { _value = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        public void SetValueFromString(float? value)
        {
            if (value != null)
            {
                this._value = (float)Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                this._value = 0;
            }
        }

    }
    public class HardwareCollectData
    {
        public String Hardware { get; set; }
        public String Name { get; set; }
        public List<SensorCollectData> Sensors { get; set; }
    }

    [RoutePrefix("Sensors")]
    public class SensorsController : ApiController
    {

        [HttpGet]
        [Route("short")]
        public List<HardwareCollectData> GetSensors()
        {
            SelfHostService.EventLog1.WriteEntry("Requested Short Info", EventLogEntryType.Information, SelfHostService.EventId);
            List<HardwareCollectData> datas = new List<HardwareCollectData>();
            try
            {
                foreach (IHardware hardware in SelfHostService.Computer.Hardware)
                {
                    HardwareCollectData hardwareData = new HardwareCollectData
                    {
                        Name = hardware.Name,
                        Sensors = new List<SensorCollectData>()
                    };
                    if (HardwareType.Cpu == hardware.HardwareType)
                    {
                        hardwareData.Hardware = "Cpu";
                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            if (SensorType.Load == sensor.SensorType && "CPU Total".Equals(sensor.Name) ||
                                SensorType.Temperature == sensor.SensorType && "Core Average".Equals(sensor.Name) ||
                                SensorType.Power == sensor.SensorType && "CPU Package".Equals(sensor.Name)
                                )
                            {
                                hardwareData.Sensors.Add(new SensorCollectData
                                {
                                    Name = sensor.Name,
                                    Sensor = sensor.SensorType.ToString(),
                                    Value = (float)sensor.Value,
                                });
                            }
                        }
                    }
                    else if (HardwareType.Memory == hardware.HardwareType)
                    {
                        hardwareData.Hardware = "Memory";
                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            if (SensorType.Load == sensor.SensorType && "Memory".Equals(sensor.Name) ||
                                SensorType.Data == sensor.SensorType && "Memory Available".Equals(sensor.Name) ||
                                SensorType.Data == sensor.SensorType && "Memory Used".Equals(sensor.Name)
                                )
                            {
                                hardwareData.Sensors.Add(new SensorCollectData
                                {
                                    Name = sensor.Name,
                                    Sensor = sensor.SensorType.ToString(),
                                    Value = (float)sensor.Value,
                                });
                            }
                        }
                    }
                    else if (HardwareType.GpuNvidia == hardware.HardwareType)
                    {
                        hardwareData.Hardware = "Gpu";
                        //hardware.Update();
                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            if (SensorType.Temperature == sensor.SensorType && "GPU Core".Equals(sensor.Name) ||
                                SensorType.Load == sensor.SensorType && "GPU Core".Equals(sensor.Name) ||
                                SensorType.Power == sensor.SensorType && "GPU Package".Equals(sensor.Name)
                                )
                            {
                                hardwareData.Sensors.Add(new SensorCollectData()
                                {
                                    Name = sensor.SensorType.ToString(),
                                    Sensor = sensor.SensorType.ToString(),
                                    Value = (float)sensor.Value,
                                });
                            }
                        }
                    }
                    else if (HardwareType.Storage == hardware.HardwareType && (hardware is NVMeGeneric))
                    {
                        NVMeGeneric nVMe = (NVMeGeneric)hardware;
                        hardwareData.Hardware = "NVMe";
                        nVMe.Update();
                        foreach (ISensor sensor in nVMe.Sensors)
                        {
                            if (SensorType.Load == sensor.SensorType && "Total Activity".Equals(sensor.Name) ||
                                SensorType.Load == sensor.SensorType && "Used Space".Equals(sensor.Name) ||
                                SensorType.Temperature == sensor.SensorType && "Temperature".Equals(sensor.Name)
                                )
                            {
                                hardwareData.Sensors.Add(new SensorCollectData
                                {
                                    Name = sensor.Name,
                                    Sensor = sensor.SensorType.ToString(),
                                    Value = (float)sensor.Value,
                                });
                            }
                        }
                    }
                    if (hardwareData.Hardware != null)
                    {
                        datas.Add(hardwareData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(LogWriter.ERROR, nameof(SensorsController), ex.ToString());
                SelfHostService.EventLog1.WriteEntry(ex.Message, EventLogEntryType.Error, SelfHostService.EventId, (short)40, ex.StackTrace.ToCharArray().Select(c => (byte)c).ToArray());
            }
            finally
            {
                SelfHostService.EventId++;
            }

            return datas;
        }





        [HttpGet]
        [Route("full")]
        public List<HardwareCollectData> GetFullSensors()
        {
            SelfHostService.EventLog1.WriteEntry("Requested Full Info", EventLogEntryType.Information, SelfHostService.EventId);
            List<HardwareCollectData> datas = new List<HardwareCollectData>();
            try
            {
                foreach (IHardware hardware in SelfHostService.Computer.Hardware)
                {
                    HardwareCollectData hardwareData = new HardwareCollectData
                    {
                        Name = hardware.Name,
                        Sensors = new List<SensorCollectData>(),
                        Hardware = hardware.HardwareType.ToString()
                    };
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        hardwareData.Sensors.Add(new SensorCollectData
                        {
                            Name = sensor.Name,
                            Sensor = sensor.SensorType.ToString(),
                            Value = (float)sensor.Value
                        });
                    }
                    if (hardwareData.Hardware != null)
                    {
                        datas.Add(hardwareData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(LogWriter.ERROR, nameof(SensorsController), ex.ToString());
                SelfHostService.EventLog1.WriteEntry(ex.Message, EventLogEntryType.Error, SelfHostService.EventId, (short)41, ex.StackTrace.ToCharArray().Select(c => (byte)c).ToArray());
            }
            finally
            {
                SelfHostService.EventId++;
            }
            return datas;
        }
    }

}
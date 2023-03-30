using AudioSwitcher.AudioApi;
using HidSharp;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using VisioForge.Libs.NAudio.CoreAudioApi;

namespace WindowsService_HostAPI
{
    [RoutePrefix("Multimedia")]
    public class MultimediaController : ApiController
    {
        /// <summary>
        /// Return status of default input device.
        /// </summary> 
        /// <returns> 
        /// State: 1 - worked, 0 - muted or didn't exists.
        /// Result : true - if  device exists, false - error or device didn't exists
        /// </returns>
        [HttpGet]
        [Route("getMicState")]
        public Object GetSensors()
        {
            try
            {
                var device = SelfHostService.AudioController.GetDevices(DeviceType.Capture, AudioSwitcher.AudioApi.DeviceState.Active).FirstOrDefault(x => x.IsDefaultDevice);
                return new
                {
                    State = device != null && !device.IsMuted ? 1 : 0,
                    Result = device != null,
                    WithErrors = false
                };
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(LogWriter.ERROR, nameof(MultimediaController), ex.ToString());
                return new
                {
                    State = -1,
                    Result = false,
                    WithErrors = true
                };
            }
        }



        /// <summary>
        /// Set state for default input device.
        /// </summary> 
        /// <param name="state">1 - worked, 0 - muted.</param>
        /// <returns> 
        /// State: 1 - worked, 0 - muted or didn't exists.
        /// Result : true - if  device exists, false - error or device didn't exists
        /// </returns>
        //[HttpGet]
        //[HttpPost]
        [HttpGet, Route("changeMicState2")]
        public async Task<object> ChengeInputDeviceState2Async([FromUri] int state)
        {
            bool finishedWithError = false;
            bool result = false;
            int resultState = 0;
            try
            {

                var device = SelfHostService.AudioController.GetDevices(DeviceType.Capture, AudioSwitcher.AudioApi.DeviceState.Active).FirstOrDefault(x => x.IsDefaultDevice);
                result = device != null;
                var awaitResult = await device.SetMuteAsync(state != 1);
                resultState = result && awaitResult ? 1 : 0;
                if (resultState != state && result)
                {
                    finishedWithError = true;
                    LogWriter.LogWrite(LogWriter.WARNING, nameof(MultimediaController), "[Warning] Microphone state didn't changed");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(LogWriter.ERROR, nameof(MultimediaController), ex.ToString());
                finishedWithError = true;
            }
            return new
            {
                State = resultState,
                Result = result,
                WithErrors = finishedWithError
            };
        }



        /// <summary>
        /// Set state for default input device.
        /// </summary> 
        /// <param name="state">1 - worked, 0 - muted.</param>
        /// <returns> 
        /// State: 1 - worked, 0 - muted or didn't exists.
        /// Result : true - if  device exists, false - error or device didn't exists
        /// </returns>
        //[HttpGet]
        //[HttpPost]
        [HttpGet, Route("changeMicState")]
        public object ChengeInputDeviceState([FromUri] int state)
        {
            bool finishedWithError = false;
            bool result = false;
            int resultState = 0;
            try
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, VisioForge.Libs.NAudio.CoreAudioApi.Role.Communications);
                result = device != null;
                device.AudioEndpointVolume.Mute = state != 1;

                //var device = SelfHostService.AudioController.GetDevices(DeviceType.Capture, DeviceState.Active).FirstOrDefault(x => x.IsDefaultDevice);
                //result = device != null;
                //var awaitResult = await device.SetMuteAsync(state != 1);
                resultState = result && !device.AudioEndpointVolume.Mute ? 1 : 0;
                if (resultState != state && result)
                {
                    finishedWithError = true;
                    LogWriter.LogWrite(LogWriter.WARNING, nameof(MultimediaController), "[Warning] Microphone state didn't changed");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(LogWriter.ERROR, nameof(MultimediaController), ex.ToString());
                finishedWithError = true;
            }
            return new
            {
                State = resultState,
                Result = result,
                WithErrors = finishedWithError
            };
        }
    }

}
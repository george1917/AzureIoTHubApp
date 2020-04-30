using Microsoft.Azure.Devices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NTX.SysCore;
using NTX.SysCore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    //Microsoft.Azure.Devices

    public class HomeController : Controller
    {
        private static ServiceClient s_serviceClient;
        private readonly static string s_connectionString = "IoT Hub Connection String";

        [HttpPost]
        public async Task<string> GetWeightData()
        {
            s_serviceClient = ServiceClient.CreateFromConnectionString(s_connectionString);
            string json = await InvokeMethod();
            var deviceData = JsonSerializer.Deserialize<DeviceData>(json);
            return string.Format(@"Weight：{0}", deviceData.Weight);
        }

        private class DeviceData
        {
            [JsonPropertyName("weight")]
            public decimal? Weight { get; set; }
        }

        private async Task<string> InvokeMethod()
        {
            //Web call device method
            var methodInvocation = new CloudToDeviceMethod("GetWeight") { ResponseTimeout = TimeSpan.FromSeconds(30) };
            methodInvocation.SetPayloadJson("10");

            // Invoke the direct method asynchronously and get the response from the simulated device.
            var response = await s_serviceClient.InvokeDeviceMethodAsync("Device ID", methodInvocation);

            return response.GetPayloadAsJson();
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
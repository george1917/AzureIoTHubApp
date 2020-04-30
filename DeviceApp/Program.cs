using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceApp
{
    //Microsoft.Azure.Devices.Client

    class Program
    {
        private static string s_deviceConnectionString = "Device Connection String";
        private static TransportType s_transportType = TransportType.Mqtt;
        static CancellationTokenSource tokenSource = new CancellationTokenSource();
        static readonly TaskScheduler _syncContextTaskScheduler = TaskScheduler.Current;
        static int execCount = 0;

        public static async Task Main(string[] args)
        {
            if (string.IsNullOrEmpty(s_deviceConnectionString) && args.Length > 0)
            {
                s_deviceConnectionString = args[0];
            }

            using (var deviceClient = DeviceClient.CreateFromConnectionString(s_deviceConnectionString, s_transportType))
            {
                _deviceClient = deviceClient;
                await RunSampleAsync().ConfigureAwait(false);

                GetWeight();
                Console.Read();
            }
        }

        #region Device get scale weight
        private static void GetWeight()
        {
            CancellationToken token = tokenSource.Token;

            Task task = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Random rnd = new Random();
                    int a = rnd.Next(10000);
                    decimal f = (decimal)(a * 0.01);

                    Task.Factory.StartNew(() => pbBarFun(f), new CancellationTokenSource().Token, TaskCreationOptions.None, _syncContextTaskScheduler).Wait();

                    Thread.Sleep(5000);
                }
            }, token);
        } 
         
        private static void pbBarFun(decimal rFlag)
        {
            decimal preValue = rFlag - 1;
            decimal nextValue = rFlag + 1;
            CurrentWeight = rFlag;

            execCount++;
            Console.WriteLine("No. {0} :", execCount);
            Console.WriteLine("  -> {0}", preValue);
            Console.WriteLine("  -> {0}", nextValue);
            Console.WriteLine("  -> {0}", rFlag);
            Console.WriteLine("--------------------");
        } 
         
        private static decimal? _CurrentWeight;
        public static decimal? CurrentWeight
        {
            get { return _CurrentWeight; }
            set
            {
                _CurrentWeight = value;
            }
        }
        #endregion

        #region Web call device method 
        private static DeviceClient _deviceClient;
        private class DeviceData
        {
            [JsonPropertyName("weight")]
            public decimal? Weight { get; set; }
        }

        /// <summary>
        /// Setup a callback for the 'GetWeight' method.
        /// </summary> 
        public static async Task RunSampleAsync()
        {
            DeviceData weight = null;
            await _deviceClient
                .SetMethodHandlerAsync("GetWeight", GetWeightAsync, weight)
                .ConfigureAwait(false);
        }

        private static Task<MethodResponse> GetWeightAsync(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t *** {methodRequest.Name} was called.");

            userContext = new DeviceData() { Weight = CurrentWeight };
            string result = JsonSerializer.Serialize(userContext);
            MethodResponse retValue = new MethodResponse(Encoding.UTF8.GetBytes(result), 200);

            return Task.FromResult(retValue);
        }
        #endregion

    }
}

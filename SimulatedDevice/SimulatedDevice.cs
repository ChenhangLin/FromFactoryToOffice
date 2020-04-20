// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace simulated_device
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private readonly static string s_connectionString = "{YourDeviceConnectionString}";

        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
			double accuracyOffset = 0;
            Random rand = new Random();
			
			int productCounter = 1;

            while (productCounter <= 500)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;
				double accuracyChecking = accuracyOffset + rand.NextDouble();
				
                // Create JSON message
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity,
					accuracyCalibration = accuracyChecking,
					productTotal = productCounter ++,
					anomaly = ((currentTemperature > 34) ? 1 : 0)
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                //message.Properties.Add("anomaly", (currentTemperature > 30) ? "true" : "false");
				
				//Add Proper Content Type and Encoding to avoid message body being encrypted
				message.ContentEncoding = "utf-8";
				message.ContentType = "application/json";

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending real-time data: {1}", DateTime.Now, messageString);
				
				// Send at the frequency of 1 second
                await Task.Delay(1000);
            }
        }
        private static void Main(string[] args)
        {
            Console.WriteLine("Connected to demo factory device. Preparing to transmitt device data... \n");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }
    }
}

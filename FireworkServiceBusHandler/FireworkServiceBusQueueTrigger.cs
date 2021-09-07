using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FireworkServices.Context;
using FireworkServices.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ranger.FireworkHandler
{
    public class FireworkServiceBusQueueTrigger
    {

        private readonly TelemetryClient telemetryClient;

        public FireworkServiceBusQueueTrigger()
        {
            this.telemetryClient = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            this.telemetryClient.Context.Cloud.RoleName = "fireworkhandler";
        }

        [Function("FireworkServiceBusQueueTrigger")]
        public async Task RunAsync([ServiceBusTrigger("firework", Connection = "firework_SERVICEBUS")] string myQueueItem, FunctionContext context)
        {

            FireworkModel firework = JsonSerializer.Deserialize<FireworkModel>(myQueueItem);
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            try
            {
                using (FireworkContext db = new())
                {
                    await db.AddAsync(firework);
                    await db.SaveChangesAsync();
                }
                success = true;
            }
            catch (System.Exception)
            {
                timer.Stop();
                success = false;
                telemetryClient.TrackException(new TaskCanceledException("Error to add Data. Will try agian later."));
                throw new TaskCanceledException("Error to add Data. Will try agian later.");
            }
            finally
            {
                timer.Stop();
                success = true;
                telemetryClient.TrackDependency("SQL", "azurebasecamp | firework", $"Save: {firework.Name}", startTime, timer.Elapsed, success);
            }

            timer.Restart();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = new { name = firework.Name };
                    var content = new StringContent(
                        JsonSerializer.Serialize(data),
                        Encoding.UTF8,
                        "application/json"
                    );
                    var apiurl = Environment.GetEnvironmentVariable("WebAPIURL");
                    await client.PostAsync(
                        apiurl,
                        content
                    );
                }
            }
            finally
            {
                timer.Stop();
                success = true;
                telemetryClient.TrackDependency("SignalR", "Fire!!", $"Fire: {firework.Name}", startTime, timer.Elapsed, success);
            }

        }
    }
}

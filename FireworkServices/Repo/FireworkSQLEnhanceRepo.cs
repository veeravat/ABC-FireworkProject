using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FireworkServices.Context;
using FireworkServices.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace FireworkServices.Repo
{
    public class FireworkSQLEnhanceRepo : IFireworkRepo
    {

        private readonly FireworkContext db;
        private readonly IHubContext<FireworkSignalR> signalR;
        private readonly IDatabase redisCache;
        private readonly ServiceBusSender sb;
        private readonly TelemetryClient telemetry;
        public FireworkSQLEnhanceRepo(FireworkContext db, IHubContext<FireworkSignalR> signalR, IConnectionMultiplexer redisCache, ServiceBusSender sb, TelemetryClient telemetry)
        {
            this.db = db;
            this.sb = sb;
            this.telemetry = telemetry;
            this.signalR = signalR;
            this.redisCache = redisCache.GetDatabase();
        }

        public async Task<decimal> GetTotalFireworkAsync()
        {
            decimal fireworks = 0;
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                fireworks = Decimal.Parse((await redisCache.StringGetAsync("TotalFirework")).ToString());
                success = true;
            }
            catch (System.Exception)
            {
                try
                {
                    fireworks = await db.Fireworks.CountAsync();
                    success = true;
                }
                catch (System.Exception)
                {
                    success = false;
                    telemetry.TrackException(new DbUpdateConcurrencyException("Database is busy now. Try agian later"));
                    throw new DbUpdateConcurrencyException("Database is busy now. Try agian later");
                }
                await redisCache.StringSetAsync(
                    "TotalFirework",
                    fireworks.ToString(),
                    TimeSpan.FromMinutes(5)
                    );
                timer.Stop();
                telemetry.TrackDependency("Redis", "GetFirework", $"Loaded total firework from db", startTime, timer.Elapsed, success);
            }
            finally
            {
                timer.Stop();
                telemetry.TrackDependency("Redis", "GetFirework", $"Loaded total firework from cache", startTime, timer.Elapsed, success);
            }
            return fireworks;
        }

        public async Task SaveFireworkAsync(FireworkModel Fireworker)
        {
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            string jsonString = JsonSerializer.Serialize(Fireworker);
            ServiceBusMessage message = new(jsonString);

            try
            {
                await sb.SendMessageAsync(message);
                success = true;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(new Exception($"Fail to queue message:{ex.Message}"));
            }
            finally
            {
                timer.Stop();
                telemetry.TrackDependency("SERVICEBUS", "Message", $"Queue firework to service bus", startTime, timer.Elapsed, success);
            }
            // await db.Fireworks.AddAsync(Fireworker);
            // await db.SaveChangesAsync();
        }

        public async Task SentSignalrAsync(string Name)
        {
            await signalR.Clients.All.SendAsync("ReceiveMessage", Name, Name);
        }


        public string TryHash(String str)
        {
            SHA256 sha256 = SHA256.Create();
            MD5 md5 = MD5.Create();
            // int N = 10000;
            var data = Encoding.ASCII.GetBytes(str);

            new Random(42).NextBytes(data);
            byte[] Sha256 = sha256.ComputeHash(data);
            byte[] Md5 = md5.ComputeHash(data);
            // var PiCalc = Math.Pow(Math.Abs(200.452 * Math.PI), 12.0) / 12.5;
            return Sha256.ToString();
        }
    }
}
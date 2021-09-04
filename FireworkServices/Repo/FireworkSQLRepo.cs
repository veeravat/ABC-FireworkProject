using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FireworkServices.Context;
using FireworkServices.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FireworkServices.Repo
{
    public class FireworkSQLRepo : IFireworkRepo
    {

        private readonly FireworkContext db;
        private readonly IHubContext<FireworkSignalR> signalR;

        public FireworkSQLRepo(FireworkContext db, IHubContext<FireworkSignalR> signalR)
        {
            this.db = db;
            this.signalR = signalR;
        }

        public async Task<decimal> GetTotalFireworkAsync()
        {
            var fireworks = await db.Fireworks.CountAsync();
            return fireworks;
        }

        public async Task SaveFireworkAsync(FireworkModel Fireworker)
        {
            await db.Fireworks.AddAsync(Fireworker);
            await db.SaveChangesAsync();
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
using System;
using System.Linq;
using System.Threading.Tasks;
using FireworkServices.DTOs;
using FireworkServices.Models;
using FireworkServices.Repo;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FireworkServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FireworkController : ControllerBase
    {
        private readonly IFireworkRepo repo;
        private readonly ILogger<FireworkController> logger;
        private TelemetryClient telemetry;


        public FireworkController(IFireworkRepo repo, ILogger<FireworkController> logger, TelemetryClient telemetry)
        {
            this.repo = repo;
            this.logger = logger;
            this.telemetry = telemetry;
        }

        [HttpPost]
        public async Task<IActionResult> AddFirework(AddFireworkDTO fire)
        {
            FireworkModel firework = new()
            {
                Name = fire.Name,
                FireTime = DateTimeOffset.UtcNow
            };
            try
            {
                await repo.SaveFireworkAsync(firework);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw new Exception("Operation went wrong", ex);
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetFireworks()
        {
            decimal total = 0;
            try
            {
                total = await repo.GetTotalFireworkAsync();
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw new Exception("Operation went wrong", ex);
            }
            return Ok(total);
        }

        [HttpPost("signalR")]
        public async Task<IActionResult> SendSignalR(AddFireworkDTO fire)
        {
            await repo.SentSignalrAsync(fire.Name);
            return Ok();
        }
    }
}
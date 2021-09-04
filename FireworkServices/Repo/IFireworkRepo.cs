using System.Threading.Tasks;
using FireworkServices.Models;

namespace FireworkServices.Repo
{
    public interface IFireworkRepo
    {
        Task SaveFireworkAsync(FireworkModel Fireworker);
        Task SentSignalrAsync(string Name);
        Task<decimal> GetTotalFireworkAsync();
    }
}
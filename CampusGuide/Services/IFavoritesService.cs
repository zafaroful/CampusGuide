using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;

namespace CampusGuide.Services
{
    public interface IFavoritesService
    {
        Task<List<FavoriteItem>> GetFavoritesAsync();
        Task SaveFavoriteAsync(FavoriteItem item);
        Task RemoveFavoriteAsync(string id);
    }
}

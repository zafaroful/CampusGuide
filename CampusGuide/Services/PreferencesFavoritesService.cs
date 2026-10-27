using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CampusGuide.Models;
using Microsoft.Maui.Storage;

namespace CampusGuide.Services
{
    public class PreferencesFavoritesService : IFavoritesService
    {
        const string PreferencesKey = "favorites.json";
        readonly string filePath;

        public PreferencesFavoritesService()
        {
            filePath = Path.Combine(FileSystem.AppDataDirectory, PreferencesKey);
        }

        public async Task<List<FavoriteItem>> GetFavoritesAsync()
        {
            if (!File.Exists(filePath))
                return new List<FavoriteItem>();

            var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<FavoriteItem>>(json) ?? new List<FavoriteItem>();
        }

        public async Task SaveFavoriteAsync(FavoriteItem item)
        {
            var items = await GetFavoritesAsync().ConfigureAwait(false);
            if (!items.Exists(i => i.Id == item.Id))
            {
                items.Add(item);
                var json = JsonSerializer.Serialize(items);
                await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
            }
        }

        public async Task RemoveFavoriteAsync(string id)
        {
            var items = await GetFavoritesAsync().ConfigureAwait(false);
            var removed = items.RemoveAll(i => i.Id == id) > 0;
            if (removed)
            {
                var json = JsonSerializer.Serialize(items);
                await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
            }
        }
    }
}

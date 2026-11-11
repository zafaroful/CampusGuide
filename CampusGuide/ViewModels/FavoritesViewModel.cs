using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CampusGuide.Models;
using CampusGuide.Services;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.ViewModels
{
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly IFavoritesService _favoritesService;
        private readonly IFirestoreService _firestoreService;

        [ObservableProperty]
        private ObservableCollection<FavoriteItem> favorites = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasFavorites = false;

        [ObservableProperty]
        private bool isEmpty = false;

        [ObservableProperty]
        private string countText = string.Empty;

        public FavoritesViewModel()
        {
            _favoritesService = ServiceHelper.GetService<IFavoritesService>() ?? throw new InvalidOperationException("IFavoritesService not registered");
            _firestoreService = ServiceHelper.GetService<IFirestoreService>() ?? throw new InvalidOperationException("IFirestoreService not registered");

            // Load favorites when initialized
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(300); // Give app time to initialize
                await LoadFavoritesCommand.ExecuteAsync(null);
            });
        }

        /// <summary>
        /// Loads all favorites from local storage.
        /// </summary>
        [RelayCommand]
        public async Task LoadFavorites()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine("FavoritesViewModel: Loading favorites from local storage...");
                var favoritesList = await _favoritesService.GetFavoritesAsync().ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Favorites.Clear();

                    if (favoritesList != null && favoritesList.Count > 0)
                    {
                        foreach (var favorite in favoritesList)
                        {
                            Favorites.Add(favorite);
                        }
                    }

                    HasFavorites = Favorites.Count > 0;
                    IsEmpty = !HasFavorites;
                    UpdateCountText();
                    System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Loaded {Favorites.Count} favorites");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Error loading favorites: {ex.Message}");
                ErrorMessage = $"Failed to load favorites: {ex.Message}";
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HasFavorites = false;
                    IsEmpty = false;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Removes a favorite by ID.
        /// </summary>
        [RelayCommand]
        public async Task RemoveFavorite(string favoriteId)
        {
            if (string.IsNullOrEmpty(favoriteId))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Removing favorite {favoriteId}");
                await _favoritesService.RemoveFavoriteAsync(favoriteId).ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var itemToRemove = Favorites.FirstOrDefault(f => f.Id == favoriteId);
                    if (itemToRemove != null)
                    {
                        Favorites.Remove(itemToRemove);
                    }

                    HasFavorites = Favorites.Count > 0;
                    UpdateCountText();
                    System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Favorite removed, {Favorites.Count} remaining");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Error removing favorite: {ex.Message}");
                ErrorMessage = $"Failed to remove favorite: {ex.Message}";
            }
        }

        /// <summary>
        /// Adds a location to favorites.
        /// </summary>
        [RelayCommand]
        public async Task AddFavorite(LocationModel location)
        {
            if (location == null)
                return;

            try
            {
                var favoriteItem = new FavoriteItem
                {
                    Id = location.Id,
                    LocationId = location.Id,
                    Name = location.Name,
                    Description = location.Description,
                    Type = location.Type,
                    Building = location.Building,
                    Floor = location.Floor
                };

                System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Adding favorite {location.Name}");
                await _favoritesService.SaveFavoriteAsync(favoriteItem).ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadFavoritesCommand.ExecuteAsync(null);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FavoritesViewModel: Error adding favorite: {ex.Message}");
                ErrorMessage = $"Failed to add favorite: {ex.Message}";
            }
        }

        private void UpdateCountText()
        {
            CountText = Favorites.Count == 1 ? "1 saved place" : $"{Favorites.Count} saved places";
        }

        /// <summary>
        /// Checks if a location is in favorites.
        /// </summary>
        public bool IsFavorite(string locationId)
        {
            return Favorites.Any(f => f.LocationId == locationId);
        }

        /// <summary>
        /// Refreshes the favorites list.
        /// </summary>
        [RelayCommand]
        public async Task RefreshFavorites()
        {
            await LoadFavoritesCommand.ExecuteAsync(null);
        }
    }
}

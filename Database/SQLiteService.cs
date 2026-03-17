using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TravelGuideApp.Models;
using TravelGuideApp.Services;

namespace TravelGuideApp.Database
{
    public class SQLiteService
    {
        private SQLiteAsyncConnection _database;
        private const string UserTable = "User";
        private const string PoiTable = "POI";
        private const string TourTable = "Tour";

        public SQLiteService()
        {
        }

        async Task Init()
        {
            if (_database is not null)
                return;

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TravelGuide.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<POI>();
            await _database.CreateTableAsync<POI_Media>();
            await _database.CreateTableAsync<POI_Image>();
            await _database.CreateTableAsync<Tour>();
            await _database.CreateTableAsync<Tour_POI>();
            await _database.CreateTableAsync<QR_Code>();
            await _database.CreateTableAsync<User_POI_Log>();
            await _database.CreateTableAsync<User_Favorite_POI>();
            await _database.CreateTableAsync<User_Favorite_Tour>();
            await _database.CreateTableAsync<Review>();

            await EnsureColumnAsync(UserTable, "Points", "INTEGER");
            await EnsureColumnAsync(PoiTable, "DescriptionEn", "TEXT");
            await EnsureColumnAsync(TourTable, "DescriptionEn", "TEXT");
        }

        private async Task EnsureColumnAsync(string table, string column, string type)
        {
            var columns = await _database.QueryAsync<TableColumnInfo>($"PRAGMA table_info({table})");
            if (columns.Any(c => string.Equals(c.name, column, StringComparison.OrdinalIgnoreCase)))
                return;

            await _database.ExecuteAsync($"ALTER TABLE {table} ADD COLUMN {column} {type}");
        }

        private class TableColumnInfo
        {
            public string name { get; set; }
        }

        public async Task<List<POI>> GetPOIsAsync()
        {
            await Init();
            return await _database.Table<POI>().ToListAsync();
        }

        public async Task<POI> GetPOIByIdAsync(int id)
        {
            await Init();
            return await _database.Table<POI>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Tour> GetTourByIdAsync(int id)
        {
            await Init();
            return await _database.Table<Tour>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<POI>> GetPOIsForTourAsync(int tourId)
        {
            await Init();
            var mappings = await _database.Table<Tour_POI>()
                .Where(tp => tp.TourId == tourId)
                .OrderBy(tp => tp.OrderIndex)
                .ToListAsync();

            if (mappings.Count == 0)
                return new List<POI>();

            // Only fetch the specific POI IDs needed — avoids full-table scan
            var poiIds = mappings.Select(m => m.PoiId).ToHashSet();
            var pois   = await _database.Table<POI>()
                .Where(p => poiIds.Contains(p.Id))
                .ToListAsync();

            var map = pois.ToDictionary(p => p.Id, p => p);
            return mappings
                .Where(m => map.ContainsKey(m.PoiId))
                .Select(m => map[m.PoiId])
                .ToList();
        }

        public async Task<List<POI_Image>> GetPoiImagesAsync(int poiId)
        {
            await Init();
            return await _database.Table<POI_Image>()
                .Where(i => i.PoiId == poiId)
                .ToListAsync();
        }

        public async Task<List<POI_Media>> GetPoiMediaAsync(int poiId)
        {
            await Init();
            return await _database.Table<POI_Media>()
                .Where(m => m.PoiId == poiId)
                .ToListAsync();
        }

        public async Task<bool> IsPoiFavoriteAsync(int userId, int poiId)
        {
            await Init();
            var fav = await _database.Table<User_Favorite_POI>()
                .Where(f => f.UserId == userId && f.PoiId == poiId)
                .FirstOrDefaultAsync();
            return fav != null;
        }

        public async Task TogglePoiFavoriteAsync(int userId, int poiId)
        {
            await Init();
            var fav = await _database.Table<User_Favorite_POI>()
                .Where(f => f.UserId == userId && f.PoiId == poiId)
                .FirstOrDefaultAsync();

            if (fav == null)
            {
                await _database.InsertAsync(new User_Favorite_POI
                {
                    UserId = userId,
                    PoiId = poiId
                });
            }
            else
            {
                await _database.DeleteAsync(fav);
            }
        }

        public async Task<List<POI>> GetFavoritePoisAsync(int userId)
        {
            await Init();
            var favs = await _database.Table<User_Favorite_POI>()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (favs.Count == 0)
                return new List<POI>();

            // Only fetch the specific POI IDs needed — avoids full-table scan
            var poiIds = favs.Select(f => f.PoiId).ToHashSet();
            return await _database.Table<POI>()
                .Where(p => poiIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<bool> IsTourFavoriteAsync(int userId, int tourId)
        {
            await Init();
            var fav = await _database.Table<User_Favorite_Tour>()
                .Where(f => f.UserId == userId && f.TourId == tourId)
                .FirstOrDefaultAsync();
            return fav != null;
        }

        public async Task ToggleTourFavoriteAsync(int userId, int tourId)
        {
            await Init();
            var fav = await _database.Table<User_Favorite_Tour>()
                .Where(f => f.UserId == userId && f.TourId == tourId)
                .FirstOrDefaultAsync();

            if (fav == null)
            {
                await _database.InsertAsync(new User_Favorite_Tour
                {
                    UserId = userId,
                    TourId = tourId
                });
            }
            else
            {
                await _database.DeleteAsync(fav);
            }
        }

        public async Task<List<Tour>> GetFavoriteToursAsync(int userId)
        {
            await Init();
            var favs = await _database.Table<User_Favorite_Tour>()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (favs.Count == 0)
                return new List<Tour>();

            // Only fetch the specific Tour IDs needed — avoids full-table scan
            var tourIds = favs.Select(f => f.TourId).ToHashSet();
            return await _database.Table<Tour>()
                .Where(t => tourIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<int> AddReviewAsync(Review review)
        {
            await Init();
            review.CreatedAt = DateTime.UtcNow;
            return await _database.InsertAsync(review);
        }

        public async Task<List<Review>> GetReviewsForTourAsync(int tourId)
        {
            await Init();
            return await _database.Table<Review>()
                .Where(r => r.TourId == tourId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task ReplacePOIsAsync(IEnumerable<POI> items)
        {
            await Init();
            await _database.DeleteAllAsync<POI>();
            await _database.InsertAllAsync(items);
        }

        public async Task ReplacePoiImagesAsync(IEnumerable<POI_Image> items)
        {
            await Init();
            await _database.DeleteAllAsync<POI_Image>();
            await _database.InsertAllAsync(items);
        }

        public async Task ReplacePoiMediaAsync(IEnumerable<POI_Media> items)
        {
            await Init();
            await _database.DeleteAllAsync<POI_Media>();
            await _database.InsertAllAsync(items);
        }

        public async Task<int> SavePOIAsync(POI item)
        {
            await Init();
            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        public async Task<List<Tour>> GetToursAsync()
        {
            await Init();
            return await _database.Table<Tour>().ToListAsync();
        }

        public async Task ReplaceToursAsync(IEnumerable<Tour> items)
        {
            await Init();
            await _database.DeleteAllAsync<Tour>();
            await _database.InsertAllAsync(items);
        }

        public async Task SeedDataAsync()
        {
            await Init();
            var count = await _database.Table<POI>().CountAsync();
            if (count == 0)
            {
                // Seed some POIs
                await _database.InsertAsync(new POI
                {
                    Name = "Nhà thờ Đức Bà",
                    Description = "Nhà thờ Công giáo có kiến trúc cổ kính ở trung tâm TP.HCM.",
                    DescriptionEn = "Historic Catholic cathedral in the heart of Ho Chi Minh City.",
                    Latitude = 10.7797,
                    Longitude = 106.6990,
                    Radius = 100,
                    Priority = 1
                });

                await _database.InsertAsync(new POI
                {
                    Name = "Dinh Độc Lập",
                    Description = "Di tích lịch sử nổi tiếng tại TP.HCM, nơi đánh dấu sự thống nhất đất nước.",
                    DescriptionEn = "Famous historical landmark in Ho Chi Minh City, symbol of reunification.",
                    Latitude = 10.7770,
                    Longitude = 106.6953,
                    Radius = 150,
                    Priority = 2
                });
            }

            var tourCount = await _database.Table<Tour>().CountAsync();
            if (tourCount == 0)
            {
                await _database.InsertAsync(new Tour
                {
                    Name = "Tour Trung Tâm TP.HCM",
                    Description = "Hành trình tham quan các điểm nổi bật trung tâm thành phố.",
                    DescriptionEn = "A city-center route covering the most notable landmarks.",
                    EstimatedTime = TimeSpan.FromHours(3)
                });
            }

            var relationCount = await _database.Table<Tour_POI>().CountAsync();
            if (relationCount == 0)
            {
                var tour = await _database.Table<Tour>().FirstOrDefaultAsync();
                var pois = await _database.Table<POI>().ToListAsync();
                if (tour != null && pois.Count > 0)
                {
                    await _database.InsertAsync(new Tour_POI
                    {
                        TourId = tour.Id,
                        PoiId = pois[0].Id,
                        OrderIndex = 1
                    });

                    if (pois.Count > 1)
                    {
                        await _database.InsertAsync(new Tour_POI
                        {
                            TourId = tour.Id,
                            PoiId = pois[1].Id,
                            OrderIndex = 2
                        });
                    }
                }
            }

            var qrCount = await _database.Table<QR_Code>().CountAsync();
            if (qrCount == 0)
            {
                var firstPoi = await _database.Table<POI>().FirstOrDefaultAsync();
                if (firstPoi != null)
                {
                    await _database.InsertAsync(new QR_Code
                    {
                        PoiId = firstPoi.Id,
                        QrValue = $"POI-{firstPoi.Id}"
                    });
                }
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            await Init();
            return await _database.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            await Init();
            return await _database.Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByPhoneAsync(string phone)
        {
            await Init();
            return await _database.Table<User>()
                .Where(u => u.PhoneNumber == phone)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateUserAsync(User user, string plainPassword)
        {
            await Init();
            user.PasswordHash = PasswordHasher.Hash(plainPassword);
            return await _database.InsertAsync(user);
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            await Init();
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return null;

            return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
        }

        public async Task<POI> GetPoiByQrValueAsync(string qrValue)
        {
            await Init();
            if (string.IsNullOrWhiteSpace(qrValue))
                return null;

            var qr = await _database.Table<QR_Code>()
                .Where(q => q.QrValue == qrValue)
                .FirstOrDefaultAsync();

            if (qr == null)
            {
                if (qrValue.StartsWith("POI-", StringComparison.OrdinalIgnoreCase))
                {
                    var idPart = qrValue.Substring(4);
                    if (int.TryParse(idPart, out var poiId))
                    {
                        return await GetPOIByIdAsync(poiId);
                    }
                }
                return null;
            }

            return await GetPOIByIdAsync(qr.PoiId);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            await Init();
            return await _database.UpdateAsync(user);
        }

        public async Task<int> AddUserPoiLogAsync(int userId, int poiId, string triggerType)
        {
            await Init();
            var now = DateTime.UtcNow;
            var log = new User_POI_Log
            {
                UserId = userId,
                PoiId = poiId,
                StartTime = now,
                EndTime = now,
                TriggerType = triggerType
            };
            var insertResult = await _database.InsertAsync(log);

            var user = await _database.Table<User>()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            if (user != null)
            {
                user.Points += 10;
                if (user.Points >= 100)
                {
                    user.Tier = "VIP";
                }
                await _database.UpdateAsync(user);
            }

            return insertResult;
        }

        public async Task<List<User_POI_Log>> GetUserPoiLogsAsync(int userId)
        {
            await Init();
            return await _database.Table<User_POI_Log>()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();
        }
    }
}

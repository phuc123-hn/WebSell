using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using web1.Data;
using web1.Models;
using web1.Infrastructure.Logging;
using web1.Infrastructure.Caching;

namespace web1.Services
{
    /// <summary>
    /// Application settings service - manage system configuration
    /// Support caching
    /// </summary>
    public interface ISettingsService
    {
        string GetSetting(string key, string defaultValue = "");
        Task<string> GetSettingAsync(string key, string defaultValue = "");
        bool GetBoolSetting(string key, bool defaultValue = false);
        int GetIntSetting(string key, int defaultValue = 0);
        void SetSetting(string key, string value, string description = "");
        void ClearCache();
    }

    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICacheService _cacheService;
        private readonly FileLogger _logger = new FileLogger();

        public SettingsService(ApplicationDbContext db = null, ICacheService cacheService = null)
        {
            _db = db ?? new ApplicationDbContext();
            _cacheService = cacheService ?? new CacheService();
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                // Try cache first
                string cachedValue = _cacheService.Get<string>($"Setting:{key}");
                if (!string.IsNullOrEmpty(cachedValue))
                    return cachedValue;

                // Get from database
                var setting = _db.ApplicationSettings.FirstOrDefault(s => s.Key == key);
                string value = setting?.Value ?? defaultValue;

                // Cache result
                if (setting != null)
                    _cacheService.Set($"Setting:{key}", value, 60); // Cache 1 hour

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting setting: {key}", ex);
                return defaultValue;
            }
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            try
            {
                // Try cache first
                string cachedValue = _cacheService.Get<string>($"Setting:{key}");
                if (!string.IsNullOrEmpty(cachedValue))
                    return cachedValue;

                // Get from database
                var setting = await _db.ApplicationSettings.FirstOrDefaultAsync(s => s.Key == key);
                string value = setting?.Value ?? defaultValue;

                // Cache result
                if (setting != null)
                    _cacheService.Set($"Setting:{key}", value, 60);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting setting async: {key}", ex);
                return defaultValue;
            }
        }

        public bool GetBoolSetting(string key, bool defaultValue = false)
        {
            string value = GetSetting(key);
            if (bool.TryParse(value, out bool result))
                return result;
            return defaultValue;
        }

        public int GetIntSetting(string key, int defaultValue = 0)
        {
            string value = GetSetting(key);
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        public void SetSetting(string key, string value, string description = "")
        {
            try
            {
                var setting = _db.ApplicationSettings.FirstOrDefault(s => s.Key == key);

                if (setting == null)
                {
                    setting = new ApplicationSetting
                    {
                        Key = key,
                        Value = value,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ApplicationSettings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                _db.SaveChanges();

                // Invalidate cache
                _cacheService.Remove($"Setting:{key}");

                _logger.LogInfo($"Setting updated: {key} = {value}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting value for key: {key}", ex);
            }
        }

        public void ClearCache()
        {
            _cacheService.RemoveByPattern("Setting:");
            _logger.LogInfo("Settings cache cleared");
        }
    }
}

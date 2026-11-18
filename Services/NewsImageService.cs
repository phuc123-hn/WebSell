using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using web1.Data;
using web1.Models;
using web1.Infrastructure.Logging;

namespace web1.Services
{
    public interface INewsImageService
    {
        bool UploadNewsImage(int newsId, HttpPostedFileBase file, out string savedPath);
        bool DeleteNewsImage(int imageId);
        List<NewsImage> GetNewsImages(int newsId);
        bool DeleteAllNewsImages(int newsId);
    }

    public class NewsImageService : INewsImageService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();

        // Configuration
        private const int MAX_FILE_SIZE_MB = 5;
        private const long MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
        private static readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".webp" };
        private const string UPLOAD_FOLDER = "Content/Images/News";

        public NewsImageService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();
        }

        /// <summary>
        /// Upload news image with validation
        /// </summary>
        public bool UploadNewsImage(int newsId, HttpPostedFileBase file, out string savedPath)
        {
            savedPath = null;

            try
            {
                // Validate news exists
                var news = _db.News.Find(newsId);
                if (news == null)
                {
                    _logger.LogWarning($"UploadNewsImage: News {newsId} not found");
                    return false;
                }

                // Validate file
                if (file == null || file.ContentLength == 0)
                {
                    _logger.LogWarning($"UploadNewsImage: File is empty");
                    return false;
                }

                // Validate file size
                if (file.ContentLength > MAX_FILE_SIZE_BYTES)
                {
                    _logger.LogWarning($"UploadNewsImage: File size {file.ContentLength} exceeds max {MAX_FILE_SIZE_BYTES}");
                    return false;
                }

                // Validate file extension
                string fileName = Path.GetFileName(file.FileName);
                string fileExtension = Path.GetExtension(fileName).ToLower();

                if (!ALLOWED_EXTENSIONS.Contains(fileExtension))
                {
                    _logger.LogWarning($"UploadNewsImage: Invalid extension {fileExtension}");
                    return false;
                }

                // Create upload folder if not exists
                string uploadPath = HttpContext.Current.Server.MapPath($"~/{UPLOAD_FOLDER}/{newsId}");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename to prevent overwrites
                string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
                string fullPath = Path.Combine(uploadPath, uniqueFileName);

                // Save file
                file.SaveAs(fullPath);

                // Save to database
                var newsImage = new NewsImage
                {
                    NewsId = newsId,
                    ImageUrl = $"~/{UPLOAD_FOLDER}/{newsId}/{uniqueFileName}",
                    DisplayOrder = 0, // Will be set in Edit view
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _db.NewsImages.Add(newsImage);
                _db.SaveChanges();

                savedPath = newsImage.ImageUrl;

                _logger.LogInfo($"UploadNewsImage: File uploaded successfully for news {newsId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading news image for news {newsId}", ex);
                return false;
            }
        }

        /// <summary>
        /// Delete news image (soft delete + physical file deletion)
        /// </summary>
        public bool DeleteNewsImage(int imageId)
        {
            try
            {
                var newsImage = _db.NewsImages.Find(imageId);
                if (newsImage == null)
                {
                    _logger.LogWarning($"DeleteNewsImage: Image {imageId} not found");
                    return false;
                }

                // Delete physical file
                try
                {
                    string physicalPath = HttpContext.Current.Server.MapPath(newsImage.ImageUrl);
                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                    }
                }
                catch (Exception)
                {
                    _logger.LogWarning($"DeleteNewsImage: Could not delete physical file for image {imageId}");
                    // Continue with database deletion even if file deletion fails
                }

                // Soft delete from database
                newsImage.IsDeleted = true;
                _db.SaveChanges();

                _logger.LogInfo($"DeleteNewsImage: Image {imageId} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting news image {imageId}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get all active images for a news article
        /// </summary>
        public List<NewsImage> GetNewsImages(int newsId)
        {
            try
            {
                return _db.NewsImages
                    .Where(ni => ni.NewsId == newsId && !ni.IsDeleted)
                    .OrderBy(ni => ni.DisplayOrder)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting images for news {newsId}", ex);
                return new List<NewsImage>();
            }
        }

        /// <summary>
        /// Delete all images for a news article (when news is deleted)
        /// </summary>
        public bool DeleteAllNewsImages(int newsId)
        {
            try
            {
                var newsImages = _db.NewsImages.Where(ni => ni.NewsId == newsId && !ni.IsDeleted).ToList();

                foreach (var image in newsImages)
                {
                    DeleteNewsImage(image.Id);
                }

                _logger.LogInfo($"DeleteAllNewsImages: All images deleted for news {newsId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting all images for news {newsId}", ex);
                return false;
            }
        }
    }
}

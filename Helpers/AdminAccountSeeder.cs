using System;
using System.Linq;
using web1.Data;
using web1.Infrastructure.Security;
using web1.Models;

namespace web1.Helpers
{
    /// <summary>
    /// Helper to seed admin account with new password hasher
    /// </summary>
    public static class AdminAccountSeeder
    {
        /// <summary>
        /// Seed or recreate admin account
        /// </summary>
        public static void SeedAdminAccount(ApplicationDbContext db)
        {
            try
            {
                var passwordHasher = new PasswordHasherService();

                // Check if admin exists
                var existingAdmin = db.Users.FirstOrDefault(u => u.Username == "admin123");

                if (existingAdmin != null)
                {
                    // Delete existing admin
                    db.Users.Remove(existingAdmin);
                    db.SaveChanges();
                }

                // Create new admin with secure password hash
                var adminUser = new User
                {
                    Username = "admin123",
                    PasswordHash = passwordHasher.HashPassword("Admin@123456"), // Strong password
                    Email = "admin@techworld.vn",
                    Phone = "0123456789",
                    FullName = "Administrator",
                    IsVerified = true,
                    OTP = "",
                    OTPExpire = DateTime.Now,
                    Role = "Admin",
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(adminUser);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("✅ Admin account created successfully!");
                System.Diagnostics.Debug.WriteLine("Username: admin123");
                System.Diagnostics.Debug.WriteLine("Password: Admin@123456");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error creating admin account: " + ex.Message);
            }
        }

        /// <summary>
        /// Seed test accounts for different roles
        /// </summary>
        public static void SeedTestAccounts(ApplicationDbContext db)
        {
            try
            {
                var passwordHasher = new PasswordHasherService();

                // Test Customer Account
                if (!db.Users.Any(u => u.Username == "testuser"))
                {
                    var testUser = new User
                    {
                        Username = "testuser",
                        PasswordHash = passwordHasher.HashPassword("Test@123456"),
                        Email = "testuser@techworld.vn",
                        Phone = "0987654321",
                        FullName = "Test User",
                        IsVerified = true,
                        OTP = "",
                        OTPExpire = DateTime.Now,
                        Role = "Customer",
                        CreatedAt = DateTime.Now
                    };
                    db.Users.Add(testUser);
                }

                // Another test customer
                if (!db.Users.Any(u => u.Username == "john_doe"))
                {
                    var johnDoe = new User
                    {
                        Username = "john_doe",
                        PasswordHash = passwordHasher.HashPassword("John@123456"),
                        Email = "john@example.com",
                        Phone = "0912345678",
                        FullName = "John Doe",
                        IsVerified = true,
                        OTP = "",
                        OTPExpire = DateTime.Now,
                        Role = "Customer",
                        CreatedAt = DateTime.Now
                    };
                    db.Users.Add(johnDoe);
                }

                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("✅ Test accounts created successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error creating test accounts: " + ex.Message);
            }
        }
    }
}

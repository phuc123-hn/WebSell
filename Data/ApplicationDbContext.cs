using System;
using System.Data.Entity;
using web1.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace web1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=DefaultConnection")
        {
            // Tắt Dynamic Proxy để tránh System.Data.Entity.DynamicProxies
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }
        public DbSet<EventQuestion> EventQuestions { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        
        // NEW: Lucky Spin
        public DbSet<LuckySpinPrize> LuckySpinPrizes { get; set; }
        public DbSet<LuckySpinHistory> LuckySpinHistories { get; set; }
        public DbSet<OrderVoucher> OrderVouchers { get; set; }

        // NEW: Community Module
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<UserBan> UserBans { get; set; }
        public DbSet<PostWarning> PostWarnings { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<AccountLockout> AccountLockouts { get; set; }

        // NEW: Authentication & Security
        public DbSet<RateLimitToken> RateLimitTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; }

        // NEW: Phase 1 - Refactored Anti-Abuse (Tách riêng biệt)
        public DbSet<OtpToken> OtpTokens { get; set; }
        public DbSet<RateLimitLog> RateLimitLogs { get; set; }
        public DbSet<BlockedEntity> BlockedEntities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Voucher>()
                .HasOptional(v => v.Event)
                .WithMany(e => e.Vouchers)
                .HasForeignKey(v => v.EventId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Voucher>()
                .HasOptional(v => v.OwnerUser)
                .WithMany()
                .HasForeignKey(v => v.OwnerUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VoucherUsage>()
                .HasRequired(vu => vu.Voucher)
                .WithMany(v => v.VoucherUsages)
                .HasForeignKey(vu => vu.VoucherId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VoucherUsage>()
                .HasRequired(vu => vu.User)
                .WithMany()
                .HasForeignKey(vu => vu.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventQuestion>()
                .HasRequired(eq => eq.Event)
                .WithMany(e => e.Questions)
                .HasForeignKey(eq => eq.EventId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.User)
                .WithMany()
                .HasForeignKey(ep => ep.UserId)
                .WillCascadeOnDelete(false);

            // NEW: Order Voucher
            modelBuilder.Entity<OrderVoucher>()
                .HasRequired(ov => ov.Order)
                .WithMany(o => o.OrderVouchers)
                .HasForeignKey(ov => ov.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderVoucher>()
                .HasRequired(ov => ov.Voucher)
                .WithMany()
                .HasForeignKey(ov => ov.VoucherId)
                .WillCascadeOnDelete(false);

            // NEW: Lucky Spin History
            modelBuilder.Entity<LuckySpinHistory>()
                .HasRequired(lsh => lsh.User)
                .WithMany()
                .HasForeignKey(lsh => lsh.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LuckySpinHistory>()
                .HasRequired(lsh => lsh.Order)
                .WithMany()
                .HasForeignKey(lsh => lsh.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<LuckySpinHistory>()
                .HasRequired(lsh => lsh.Prize)
                .WithMany()
                .HasForeignKey(lsh => lsh.PrizeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LuckySpinHistory>()
                .HasOptional(lsh => lsh.WonVoucher)
                .WithMany()
                .HasForeignKey(lsh => lsh.WonVoucherId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LuckySpinHistory>()
                .HasOptional(lsh => lsh.WonProduct)
                .WithMany()
                .HasForeignKey(lsh => lsh.WonProductId)
                .WillCascadeOnDelete(false);

            // NEW: Community Module - Post
            modelBuilder.Entity<Post>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);

            // NEW: Community Module - Comment
            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            // NEW: Community Module - Like
            modelBuilder.Entity<Like>()
                .HasRequired(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Like>()
                .HasRequired(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .WillCascadeOnDelete(false);

            // UNIQUE constraint: (PostId, UserId) - prevent multiple likes
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.PostId, l.UserId })
                .IsUnique();

            // NEW: Community Module - Reaction
            modelBuilder.Entity<Reaction>()
                .HasRequired(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Reaction>()
                .HasRequired(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            // UNIQUE constraint: (PostId, UserId) - prevent multiple reactions from same user
            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.PostId, r.UserId })
                .IsUnique();

            // NEW: Community Module - Message
            modelBuilder.Entity<Message>()
                .HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .WillCascadeOnDelete(false);

            // NEW: UserBan
            modelBuilder.Entity<UserBan>()
                .HasRequired(ub => ub.User)
                .WithMany()
                .HasForeignKey(ub => ub.UserId)
                .WillCascadeOnDelete(false);

            // NEW: PostWarning
            modelBuilder.Entity<PostWarning>()
                .HasRequired(pw => pw.Post)
                .WithMany()
                .HasForeignKey(pw => pw.PostId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PostWarning>()
                .HasRequired(pw => pw.Admin)
                .WithMany()
                .HasForeignKey(pw => pw.AdminId)
                .WillCascadeOnDelete(false);

            // NEW: AccountLockout
            modelBuilder.Entity<AccountLockout>()
                .HasRequired(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .WillCascadeOnDelete(false);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(password));

                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}

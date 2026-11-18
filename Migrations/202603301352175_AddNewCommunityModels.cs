namespace web1.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewCommunityModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountLockouts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        LockedUntil = c.DateTime(nullable: false),
                        FailedAttempts = c.Int(nullable: false),
                        LockTime = c.DateTime(nullable: false),
                        Reason = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Phone = c.String(nullable: false, maxLength: 10),
                        FullName = c.String(nullable: false, maxLength: 100),
                        IsVerified = c.Boolean(nullable: false),
                        OTP = c.String(maxLength: 10),
                        OTPExpire = c.DateTime(nullable: false),
                        Role = c.String(maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Username, unique: true)
                .Index(t => t.Email, unique: true);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        ShippingAddress = c.String(maxLength: 500),
                        ShippingPhone = c.String(maxLength: 10),
                        ShippingName = c.String(maxLength: 100),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                        PaymentMethod = c.String(),
                        IsPaid = c.Boolean(nullable: false),
                        Notes = c.String(),
                        AppliedVoucherCodes = c.String(maxLength: 500),
                        TotalVoucherDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        CategoryId = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 2000),
                        ImageUrl = c.String(),
                        Stock = c.Int(nullable: false),
                        IsHot = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Rating = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Type = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderVouchers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        VoucherId = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AppliedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Vouchers", t => t.VoucherId)
                .Index(t => t.OrderId)
                .Index(t => t.VoucherId);
            
            CreateTable(
                "dbo.Vouchers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 500),
                        DiscountPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UsedQuantity = c.Int(nullable: false),
                        MinOrderAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EventId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        RequireEventParticipation = c.Boolean(nullable: false),
                        IsLuckySpinVoucher = c.Boolean(nullable: false),
                        IsPrivate = c.Boolean(nullable: false),
                        OwnerUserId = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId)
                .ForeignKey("dbo.Users", t => t.OwnerUserId)
                .Index(t => t.Code, unique: true)
                .Index(t => t.EventId)
                .Index(t => t.OwnerUserId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 500),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ImageUrl = c.String(maxLength: 1000),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventParticipants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        DiscountPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ParticipatedAt = c.DateTime(nullable: false),
                        CompletedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.EventId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.EventQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        Question = c.String(nullable: false, maxLength: 500),
                        OptionA = c.String(nullable: false, maxLength: 255),
                        OptionB = c.String(nullable: false, maxLength: 255),
                        OptionC = c.String(nullable: false, maxLength: 255),
                        OptionD = c.String(nullable: false, maxLength: 255),
                        CorrectAnswer = c.String(nullable: false, maxLength: 1),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.VoucherUsages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VoucherId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        OrderId = c.Int(),
                        UsedAt = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Vouchers", t => t.VoucherId)
                .Index(t => t.VoucherId)
                .Index(t => t.UserId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Content = c.String(nullable: false, maxLength: 2000),
                        CreatedAt = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Content = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Rating = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        ImageUrl = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Likes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => new { t.PostId, t.UserId }, unique: true);
            
            CreateTable(
                "dbo.Reactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ReactionType = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => new { t.PostId, t.UserId }, unique: true);
            
            CreateTable(
                "dbo.LoginAttempts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false),
                        IpAddress = c.String(nullable: false),
                        IsSuccessful = c.Boolean(nullable: false),
                        AttemptTime = c.DateTime(nullable: false),
                        FailureReason = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LuckySpinHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false),
                        PrizeId = c.Int(nullable: false),
                        WonVoucherId = c.Int(),
                        WonProductId = c.Int(),
                        Status = c.String(),
                        SpinDate = c.DateTime(nullable: false),
                        ClaimedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.LuckySpinPrizes", t => t.PrizeId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Products", t => t.WonProductId)
                .ForeignKey("dbo.Vouchers", t => t.WonVoucherId)
                .Index(t => t.UserId)
                .Index(t => t.OrderId)
                .Index(t => t.PrizeId)
                .Index(t => t.WonVoucherId)
                .Index(t => t.WonProductId);
            
            CreateTable(
                "dbo.LuckySpinPrizes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 500),
                        WinRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PrizeType = c.String(nullable: false, maxLength: 50),
                        VoucherId = c.Int(),
                        ProductId = c.Int(),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.News",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Content = c.String(),
                        ImageUrl = c.String(),
                        PublishDate = c.DateTime(nullable: false),
                        Category = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        Summary = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PostWarnings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        AdminId = c.Int(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 500),
                        WarningDate = c.DateTime(nullable: false),
                        Action = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AdminId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.UserBans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        WarningCount = c.Int(nullable: false),
                        IsBanned = c.Boolean(nullable: false),
                        BanStartDate = c.DateTime(),
                        BanEndDate = c.DateTime(),
                        BanReason = c.String(maxLength: 500),
                        BanDays = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserBans", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostWarnings", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostWarnings", "AdminId", "dbo.Users");
            DropForeignKey("dbo.Messages", "UserId", "dbo.Users");
            DropForeignKey("dbo.LuckySpinHistories", "WonVoucherId", "dbo.Vouchers");
            DropForeignKey("dbo.LuckySpinHistories", "WonProductId", "dbo.Products");
            DropForeignKey("dbo.LuckySpinHistories", "UserId", "dbo.Users");
            DropForeignKey("dbo.LuckySpinHistories", "PrizeId", "dbo.LuckySpinPrizes");
            DropForeignKey("dbo.LuckySpinHistories", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Posts", "UserId", "dbo.Users");
            DropForeignKey("dbo.Reactions", "UserId", "dbo.Users");
            DropForeignKey("dbo.Reactions", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Likes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Likes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.AccountLockouts", "UserId", "dbo.Users");
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.OrderVouchers", "VoucherId", "dbo.Vouchers");
            DropForeignKey("dbo.VoucherUsages", "VoucherId", "dbo.Vouchers");
            DropForeignKey("dbo.VoucherUsages", "UserId", "dbo.Users");
            DropForeignKey("dbo.VoucherUsages", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Vouchers", "OwnerUserId", "dbo.Users");
            DropForeignKey("dbo.Vouchers", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventQuestions", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventParticipants", "UserId", "dbo.Users");
            DropForeignKey("dbo.EventParticipants", "EventId", "dbo.Events");
            DropForeignKey("dbo.OrderVouchers", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderDetails", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.OrderDetails", "OrderId", "dbo.Orders");
            DropIndex("dbo.UserBans", new[] { "UserId" });
            DropIndex("dbo.PostWarnings", new[] { "AdminId" });
            DropIndex("dbo.PostWarnings", new[] { "PostId" });
            DropIndex("dbo.Messages", new[] { "UserId" });
            DropIndex("dbo.LuckySpinHistories", new[] { "WonProductId" });
            DropIndex("dbo.LuckySpinHistories", new[] { "WonVoucherId" });
            DropIndex("dbo.LuckySpinHistories", new[] { "PrizeId" });
            DropIndex("dbo.LuckySpinHistories", new[] { "OrderId" });
            DropIndex("dbo.LuckySpinHistories", new[] { "UserId" });
            DropIndex("dbo.Reactions", new[] { "PostId", "UserId" });
            DropIndex("dbo.Likes", new[] { "PostId", "UserId" });
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Comments", new[] { "PostId" });
            DropIndex("dbo.VoucherUsages", new[] { "OrderId" });
            DropIndex("dbo.VoucherUsages", new[] { "UserId" });
            DropIndex("dbo.VoucherUsages", new[] { "VoucherId" });
            DropIndex("dbo.EventQuestions", new[] { "EventId" });
            DropIndex("dbo.EventParticipants", new[] { "UserId" });
            DropIndex("dbo.EventParticipants", new[] { "EventId" });
            DropIndex("dbo.Vouchers", new[] { "OwnerUserId" });
            DropIndex("dbo.Vouchers", new[] { "EventId" });
            DropIndex("dbo.Vouchers", new[] { "Code" });
            DropIndex("dbo.OrderVouchers", new[] { "VoucherId" });
            DropIndex("dbo.OrderVouchers", new[] { "OrderId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.OrderDetails", new[] { "ProductId" });
            DropIndex("dbo.OrderDetails", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.Users", new[] { "Username" });
            DropIndex("dbo.AccountLockouts", new[] { "UserId" });
            DropTable("dbo.UserBans");
            DropTable("dbo.PostWarnings");
            DropTable("dbo.News");
            DropTable("dbo.Messages");
            DropTable("dbo.LuckySpinPrizes");
            DropTable("dbo.LuckySpinHistories");
            DropTable("dbo.LoginAttempts");
            DropTable("dbo.Reactions");
            DropTable("dbo.Likes");
            DropTable("dbo.Posts");
            DropTable("dbo.Comments");
            DropTable("dbo.VoucherUsages");
            DropTable("dbo.EventQuestions");
            DropTable("dbo.EventParticipants");
            DropTable("dbo.Events");
            DropTable("dbo.Vouchers");
            DropTable("dbo.OrderVouchers");
            DropTable("dbo.Categories");
            DropTable("dbo.Products");
            DropTable("dbo.OrderDetails");
            DropTable("dbo.Orders");
            DropTable("dbo.Users");
            DropTable("dbo.AccountLockouts");
        }
    }
}

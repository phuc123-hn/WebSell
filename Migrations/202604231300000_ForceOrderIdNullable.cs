namespace web1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ForceOrderIdNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LuckySpinHistories", "OrderId", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LuckySpinHistories", "OrderId", c => c.Int(nullable: false));
        }
    }
}

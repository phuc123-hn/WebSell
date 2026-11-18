namespace web1.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSpinCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "SpinCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "SpinCount");
        }
    }
}

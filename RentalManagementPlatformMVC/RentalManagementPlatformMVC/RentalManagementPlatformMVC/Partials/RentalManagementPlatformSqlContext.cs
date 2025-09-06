using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformMVC.Models;

public partial class RentalManagementPlatformSqlContext : DbContext
{
    public RentalManagementPlatformSqlContext() { }

    //EF Core建立連線
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //如果外面已經建立完成了，不須重新設定，直接return
        if (optionsBuilder.IsConfigured)
            return;

        //IConfiguration設定來源
        //ConfigurationBuilder可以逐步組合多個設定來源，最後建立出一個 IConfiguration 物件
        IConfiguration config = new ConfigurationBuilder()
            // 設定檔案的基底路徑
            //AppDomain.CurrentDomain.BaseDirectory為目前應用程式的根目錄
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            // 加入 JSON 檔
            .AddJsonFile("appsettings.json")
            // 生成 IConfiguration設定
            .Build();
        optionsBuilder.UseSqlServer(config.GetConnectionString("RentalManagementPlatformSql"));
    }
}

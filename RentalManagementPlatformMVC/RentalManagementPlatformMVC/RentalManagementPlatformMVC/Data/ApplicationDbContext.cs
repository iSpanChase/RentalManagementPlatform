using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
		// 定義 DbSet (對應資料表)
		public DbSet<User> User { get; set; }

		// 覆寫 OnModelCreating
		protected override void OnModelCreating(ModelBuilder mb)
		{
			base.OnModelCreating(mb);

			mb.Entity<User>(e =>
			{
				e.ToTable("USER");
				e.HasKey(x => x.UserId);
				e.Property(x => x.UserId).HasColumnName("user_id");

				e.Property(x => x.Username)
					.HasColumnName("username")
					.HasMaxLength(512)
					.IsRequired();

				e.Property(x => x.Email)
					.HasColumnName("email")
					.HasMaxLength(512)
					.IsRequired();

				e.Property(x => x.Name)
					.HasColumnName("name")
					.HasMaxLength(512)
					.IsRequired();

				e.Property(x => x.AutoSubscribe).HasColumnName("auto_subscribe");

				e.Property(x => x.PasswordHash)
					.HasColumnName("password_hash")
					.HasMaxLength(512)
					.IsRequired();

				e.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(50);

				e.Property(x => x.BirthDate).HasColumnName("birth_date");

				e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);

				e.Property(x => x.Address).HasColumnName("full_address").HasMaxLength(512);

				e.Property(x => x.Point).HasColumnName("point");

				e.Property(x => x.Isverified).HasColumnName("isverified");

				e.Property(x => x.ProfileImageurl).HasColumnName("profile_imageurl").HasMaxLength(512);

				e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSDATETIME()");

				e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("SYSDATETIME()");
			});
		}
	}
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NZWalks.API.Data
{
    public class NZWalksAuthDbContext : IdentityDbContext
    {
        public NZWalksAuthDbContext(DbContextOptions<NZWalksAuthDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var readerRoleId = "72ae7dfe-ef5f-41bb-9842-4fc10f68894d";
            var writerRoleId = "ed282b4b-b146-4568-94f4-0f996098dcc9";
            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerRoleId,
                    ConcurrencyStamp =readerRoleId,
                    Name="Reader",
                    NormalizedName ="Reader".ToUpper()

                },
                new IdentityRole
                {
                    Id = writerRoleId,
                    ConcurrencyStamp =writerRoleId,
                    Name="Writer",
                    NormalizedName ="Writer".ToUpper()

                }
            };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}

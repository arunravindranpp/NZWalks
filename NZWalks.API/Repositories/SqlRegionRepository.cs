using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class SqlRegionRepository : IRegionRepository
    {
        private readonly NZWalksDbContext dbcontext;

        public SqlRegionRepository(NZWalksDbContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public async Task<Region> CreateAsync(Region region)
        {
            await dbcontext.Regions.AddAsync(region);
            await dbcontext.SaveChangesAsync();
            return region;
        }

        public async Task<Region?> DeleteAsync(Guid id)
        {
            var existRegion =  await dbcontext.Regions.FirstOrDefaultAsync(i => i.Id == id);
            if (existRegion == null)
            {
                return null;
            }
            dbcontext.Regions.Remove(existRegion);
            await dbcontext.SaveChangesAsync();
            return existRegion;
        }

        public async Task<List<Region>> GetAllAsync()
        {
            return await dbcontext.Regions.ToListAsync();
        }

        public async Task<Region?> GetByIdAsync(Guid id)
        {
            return await dbcontext.Regions.FirstOrDefaultAsync(i=>i.Id== id);
        }

        public async Task<Region?> UpdateAsync(Guid id, Region region)
        {
            var existRegion = await dbcontext.Regions.FirstOrDefaultAsync(i=>i.Id ==id);
            if (existRegion == null)
            {
                return null;
            }
            existRegion.Code = region.Code;
            existRegion.Name = region.Name;
            existRegion.RegionImageUrl = region.RegionImageUrl;
            await dbcontext.SaveChangesAsync();
            return existRegion;
        }
    }
}

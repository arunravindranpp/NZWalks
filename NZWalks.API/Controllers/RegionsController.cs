using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using System.Data;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext _dbContext;
        public RegionsController(NZWalksDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var regions = await _dbContext.Regions.ToListAsync();
            return Ok(regions);
        }

        // GET SINGLE REGION (Get Region By ID)
        // GET: https://localhost:portnumber/api/regions/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            //var region = dbContext.Regions.Find(id);
            var region = await _dbContext.Regions.FirstOrDefaultAsync(i => i.Id == id);

            if (region == null)
            {
                return NotFound();
            }

            return Ok(region);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegionRequestDto regionRequestDto)
        {
            var regionDomainModel = new Region
            {
                Code = regionRequestDto.Code,
                Name = regionRequestDto.Name,
                RegionImageUrl = regionRequestDto.RegionImageUrl
            };
            await _dbContext.Regions.AddAsync(regionDomainModel);
            await _dbContext.SaveChangesAsync();

            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Name = regionDomainModel.Name,
                Code = regionDomainModel.Code,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };
            return CreatedAtAction(nameof(GetById), new { id = regionDomainModel.Id }, regionDto);
        }
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RegionRequestDto regionRequestDto)
        {
            var regionDomainModel = _dbContext.Regions.FirstOrDefault(i => i.Id == id);
            if (regionDomainModel == null)
            {
                return NotFound();
            }
            regionDomainModel.Name = regionRequestDto.Name;
            regionDomainModel.Code = regionRequestDto.Code;
            regionDomainModel.RegionImageUrl = regionRequestDto.RegionImageUrl;

            await _dbContext.SaveChangesAsync();
            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Name = regionDomainModel.Name,
                Code = regionDomainModel.Code,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };

            return Ok(regionDto);
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var regionDomainModel = await _dbContext.Regions.FirstOrDefaultAsync(i => i.Id == id);
            if (regionDomainModel == null)
            {
                return NotFound();
            }

            _dbContext.Regions.Remove(regionDomainModel);
            await _dbContext.SaveChangesAsync();
            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Name = regionDomainModel.Name,
                Code = regionDomainModel.Code,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };

            return Ok(regionDto);
        }
    }
}

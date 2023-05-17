using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Data;
using System.Text.Json;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext _dbContext;
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper mapper;
        private readonly ILogger<RegionsController> logger;

        public RegionsController(NZWalksDbContext dbContext, IRegionRepository regionRepository,IMapper mapper,ILogger<RegionsController> logger)
        {
            this._dbContext = dbContext;
            this._regionRepository = regionRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Reader")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //throw new Exception("This is a custom exception");
                logger.LogInformation("GetAll method invoked");
                var regionDomains = await _regionRepository.GetAllAsync();
                var regionDto = mapper.Map<List<RegionDto>>(regionDomains);
                logger.LogInformation($"Finished Getall method with data: {JsonSerializer.Serialize(regionDto)}");
                logger.LogWarning("Test warning");
                logger.LogError("Test Error");
                return Ok(regionDto);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        // GET SINGLE REGION (Get Region By ID)
        // GET: https://localhost:portnumber/api/regions/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            //var region = dbContext.Regions.Find(id);
            var regionDomains = await _regionRepository.GetByIdAsync(id);
            if (regionDomains == null)
            {
                return NotFound();
            }
            var regionDto = mapper.Map<RegionDto>(regionDomains);
            return Ok(regionDto);
        }
        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create([FromBody] RegionRequestDto regionRequestDto)
        {
            var regionDomainModel = mapper.Map<Region>(regionRequestDto);
            regionDomainModel =await _regionRepository.CreateAsync(regionDomainModel);
            var regionDto = mapper.Map<RegionDto>(regionDomainModel);
            return CreatedAtAction(nameof(GetById), new { id = regionDomainModel.Id }, regionDto);
        }
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RegionRequestDto regionRequestDto)
        {

            var regionDomainModel = mapper.Map<Region>(regionRequestDto);
            regionDomainModel =await _regionRepository.UpdateAsync(id, regionDomainModel);
            if(regionDomainModel ==null)
            {
                return NotFound();
            }
            var regionDto = mapper.Map<RegionDto>(regionDomainModel);

            return Ok(regionDto);
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer,Reader")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var regionDomainModel = await _regionRepository.DeleteAsync(id);
            if (regionDomainModel == null)
            {
                return NotFound();
            }
            var regionDto = mapper.Map<RegionDto>(regionDomainModel);

            return Ok(regionDto);
        }
    }
}

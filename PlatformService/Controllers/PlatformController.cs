using AutoMapper;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc;
using PlatformService.ASyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo platformRepository;
        private readonly IMapper mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformController(IPlatformRepo platformRepository, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient)
        {
            this.platformRepository = platformRepository;
            this.mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult <IEnumerable<PlatformReadDto>> getPlatforms()
        {
            Console.WriteLine("Getting Platforms...");

            var platforms = platformRepository.GetAllPlatforms();
            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet("{Id}", Name = "getPlatformById")]
        public ActionResult<PlatformReadDto> getPlatformById(int Id)
        {
            Console.WriteLine("Getting Platform...");

            var platform = platformRepository.GetPlatformById(Id);

            var returnedPlatform = mapper.Map<PlatformReadDto>(platform);

            if(returnedPlatform != null)
            {
                return Ok(returnedPlatform);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            if(platformCreateDto != null)
            {
                var platform = mapper.Map<Platform>(platformCreateDto);

                platformRepository.CreatePlatform(platform);
                platformRepository.SaveChanges();

                var platformReadDto = mapper.Map<PlatformReadDto>(platform);

                try
                {
                    await _commandDataClient.SendPlatformToCommand(platformReadDto);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Could not send message synchronously: {ex.Message}");
                }

                //Send aysnc message
                try
                {
                    var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
                    platformPublishedDto.Event = "Platform_Published";
                    _messageBusClient.PublishNewPlatform(platformPublishedDto);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Could not send message synchronously: {ex.Message}");
                }

                return CreatedAtRoute(nameof(getPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
            }
            return BadRequest();

            


        }
    }
}

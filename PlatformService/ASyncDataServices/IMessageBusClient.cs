using PlatformService.Dtos;

namespace PlatformService.ASyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(PlatformPublishedDto platformPublishDto);
    }
}

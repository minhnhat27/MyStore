using Microsoft.Extensions.Hosting;
using MyStore.Application.ILibrary;

namespace MyStore.Infrastructure.DataSeeding
{
    public class StartUpService(IImageFeatureExtractor imageFeatureExtractor) : IHostedService
    {
        private readonly IImageFeatureExtractor _imageFeatureExtractor = imageFeatureExtractor;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _imageFeatureExtractor.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

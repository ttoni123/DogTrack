using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Service.IDogTrackService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class TrackGenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TrackGenerationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Console: Track Generation Service running at: {DateTimeOffset.Now}");

            using var scope = _serviceProvider.CreateScope();
            var dogTrackService = scope.ServiceProvider.GetRequiredService<IDogTrackService>();
            await dogTrackService.GenerateRaces(5);//add to appsettings as parameter

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);//Appsettings parameter
        }
    }
}
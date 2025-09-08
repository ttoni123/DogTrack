using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Service.IDogTrackService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class RaceResolvingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RaceResolvingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("RaceResolvingService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dogTrackService = scope.ServiceProvider.GetRequiredService<IDogTrackService>();


            var nextRace = await dogTrackService.GetNextRaceStart();

            if (nextRace == null)
            {
                Console.WriteLine("No upcoming races found. Sleeping 1 minute...");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            var resolveTime = nextRace.Value.AddSeconds(-5);
            var delay = resolveTime - DateTime.Now;

            Console.WriteLine(delay);

            if (delay > TimeSpan.Zero)
            {
                Console.WriteLine($"Next race starts at {nextRace}. Will resolve in {delay.TotalSeconds:F0}s.");
                await Task.Delay(TimeSpan.FromSeconds(delay.TotalSeconds), stoppingToken);
            }
            else
            {
                Console.WriteLine($"Race is already due. Resolving now.");
            }

            await dogTrackService.ResolveRaces();
        }
    }
}

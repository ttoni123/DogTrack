using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Service.IDogTrackService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class TicketValidationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TicketValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Console: Ticket validation service running at: {DateTimeOffset.Now}");

            using var scope = _serviceProvider.CreateScope();
            var dogTrackService = scope.ServiceProvider.GetRequiredService<IDogTrackService>();
            await dogTrackService.ValidateTickets();

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);//Could add parameter
        }
    }
}
using DogTrack.Models;

namespace DogTrack.Service.IDogTrackService
{
    public interface IDogTrackService
    {

        Task<decimal> GetCurrentBalance(UserContext context);

        Task<decimal> AddFunds(UserContext context, decimal amount); //Change to request model for validation

        Task<decimal> WithdrawFunds(UserContext context, decimal amount);

        Task<List<Ticket>> GetTickets(UserContext context);

        Task<TicketDetails> GetTicket(UserContext context, TicketRequest request);

        #region Race

        Task<List<Bet>> GetAvailableBets();

        Task<int> MakeBet(UserContext context, BetAddRequest request);

        #endregion Race

        #region BackgroundService

        Task<int> GenerateRaces(int numberOfRaces);

        Task<bool> ValidateTickets();

        Task<DateTime?> GetNextRaceStart();

        Task<bool> ResolveRaces();


        #endregion BackgroundService
    }
}

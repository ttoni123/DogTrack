using DogTrack.Models;
using DogTrack.Models.Enums;

namespace DogTrack.DataAccess.IDogTrackDataAccess
{
    public interface IDogTrackDataAccess
    {
        Task<int> GenerateRaces(int numberOfRaces);

        Task<int> Login(string userName, string password);

        Task<List<Ticket>> GetTickets(int userId);

        Task<TicketDetails?> GetTicket(int userId, int ticketId);

        Task<List<Bet>> GetAvailableBets();

        Task<int> MakeBet(BetAddRequest requests, int userId);

        Task<bool> ValidateBet(BetRequest request);

        Task<bool> UpdateValidTickets();

        Task<List<InvalidTicket>> GetInvalidTickets();

        Task<bool> InvalidateTicket(int ticketId);

        Task<DateTime?> GetNextRaceStart();

        Task<List<Race>> GetUnresolvedRaces();

        Task<bool> SetRaceWinner(Race race);

        Task<bool> UpdateLostTickets();

        Task<List<WonTicket>> GetWinningTickets();

        Task<bool> UpdateTicket(int ticketId, TicketStatus ticketStatus);
    }
}

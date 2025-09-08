using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using DogTrack.DataAccess.DogTrackDataAccess.Mapper;
using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Models;
using DogTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Serilog;
using BetType = DogTrack.Models.Enums.BetType;
using ILogger = Serilog.ILogger;
using InvalidTicket = DogTrack.Models.InvalidTicket;
using Ticket = DogTrack.Models.Ticket;

namespace DogTrack.DataAccess.DogTrackDataAccess
{
    public class DogTrackDataAccess : IDogTrackDataAccess.IDogTrackDataAccess
    {

        private readonly string _connString;
        private readonly int _dbTimeout;
        private readonly XSdbContext _dbContext;
        private readonly ILogger _logger;
        private static Random rnd = new Random();

        public DogTrackDataAccess(IConfiguration configuration) 
        {
            _connString = "server=tbr-w11;database=XSdb;Integrated Security=True;TrustServerCertificate=True"; //move to appsettings

            if (!Int32.TryParse(configuration["DbOptions:TimeoutInSeconds"], out _dbTimeout))
            {
                _dbTimeout = 60;//Create constant insted of magic number
            }

            DbContextOptionsBuilder<XSdbContext> optionsBuilderCommon = new();
            optionsBuilderCommon.UseSqlServer
            (
                _connString,
                sqlServerOptions => sqlServerOptions
                    .CommandTimeout(_dbTimeout)
            );
            optionsBuilderCommon.EnableSensitiveDataLogging(true);

            _dbContext = new XSdbContext(optionsBuilderCommon.Options);

            _logger = Log.Logger.ForContext(typeof(DogTrackDataAccess));
        }

        public async Task<int> GenerateRaces(int numberOfRaces)
        {
            _logger.Information("Generating race...");

            return  await _dbContext.Procedures.InsertRacesAsync(numberOfRaces);
        }

        public async Task<int> Login(string userName, string password) //Password should be hashed
        {
            _logger.Information("Login attempt for " + userName);

            var accountId = await _dbContext.Accounts
                .Where
                (
                    g =>
                        g.Username == userName
                        &&
                        g.Password == password
                )
                .Select
                (
                    g => g.AccountId
                )
                .FirstOrDefaultAsync();

            return accountId;
        }

        public async Task<List<Ticket>> GetTickets(int userId)
        {
            _logger.Information("Get tickets for user: " + userId);

            var dbTickets = _dbContext.Tickets
                .Where
                (
                    g => g.UserId == userId
                );

            var tickets = await Db_Map.Mapper.ProjectTo<Ticket>(dbTickets).ToListAsync();

            return tickets;
        }

        public async Task<TicketDetails?> GetTicket(int userId, int ticketId)
        {
            _logger.Information("Get ticket details for ticket: " + ticketId);

            //I usually create a view for thing like this.
            var ticketDetails = await _dbContext.TicketBets
                .Join(
                    _dbContext.Tickets,
                    tb => tb.TicketId,
                    t => t.TicketId,
                    (tb, t) => new { TicketBet = tb, Ticket = t }
                )
                .GroupBy(x => new { x.Ticket.TicketId, x.Ticket.TicketStatusId })
                .Select(g => new DogTrack.Models.TicketDetails
                {
                    TicketId = g.Key.TicketId,
                    TicketStatus = (Models.Enums.TicketStatus) g.Key.TicketStatusId,
                    Bets = g.Select
                    (
                        x => 
                            new Bet 
                            {
                                BetId = x.TicketBet.TicketBetId,
                                BetType = (BetType) x.TicketBet.AvailableBet.BetTypeId,
                                Odds = x.TicketBet.Odds,
                                ParticipantId = x.TicketBet.AvailableBet.RaceParticipantId,
                                RaceId = x.TicketBet.AvailableBet.RaceParticipant.RaceId
                            })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            return ticketDetails;
        }

        public async Task<List<Bet>> GetAvailableBets()
        {
            _logger.Information("Get available bets");

            var dbBets = _dbContext.AvailableBets
                .Where
                (
                    g => g.RaceParticipant.Race.RaceStart > DateTime.Now.AddMinutes(1) //Excluding the races that are starting soon
                );

            var bets = await Db_Map.Mapper.ProjectTo<Bet>(dbBets).ToListAsync();

            return bets;
        }

        public async Task<int> MakeBet(BetAddRequest requests, int userId)
        {
            _logger.Information("Create new ticket for user " + userId);

            var ticket = await _dbContext.Tickets
                .AddAsync
                (
                    new Entities.Ticket
                    {
                        TicketStatusId = (int)DogTrack.Models.Enums.TicketStatus.Pending,
                        BetAmount = requests.BetAmount,
                        WinAmount = requests.BetAmount * requests.Bets.Aggregate(1m, (acc, bet) => acc * bet.Odds),
                        UserId = userId
                    }
                );

            await _dbContext.SaveChangesAsync();

            if(ticket.Entity.TicketId != 0) 
            {
                foreach(var bet in requests.Bets) 
                {
                    await _dbContext.TicketBets
                        .AddAsync
                        (
                            new TicketBet
                            {
                                AvailableBetId = bet.BetId,
                                Odds = bet.Odds,
                                TicketId = ticket.Entity.TicketId
                            }
                        );
                }

                await _dbContext.SaveChangesAsync();
            }

            return ticket.Entity.TicketId;
        }

        public async Task<bool> ValidateBet(BetRequest request)
        {
            _logger.Information("validate bet " + request.BetId);

            var isValid = await _dbContext.AvailableBets
                .AnyAsync
                (
                    g =>
                        g.RaceParticipantId == request.RaceParticipantId
                        &&
                        g.AvailableBetId == request.BetId
                        &&
                        g.BetTypeId == (int)request.BetType
                        &&
                        g.Odds == request.Odds
                );

            return isValid;
        }

        public async Task<bool> UpdateValidTickets()
        {
            _logger.Information("Update vaild ticket to status Success");

            return await _dbContext.Procedures.ValidateTicketsAsync() > 0;
        }

        public async Task<List<InvalidTicket>> GetInvalidTickets() 
        {
            _logger.Information("Get Invalid tickets");

            var dbInvalidTickets = _dbContext.InvalidTickets;

            var tickets = await Db_Map.Mapper.ProjectTo<InvalidTicket>(dbInvalidTickets).ToListAsync();

            return tickets;
        }

        public async Task<bool> InvalidateTicket(int ticketId)
        {
            var entityTicket = new Entities.Ticket
            {
                TicketId = ticketId,
                TicketStatusId = (int) Models.Enums.TicketStatus.Rejected
            };

            _dbContext.Entry(entityTicket).Property(x => x.TicketStatusId).IsModified = true;

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<DateTime?> GetNextRaceStart() 
        {
            _logger.Information("Get next unresolved race start time");

            var time = await _dbContext.Races
                .Where
                (
                    g => g.RaceWinnerId == null
                )
                .OrderBy
                (
                    g => g.RaceStart
                )
                .Select
                (
                    g => g.RaceStart
                )
                .FirstOrDefaultAsync();

            return time;
        }

        public async Task<List<Models.Race>> GetUnresolvedRaces() 
        {
            var dbRaces = _dbContext.Races
                .Where
                (
                    g =>
                        g.RaceStart < DateTime.Now.AddSeconds(-5)
                        &&
                        g.RaceWinnerId == null
                );

            var races = await Db_Map.Mapper.ProjectTo<Models.Race>(dbRaces).ToListAsync();

            return races;
        }

        public async Task<bool> SetRaceWinner(Models.Race race) 
        {
            _logger.Information("set race winner for race " + race.RaceId);

            var raceParticipants = await _dbContext.RaceParticipants
                .Where
                (
                    g => g.RaceId == race.RaceId
                )
                .Select
                (
                    g => g.RaceParticipantId
                )
                .ToListAsync();

            var winner = rnd.Next(raceParticipants.Count);

            var entityRace = new Entities.Race
            {
                RaceId = race.RaceId,
                RaceWinnerId = raceParticipants[winner]
            };

            _dbContext.Entry(entityRace).Property(x => x.RaceWinnerId).IsModified = true;

            await _dbContext.SaveChangesAsync();

            return await _dbContext.Procedures.UpdateAvailableBetSuccessStatusAsync(race.RaceId) > 0;
        }

        public async Task<bool> UpdateLostTickets() 
        {
            _logger.Information("Update lost tickets");

            return await _dbContext.Procedures.UpdateLostTicketsAsync() > 0;
        }

        public async Task<List<Models.WonTicket>> GetWinningTickets() 
        {
            _logger.Information("Get winning tickets");

            var dbTickets = _dbContext.WonTickets;

            var tickets = await Db_Map.Mapper.ProjectTo<Models.WonTicket>(dbTickets).ToListAsync();

            return tickets;
        }

        public async Task<bool> UpdateTicket(int ticketId, Models.Enums.TicketStatus ticketStatus) 
        {
            var entityTicket = new Entities.Ticket
            {
                TicketId = ticketId,
                TicketStatusId = (int) ticketStatus
            };

            _dbContext.Entry(entityTicket).Property(x => x.TicketStatusId).IsModified = true;

            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}

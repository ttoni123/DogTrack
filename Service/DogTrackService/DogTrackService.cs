using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Models;
using DogTrack.Models.Enums;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace DogTrack.Service.DogTrackService
{
    public class DogTrackService : IDogTrackService.IDogTrackService
    {

        private readonly IDogTrackDataAccess _dataAccess;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public DogTrackService(IDogTrackDataAccess dataAccess, IMemoryCache memoryCache)
        {
            _dataAccess = dataAccess;
            _logger = Log.Logger.ForContext(typeof(DogTrackService));
            _memoryCache = memoryCache;
        }

        #region Wallet
        public async Task<decimal> WithdrawFunds(UserContext context, decimal amount)
        {
            return await Task.FromResult(WalletOperation(context.UserId, amount, false));
        }

        public async Task<decimal> AddFunds(UserContext context, decimal amount)
        {
            return await Task.FromResult(WalletOperation(context.UserId, amount, true));
        }


        public async Task<decimal> GetCurrentBalance(UserContext context)
        {
            return await Task.FromResult(WalletOperation(context.UserId, null, null));
        }
        #endregion Wallet

        #region Ticket

        public async Task<TicketDetails> GetTicket(UserContext context, TicketRequest request)
        {
            _logger.Information("Get ticket: " + request.TicketId);

            var ticket = await _dataAccess.GetTicket(context.UserId, request.TicketId);

            if(ticket == null) 
            {
                throw new Exception("Ticket not found"); //we should implement some more sophisticated way to throw exception :)
            }

            return ticket;
        }

        public async Task<List<Ticket>> GetTickets(UserContext context)
        {
            _logger.Information("Get tickets for account: " + context.UserId);

            var tickets = await _dataAccess.GetTickets(context.UserId);

            return tickets;
        }

        #endregion Ticket

        #region Bets

        public async Task<List<Bet>> GetAvailableBets()
        {
            _logger.Information("Get available bets");

            var bets = await _dataAccess.GetAvailableBets();

            return bets;
        }



        public async Task<int> MakeBet(UserContext context, BetAddRequest request)
        {

            //Missing validation for balance/maxPayment/maxWin/min all
            _logger.Information("Create new bet for user: " + context.UserId);

            List<int> invalidBets = new List<int>();
            foreach(var bet in request.Bets) 
            {
                var isValid = await _dataAccess.ValidateBet(bet);

                if (!isValid) 
                {
                    invalidBets.Add(bet.BetId);
                }
            }

            if (invalidBets.Count > 0)
            {
                throw new Exception("There are invalid bets " + string.Join(", ", invalidBets));
            }

            var result = await _dataAccess.MakeBet(request, context.UserId);

            if (result > 0)
            {
                WalletOperation(context.UserId, request.BetAmount, false);
            }

            return result;
        }

        #endregion Bets

        #region Background Services

        public async Task<int> GenerateRaces(int numberOfRaces) 
        {
            return await _dataAccess.GenerateRaces(numberOfRaces);
        }

        public async Task<bool> ValidateTickets()
        {
            _logger.Information("Update valid ticket to success");

            await _dataAccess.UpdateValidTickets();

            var invalidTicket = await _dataAccess.GetInvalidTickets();

            foreach(var ticket in invalidTicket) 
            {
                WalletOperation(ticket.UserId, ticket.BetAmount, true);

                await _dataAccess.UpdateTicket(ticket.TicketId, TicketStatus.Rejected);
            }

            return true;
        }

        public async Task<DateTime?> GetNextRaceStart() 
        {
            _logger.Information("Get next race to be resolved");

            return await _dataAccess.GetNextRaceStart();
        }

        public async Task<bool> ResolveRaces()
        {
            _logger.Information("Get Unresolved races");

            var races = await _dataAccess.GetUnresolvedRaces();

            foreach (var race in races) 
            {
                await _dataAccess.SetRaceWinner(race);
            }

            //Update all tickets that lost
            await _dataAccess.UpdateLostTickets();

            //I have to do it like this since the task asked for InMemory wallet
            //Get all tickets that won
            var winningTickets = await _dataAccess.GetWinningTickets();

            foreach(var ticket in winningTickets) 
            {
                await _dataAccess.UpdateTicket(ticket.TicketId, TicketStatus.Won);

                WalletOperation(ticket.UserId, ticket.WinAmount, true);
            }

            return await Task.FromResult(true);
        }

        #endregion Background Services

        //Abstracted to make the above code more readable
        //If amoun and isAdd is null just return state - else add/subtract the amount
        //Default amount is 100
        //Possible issues with the expiring cache, but I'm looking past that for this :)
        private decimal WalletOperation(int userId, decimal? amount, bool? isAdd) 
        {
            string cacheKey = userId.ToString();

            string? cachedValue = null;

            if (_memoryCache.TryGetValue(cacheKey, out cachedValue))
            {
                decimal balance = Decimal.Parse(cachedValue!);
                if(amount != null)
                {
                    balance = (decimal)((isAdd == true) ? balance + amount : balance - amount); 
                }
                _memoryCache.Set(cacheKey, balance.ToString());

                return balance;
            }
            
            if(amount != null)
            {
                cachedValue = (isAdd == true ? 100 + amount : 100 - amount).ToString();
            }
            else 
            {
                cachedValue = "100";
            }

            _memoryCache.Set(cacheKey, cachedValue);

            return Decimal.Parse(cachedValue!);
        }

    }
}

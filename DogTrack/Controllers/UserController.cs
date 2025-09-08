using DogTrack.Controllers.Base;
using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using DogTrack.Models;
using DogTrack.Service.IDogTrackService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticket = DogTrack.Models.Ticket;

namespace DogTrack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseController
    {

        public UserController
        (
            ILogger<RaceController> logger,
            IHttpContextAccessor httpContextAccessor,
            UserContext context,
            IDogTrackService service
        )
        : base(logger, httpContextAccessor, context, service)
        {

        }



        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public async Task<decimal?> CurrentBalance()
        {
            return await service.GetCurrentBalance(this.CallerContext);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize]
        public async Task<decimal?> AddFunds(decimal amount)
        {
            return await service.AddFunds(this.CallerContext, amount);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize]
        public async Task<decimal?> WithdrawFunds(decimal amount)
        {
            return await service.WithdrawFunds(this.CallerContext, amount);
        }


        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public async Task<List<Ticket>> Tickets()
        {
            return await service.GetTickets(this.CallerContext);
        }

        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public async Task<Ticket> Ticket([FromQuery] TicketRequest request)
        {
            return await service.GetTicket(this.CallerContext, request);
        }
    }

}

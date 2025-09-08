using DogTrack.Controllers.Base;
using DogTrack.Models;
using DogTrack.Service.IDogTrackService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DogTrack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RaceController : BaseController
    {

        public RaceController
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
        [Route("/")]
        public async Task<string> ApplicationInfo()
        {
            return await Task.FromResult("Dog Track App");
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<List<Bet>> AvailableBets()
        {
            return await service.GetAvailableBets();
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize]
        public async Task<int> Bet([FromBody] BetAddRequest request)
        {
            return await service.MakeBet(CallerContext, request);
        }


    }

}

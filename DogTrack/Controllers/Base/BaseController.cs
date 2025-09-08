using DogTrack.Models;
using DogTrack.Service.IDogTrackService;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace DogTrack.Controllers.Base
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> logger;
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly UserContext hostContext;
        protected readonly IDogTrackService service;

        protected BaseController
        (
            ILogger<BaseController> logger,
            IHttpContextAccessor httpContextAccessor,
            UserContext hostContext,
            IDogTrackService service
        )
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.hostContext = hostContext;
            this.service = service;
        }


        protected int UserId
        {
            get
            {
                var authorizedUserId = ControllerContext?.HttpContext?.User?.Claims?
                    .FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (!int.TryParse(authorizedUserId, out int userId))
                {
                    throw new ArgumentException("InvalidUser", "UserId");
                }

                return userId;
            }
        }


        public UserContext CallerContext
        {
            get
            {
                return new UserContext()
                {
                    UserId = UserId
                };
            }
        }
        public MethodInfo ActionInfo
        {
            get
            {
                return ControllerContext.ActionDescriptor.MethodInfo;
            }
        }

    }
}
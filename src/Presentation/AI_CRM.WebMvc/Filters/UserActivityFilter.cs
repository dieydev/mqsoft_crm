using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using AI_CRM.WebMvc.Services;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Filters
{
    public class UserActivityFilter : IAsyncActionFilter
    {
        private readonly IUserActivityService _userActivityService;

        public UserActivityFilter(IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity != null && context.HttpContext.User.Identity.IsAuthenticated)
            {
                var userIdStr = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    _userActivityService.UpdateUserActivity(userId);
                }
            }
            await next();
        }
    }
}

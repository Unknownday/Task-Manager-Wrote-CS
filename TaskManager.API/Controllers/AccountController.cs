using Common.Models;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute; 

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;

        public AccountController(IConfiguration configuration)
        {
            _userService = new UserService(configuration);
            
        }

        /// <summary>
        /// Создание нового токена пользователя
        /// </summary>
        /// <returns>Новый токен пользователя</returns>
        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] AuthorizationModel authorization)
        {

            UserModel? currentUser = await _userService.GetUser(authorization.Email, authorization.Password);

            if (currentUser == null) { return NotFound("User not found"); }

            currentUser.LastLoginDate = DateTime.Now;

            await _userService.UpdateUser(currentUser);

            string accessToken = _userService.GetAcessToken(currentUser.Id);

            string refreshToken = _userService.GetRefreshToken(currentUser.Id);

            var response = new
            {
                access_token = accessToken,
                refresh_token = refreshToken
            };

            _userService.SaveTokens(currentUser.Id, accessToken, refreshToken);

            return Ok(response);
        }
    }
}

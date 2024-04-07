using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;
using Common.Models;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(IConfiguration configuration)
        {
            _userService = new UserService(configuration);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModel userModel)
        {
            var result = _userService.Create(userModel);

            if (result.Message != null) return BadRequest(result.Message);

            var userModelForReturn = new ShortUserModel((UserModel)result.Result);

            return Created("localhost/api/user", userModelForReturn);
        }

        [HttpPatch]
        public IActionResult UpdateUser([FromBody] UserModel userModel)
        {
            (bool valid, int id) userId = _userService.TryGetId(Request).Result;
            if (userId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            var result = _userService.Update(userId.id, userModel);

            if (result.Message != null) return BadRequest(result.Message);

            var userModelForReturn = new ShortUserModel((UserModel)result.Result);

            return Ok(userModelForReturn);
        }

        [HttpDelete]
        public IActionResult DeleteUser()
        {

            (bool valid, int id) userId = _userService.TryGetId(Request).Result;
            if (userId.valid == false)
            {
                return BadRequest("Unable to delete user due to user session expired! Please re-authorize and try again!");
            }

            var result = _userService.Delete(userId.id);

            if (result.Message == null) return BadRequest(result.Message);
            return NoContent();
        }

        [HttpGet]
        public IActionResult GetUser()
        {

            (bool valid, int id) userId = _userService.TryGetId(Request).Result; 
            if (userId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }
            var result = _userService.Get(userId.id);
            if (result.Message != null) return NotFound(result.Message);            
            return Ok(result.Result);
        }
    }
}

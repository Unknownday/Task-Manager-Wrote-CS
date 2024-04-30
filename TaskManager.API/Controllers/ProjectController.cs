using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;
using Common.Models;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        private readonly UserService _userService;

        public ProjectController(IConfiguration configuration)
        {
            _projectService = new ProjectService(configuration);
            _userService = new UserService(configuration);
        }

        [HttpGet]
        public IActionResult Get(int projectID) 
        { 
            if (projectID < 0 || projectID > int.MaxValue) return BadRequest("Project ID can not be less then 0 or more than max value");

            var getResult = _projectService.Get(projectID);

            if (getResult.Status == ResultStatus.Error) return NotFound(getResult.Message);

            return Ok(getResult.Result);
        }

        [HttpGet("ByUserId")]
        public IActionResult GetUsersProjects(int userId)
        {
            if (userId < 0) return BadRequest("Project ID can not be less then 0");

            var getResult = _projectService.GetUserProjects(userId);

            if (getResult.Status == ResultStatus.Error) return NotFound(getResult.Message);

            return Ok(getResult.Result);
        }

        [HttpGet("Desks")]
        public IActionResult GetProjectDesks(int projectID) 
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectID);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (projectID < 0 || projectID > int.MaxValue) return BadRequest("Project ID can not be less then 0 or more than max value");

            var deskGetResult = _projectService.GetProjectDesks(projectID);
            
            if (deskGetResult.Status == ResultStatus.Error) return NotFound(deskGetResult.Message);

            return Ok(deskGetResult.Result);
        }

        [HttpDelete]
        public IActionResult Delete(int projectID)
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectID);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            var deleteResult = _projectService.Delete(projectID);

            if (deleteResult.Status == ResultStatus.Error) { return  NotFound(deleteResult.Message); }

            return NoContent();
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] ProjectModel model)
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, model.Id);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (model == null) return BadRequest("Project model can not be empty"); 

            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Project ID can not be less then 0 or more than max value");

            var patchResult = _projectService.Patch(model.Id, model);

            if (patchResult.Status == ResultStatus.Error) return NotFound(patchResult.Message);

            return Ok(patchResult.Result);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ProjectModel model)
        {
            var createResult = _projectService.Create(model);

            if (createResult.Status == ResultStatus.Error) return BadRequest(createResult.Message);

            return Created("", createResult.Result);
        }

        [HttpPost("AddUserToProject")]
        public IActionResult AddUserToProject(int userId, int projectId)
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectId);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (userId < 0 || userId > int.MaxValue) { return BadRequest($"userId can not be less than 0 or more then {int.MaxValue}"); }

            if (projectId < 0 || projectId > int.MaxValue) { return BadRequest($"projectId can not be less than 0 or more then {int.MaxValue}"); }

            var addUserResult = _projectService.AddUserToProject(userId, projectId);

            if (addUserResult.Status == ResultStatus.Error) return BadRequest(addUserResult.Message);

            return Ok();

        }

        [HttpDelete("DeleteUserFromProject")]
        public IActionResult DeleteUserFromProject(int userId, int projectId)
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectId);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (userId < 0 || userId > int.MaxValue) { return BadRequest($"userId can not be less than 0 or more then {int.MaxValue}"); }

            if (projectId < 0 || projectId > int.MaxValue) { return BadRequest($"projectId can not be less than 0 or more then {int.MaxValue}"); }

            var deleteUserResult = _projectService.DeleteUserFromProject(userId, projectId);

            if (deleteUserResult.Status == ResultStatus.Error) return BadRequest(deleteUserResult.Message);

            return Ok();

        }

        [HttpPost("AddDeskToProject")]
        public IActionResult AddDeskToProject([FromBody] int projectId, int deskId)
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectId);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (deskId < 0 || deskId > int.MaxValue) { return BadRequest($"deskId can not be less than 0 or more then {int.MaxValue}"); }

            if (projectId < 0 || projectId > int.MaxValue) { return BadRequest($"projectId can not be less than 0 or more then {int.MaxValue}"); }

            var addUserResult = _projectService.AddDeskToProject(deskId, projectId);

            if (addUserResult.Status == ResultStatus.Error) return BadRequest(addUserResult.Message);

            return Ok();
        }

        [HttpDelete("DeleteDeskFromProject")]
        public IActionResult DeleteDeskFromProject([FromBody]int projectId, int deskId) 
        {
            (bool valid, int id) ownerId = _userService.TryGetId(Request).Result;

            if (ownerId.valid == false)
            {
                return BadRequest("User tokens expired! Please re-authorize!");
            }

            bool isOwner = _projectService.IsOwner(ownerId.id, projectId);

            if (isOwner == false) { return BadRequest("Access denied!"); }

            if (deskId < 0 || deskId > int.MaxValue) { return BadRequest($"deskId can not be less than 0 or more then {int.MaxValue}"); }

            if (projectId < 0 || projectId > int.MaxValue) { return BadRequest($"projectId can not be less than 0 or more then {int.MaxValue}"); }

            var deleteUserResult = _projectService.DeletDeskFromProject(deskId, projectId);

            if (deleteUserResult.Status == ResultStatus.Error) return BadRequest(deleteUserResult.Message);

            return Ok();
        }
    }
}

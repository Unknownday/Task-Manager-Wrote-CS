using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;
using Common.Models;
using Microsoft.Identity.Client;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(IConfiguration configuration)
        {
            _projectService = new ProjectService(configuration);
        }

        [HttpGet("ById")]
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

        [HttpGet]
        public IActionResult GetProjectDesks(int projectID) 
        {
            if (projectID < 0 || projectID > int.MaxValue) return BadRequest("Project ID can not be less then 0 or more than max value");

            var deskGetResult = _projectService.GetProjectDesks(projectID);
            
            if (deskGetResult.Status == ResultStatus.Error) return NotFound(deskGetResult.Message);

            return Ok(deskGetResult.Result);
        }

        [HttpDelete]
        public IActionResult Delete(int projectID)
        {
            var deleteResult = _projectService.Delete(projectID);

            if (deleteResult.Status == ResultStatus.Error) { return  NotFound(deleteResult.Message); }

            return NoContent();
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] ProjectModel model)
        {
            if (model == null) return BadRequest("Project model can not be empty"); 

            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Project ID can not be less then 0 or more than max value");

            var patchResult = _projectService.Update(model.Id, model);

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
    }
}

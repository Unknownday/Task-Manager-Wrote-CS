using Common.Models;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController(IConfiguration configuration) : ControllerBase
    {
        private readonly TaskService _taskService = new TaskService(configuration);

        [HttpGet]
        public IActionResult Get(int taskId)
        {
            if (taskId < 0 || taskId > int.MaxValue) return BadRequest("Task ID can not be less then 0 or more than max value");

            var getResult = _taskService.Get(taskId);

            if (getResult.Status == ResultStatus.Error) return NotFound(getResult.Message);

            return Ok(getResult.Result);
        }

        [HttpDelete]
        public IActionResult Delete(int taskId)
        {
            var deleteResult = _taskService.Delete(taskId);

            if (deleteResult.Status == ResultStatus.Error) { return NotFound(deleteResult.Message); }

            return NoContent();
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] TaskModel model)
        {
            if (model == null) return BadRequest("Task model can not be empty");

            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Task ID can not be less then 0 or more than max value");

            var patchResult = _taskService.Patch(model.Id, model);

            if (patchResult.Status == ResultStatus.Error) return NotFound(patchResult.Message);

            return Ok(patchResult.Result);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TaskModel model)
        {
            if (model == null) return BadRequest("Task model can not be empty");

            var createResult = _taskService.Create(model);

            if (createResult.Status == ResultStatus.Error) return BadRequest(createResult.Message);

            return Created("", createResult.Result);
        }
    }
}
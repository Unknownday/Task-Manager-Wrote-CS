using Common.Models;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Models.Services;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeskController(IConfiguration configuration) : ControllerBase
    {
        private readonly DeskService _deskService = new DeskService(configuration);

        [HttpGet]
        public IActionResult Get(int deskId)
        {
            if (deskId < 0 || deskId > int.MaxValue) return BadRequest("Desk ID can not be less then 0 or more than max value");

            var getResult = _deskService.Get(deskId);

            if (getResult.Status == ResultStatus.Error) return NotFound(getResult.Message);

            return Ok(getResult.Result);
        }

        [HttpDelete]
        public IActionResult Delete(int projectID)
        {
            var deleteResult = _deskService.Delete(projectID);

            if (deleteResult.Status == ResultStatus.Error) { return NotFound(deleteResult.Message); }

            return NoContent();
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] DeskModel model)
        {
            if (model == null) return BadRequest("Desk model can not be empty");

            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Desk ID can not be less then 0 or more than max value");

            var patchResult = _deskService.Update(model.Id, model);

            if (patchResult.Status == ResultStatus.Error) return NotFound(patchResult.Message);

            return Ok(patchResult.Result);
        }

        [HttpPost]
        public IActionResult Create([FromBody] DeskModel model)
        {
            if (model == null) return BadRequest("Desk model can not be empty");

            var createResult = _deskService.Create(model);

            if (createResult.Status == ResultStatus.Error) return BadRequest(createResult.Message);

            return Created("", createResult.Result);
        }
    }
}

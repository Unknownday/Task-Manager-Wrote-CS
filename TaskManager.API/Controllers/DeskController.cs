using Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Desk ID can not be less than 0 or more than max value");

            var patchResult = _deskService.Patch(1, model);

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

        [HttpPost("AddColumn")]
        public IActionResult AddColumn([FromBody] DeskColumnModel model)
        {
            if (model == null) return BadRequest("Desk column cannot be null!");

            var addColumnResult = _deskService.AddColumnToDesk(model.Value, model.DeskId);

            if (addColumnResult.Status == ResultStatus.Error) return BadRequest(addColumnResult.Message);

            return Created("", addColumnResult.Result);
        }

        [HttpDelete("DeleteColumn")]
        public IActionResult RemoveColumn([FromBody] int columnId)
        {
            if (columnId < 0 || columnId > int.MaxValue) return BadRequest("Column id can not be less than 0 or more than max value");

            var deleteColumnResult = _deskService.DeleteColumnFromDesk(columnId);

            if (deleteColumnResult.Status == ResultStatus.Error) return BadRequest(deleteColumnResult.Message);

            return Ok(deleteColumnResult.Result);
        }

        [HttpGet("GetColumn")]
        public IActionResult GetColumn(int columnId)
        {
            if (columnId < 0 || columnId > int.MaxValue) return BadRequest("Column id can not be less than 0 or more than max value");

            var getColumnResult = _deskService.GetDeskColumn(columnId);

            if (getColumnResult.Status == ResultStatus.Error) return BadRequest(getColumnResult.Message);

            return Ok(getColumnResult.Result);
        }

        [HttpPatch("PatchColumn")]
        public IActionResult PatchColumn([FromBody] DeskColumnModel model)
        {
            if (model.Id < 0 || model.Id > int.MaxValue) return BadRequest("Column id can not be less than 0 or more than max value");

            var getColumnResult = _deskService.PatchDeskColumn(model);

            if (getColumnResult.Status == ResultStatus.Error) return BadRequest(getColumnResult.Message);

            return Ok(getColumnResult.Result);
        }
    }
}

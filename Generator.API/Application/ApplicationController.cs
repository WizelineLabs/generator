using Microsoft.AspNetCore.Mvc;
using Reusable.Rest.Implementations.SS;

namespace Generator.API.Application;

[ApiController]
[Route("[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly ApplicationLogic _logic;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(ILogger<ApplicationController> logger, ApplicationLogic logic)
    {
        _logger = logger;
        _logic = logic;
    }

    [HttpGet, Route("/Application/{Id}")]
    public IActionResult GetApplicationById(long Id)
    {
        var application = _logic.GetById(Id);
        return Ok(application);
    }

    [HttpGet, Route("/Application/")]
    public IActionResult GetAllApplications()
    {
        var entities = _logic.GetAll();
        return Ok(entities);
    }

    [HttpPost, Route("/Application")]
    public IActionResult InsertApplication(InsertApplication request)
    {
        var entity = request;
        _logic.Add(entity);
      
        return Ok(_logic.GetById(entity.Id));
    }

    [HttpPut, Route("/Application/{Id}")]
    public IActionResult UpdateApplication([FromBody] UpdateApplication request)
    {
        var entity = request;
        _logic.Update(entity);
       
        return Ok(_logic.GetById(entity.Id));
    }
    [HttpDelete, Route("/Application/{id}")]
    public IActionResult DeleteApplication(long id)
    {
        _logic.RemoveById(id);
       
        return Ok();
    }
}
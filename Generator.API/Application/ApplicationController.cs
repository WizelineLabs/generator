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
        //return WithDb(db => Logic.GetById(request.Id));
        return Ok(application);
    }

    [HttpGet, Route("/Application/")]
    public IActionResult GetAllApplications()
    {
        //return WithDb(db => Logic.GetAll());
        return Ok();
    }

    [HttpPut, Route("/Application")]
    public IActionResult InsertApplication(InsertApplication request)
    {
        var entity = request;
        _logic.Add(entity);
            // return InTransaction(db =>
            // {
            //     Logic.Add(entity);
            //     return new CommonResponse(Logic.GetById(entity.Id));
            // });
        return Ok( _logic.GetById(entity.Id));
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Generator.API.Application;

[ApiController]
[Route("[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(ILogger<ApplicationController> logger)
    {
        _logger = logger;
    }

    [HttpGet, Route("/Application/{Id}")]
    public IActionResult GetApplicationById(long Id)
    {
        //return WithDb(db => Logic.GetById(request.Id));
        return Ok(Id);
    }

    [HttpGet, Route("/Application/{Id}")]
    public IActionResult GetAllApplications()
    {
        //return WithDb(db => Logic.GetAll());
        return Ok();
    }

    [HttpPut, Route("/Application")]
    public IActionResult InsertApplication(InsertApplication request)
    {
        var entity = request.ConvertTo<Application>();
            return InTransaction(db =>
            {
                Logic.Add(entity);
                return new CommonResponse(Logic.GetById(entity.Id));
            });
        return Ok(Id);
    }
}
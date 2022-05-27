using Generator.API.Application;
using Microsoft.AspNetCore.Mvc;
using Reusable.Rest;
using Reusable.Rest.Implementations.SS;

namespace Generator.API.Controllers;

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
        try
        {
            var application = _logic.GetById(Id);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpGet, Route("/Application/")]
    public IActionResult GetAllApplications()
    {
        try
        {
            var entities = _logic.GetAll();
            return Ok(entities);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application")]
    public IActionResult InsertApplication(InsertApplication request)
    {
        try
        {
            var entity = request;
            _logic.Add(entity);            
            return Ok(_logic.GetById(entity.Id));
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPut, Route("/Application/{Id}")]
    public IActionResult UpdateApplication([FromBody] UpdateApplication request)
    {
        try
        {
            var entity = request;
            _logic.Update(entity);

            return Ok(_logic.GetById(entity.Id));
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }
    [HttpDelete, Route("/Application/{id}")]
    public IActionResult DeleteApplication(long id)
    {
        try
        {
            _logic.RemoveById(id);

            return Ok();
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application/CreateMainDefinition")]
    public IActionResult CreateMainDefinition(InsertApplication request)
    {
        try
        {
            var application = _logic.CreateMainDefinition(request);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application/CreateEntity")]
    public IActionResult CreateEntity(CreateEntity request)
    {
        try
        {
            var application = _logic.CreateEntity(request.Name!, request.Application!);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application/CreateGateway")]
    public IActionResult CreateGateway(CreateGateway request)
    {
        try
        {
            var application = _logic.CreateGateway(request.Name!, request.Entity!, request.Application!);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application/CreateComponent")]
    public IActionResult CreateComponent(CreateComponent request)
    {
        try
        {
            var application = _logic.CreateComponent(request.Name!, request.Application!);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpPost, Route("/Application/CreateFrontend")]
    public IActionResult CreateFrontend(CreateFrontend request)
    {
        try
        {
            var application = _logic.CreateFrontend(request.Name!, request.Application!);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost, Route("/Application/CreatePage")]
    public IActionResult CreatePage(CreatePage request)
    {
        try
        {
            var application = _logic.CreatePage(request.Name!, request.FrontendName!, request.Application!);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500,e.Message);            
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }                
    }

    [HttpGet, Route("/Application/GetMainDefinition/{appName}")]
    public IActionResult GetMainDefinition(string appName)
    {
        try
        {
            var application = _logic.GetMainDefinition(appName);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet, Route("/Application/GetComponentsInApplication/{appName}")]
    public IActionResult GetComponentsInApplication(string appName)
    {
        try
        {
            var application = _logic.GetComponentsInApplication(appName);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet, Route("/Application/GetEntitiesInApplication/{appName}")]
    public IActionResult GetEntitiesInApplication(string appName)
    {
        try
        {
            var application = _logic.GetEntitiesInApplication(appName);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet, Route("/Application/GetFrontendsInApplication/{appName}")]
    public IActionResult GetFrontendsInApplication(string appName)
    {
        try
        {
            var application = _logic.GetFrontendsInApplication(appName);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet, Route("/Application/GetPagesInApplicationAndFrontend/{appName}/{frontendName}")]
    public IActionResult GetPagesInApplicationAndFrontend(string appName, string frontendName)
    {
        try
        {
            var application = _logic.GetPagesInApplicationAndFrontend(appName, frontendName);
            return Ok(application);
        }
        catch (KnownError e)
        {
            return StatusCode(500, e.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}
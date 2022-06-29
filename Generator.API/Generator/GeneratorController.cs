using Microsoft.AspNetCore.Mvc;

namespace Generator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GeneratorController : ControllerBase
{
    private readonly ILogger<GeneratorController> _logger;
    private readonly GeneratorLogic _logic;

    public GeneratorController(ILogger<GeneratorController> logger, GeneratorLogic logic)
    {
        _logger = logger;
        _logic = logic;
    }

    [HttpGet, Route("/Generator/ClearCache")]
    public IActionResult ClearCache()
    {
        return Ok();
    }

    [HttpPost, Route("/Generator/RunApplication/{ApplicationName}")]
    public IActionResult RunApplication(string ApplicationName, bool Force)
    {
        try
        {
            return Ok(_logic.RunApplication(ApplicationName, Force));
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

    [HttpPost, Route("/Generator/RunWorkspace/{ApplicationName}")]
    public IActionResult RunWorkspace(string ApplicationName, bool Force)
    {
        try
        {
            return Ok(_logic.RunWorkspace(ApplicationName, Force));
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

    [HttpPost, Route("/Generator/RunGateway/{ApplicationName}/{GatewayName}")]
    public IActionResult RunGateway(string ApplicationName, string GatewayName, bool Force)
    {
        return Ok(ApplicationName);
    }

    [HttpPost, Route("/Generator/RunBackend")]
    public IActionResult RunBackend(string ApplicationName, bool Force)
    {
        return Ok(ApplicationName);
    }

    [HttpPost, Route("/Generator/RunFrontends/{ApplicationName}")]
    public IActionResult RunFrontends(string ApplicationName, bool Force)
    {
        try
        {
            return Ok(_logic.RunFrontends(ApplicationName, Force));
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

    [HttpPost, Route("/Generator/RunEntity/{ApplicationName}/{EntityName}")]
    public IActionResult RunEntity(string ApplicationName, string EntityName, bool Force)
    {
        return Ok(new { ApplicationName, EntityName });
    }

    [HttpPost, Route("/Generator/RunPages/{ApplicationName}/{FrontendName}")]
    public IActionResult RunPages(string ApplicationName, string FrontendName, bool Force)
    {
        return Ok(new { ApplicationName, FrontendName });
    }

    [HttpPost, Route("/Generator/RunComponent/{ApplicationName}/{ComponentName}")]
    public IActionResult RunComponent(string ApplicationName, string ComponentName, bool Force)
    {
        return Ok(new { ApplicationName, ComponentName });
    }

    [HttpPost, Route("/Generator/RunComponents/{ApplicationName}/{FrontendName}")]
    public IActionResult RunComponents(string ApplicationName, string FrontendName, bool Force)
    {
        return Ok(new { ApplicationName, FrontendName });
    }
}

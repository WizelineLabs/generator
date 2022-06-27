using Microsoft.AspNetCore.Mvc;

namespace Generator.API;

[ApiController]
[Route("[controller]")]
public class DiffController : ControllerBase
{
    private readonly DiffLogic _logic;
    private readonly ILogger<DiffController> _logger;

    public DiffController(ILogger<DiffController> logger, DiffLogic logic)
    {
        _logger = logger;
        _logic = logic;
    }

    #region Endpoints - Specific

    [HttpGet]
    [Route("/DiffModel")]
    public IActionResult GetDiffModel(DiffModel model)
    {
        try
        {
            return Ok(_logic.Diff(model));
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

    [HttpPost]
    [Route("/CopyToRight")]
    public IActionResult PostCopyToRight(DiffCopyToRight request)
    {
        try
        {
            return Ok(_logic.CopyToRight(request.File!, request.SelectedLine!));
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

    [HttpPost]
    [Route("/CopyToLeft")]
    public IActionResult PostCopyToLeft(DiffCopyToLeft request)
    {
        try
        {
            return Ok(_logic.FeedbackGenerator(request.File!, request.SelectedLine!));
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

    [HttpPost]
    [Route("/CopyToAllApps")]
    public IActionResult PostCopyToAllApps(DiffCopyToAllApps request)
    {
        try
        {
            return Ok(_logic.CopyToAllApps(request.File!, request.SelectedLine!));
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

    [HttpPost]
    [Route("/IgnoreForAllApps")]
    public IActionResult PostIgnoreForAllApps(DiffIgnoreForAllApps request)
    {
        try
        {
            return Ok(_logic.IgnoreForAllApps(request.File!, request.SelectedLine!));
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

    [HttpPost]
    [Route("/IgnoreForApp")]
    public IActionResult PostIgnoreForApp(DiffIgnoreForApp request)
    {
        try
        {
            return Ok(_logic.IgnoreForApp(request.File!, request.SelectedLine!));
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

    [HttpPost]
    [Route("/CopyFile")]
    public IActionResult PostCopyFile(DiffCopyFile request)
    {
        try
        {
            _logic.CopyFile(request.FromPath!, request.ToPath!);
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

    #endregion
}

#region Specific

public class DiffModel : Archive { }

public class DiffCopyToRight
{
    public Archive? File { get; set; }
    public DiffLine? SelectedLine { get; set; }
}

public class DiffCopyToLeft
{
    public Archive? File { get; set; }
    public DiffLine? SelectedLine { get; set; }
}

public class DiffCopyToAllApps
{
    public Archive? File { get; set; }
    public DiffLine? SelectedLine { get; set; }
}

public class DiffIgnoreForAllApps
{
    public Archive? File { get; set; }
    public DiffLine? SelectedLine { get; set; }
}

public class DiffIgnoreForApp
{
    public Archive? File { get; set; }
    public DiffLine? SelectedLine { get; set; }
}

public class DiffCopyFile
{
    public string? FromPath { get; set; }
    public string? ToPath { get; set; }
}

#endregion

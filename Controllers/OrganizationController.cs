using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly OrgHierarchyService _orgService;
    public OrganizationController(OrgHierarchyService orgService)
{
    _orgService = orgService;
}

    [HttpGet("all")]
    public IActionResult GetAllNodes()
    {
        var allNodes = _orgService.GetAllNodes();
        return Ok(allNodes);
    }


    [HttpPut("rename")]
    public IActionResult RenameHospital([FromQuery] string oldName, [FromQuery] string newName)
    {

        Console.WriteLine($"Rename request: oldName={oldName}, newName={newName}");

        bool success = _orgService.RenameHospital(oldName, newName);
        return success ? Ok("Hospital renamed.") : NotFound("Hospital not found.");
    }

    [HttpPost("add")]
    public IActionResult AddRecord([FromQuery] string parent, [FromQuery] string child)
    {
        _orgService.AddRecord(parent, child);
        return Ok("Record added.");
    }
}
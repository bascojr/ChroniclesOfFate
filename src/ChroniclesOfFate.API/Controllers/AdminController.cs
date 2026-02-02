using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChroniclesOfFate.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ============ Dashboard ============

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    // ============ Lookups ============

    [HttpGet("lookups/storybooks")]
    public async Task<IActionResult> GetStorybookLookup()
    {
        var lookup = await _adminService.GetStorybookLookupAsync();
        return Ok(lookup);
    }

    [HttpGet("lookups/skills")]
    public async Task<IActionResult> GetSkillLookup()
    {
        var lookup = await _adminService.GetSkillLookupAsync();
        return Ok(lookup);
    }

    [HttpGet("lookups/enemies")]
    public async Task<IActionResult> GetEnemyLookup()
    {
        var lookup = await _adminService.GetEnemyLookupAsync();
        return Ok(lookup);
    }

    [HttpGet("lookups/events")]
    public async Task<IActionResult> GetRandomEventLookup()
    {
        var lookup = await _adminService.GetRandomEventLookupAsync();
        return Ok(lookup);
    }

    // ============ Training Scenarios ============

    [HttpGet("training-scenarios")]
    public async Task<IActionResult> GetAllTrainingScenarios()
    {
        var scenarios = await _adminService.GetAllTrainingScenariosAsync();
        return Ok(scenarios);
    }

    [HttpGet("training-scenarios/{id}")]
    public async Task<IActionResult> GetTrainingScenario(int id)
    {
        var scenario = await _adminService.GetTrainingScenarioAsync(id);
        if (scenario == null)
            return NotFound();
        return Ok(scenario);
    }

    [HttpPost("training-scenarios")]
    public async Task<IActionResult> CreateTrainingScenario([FromBody] CreateTrainingScenarioDto dto)
    {
        try
        {
            var scenario = await _adminService.CreateTrainingScenarioAsync(dto);
            return CreatedAtAction(nameof(GetTrainingScenario), new { id = scenario.Id }, scenario);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("training-scenarios/{id}")]
    public async Task<IActionResult> UpdateTrainingScenario(int id, [FromBody] UpdateTrainingScenarioDto dto)
    {
        try
        {
            var scenario = await _adminService.UpdateTrainingScenarioAsync(id, dto);
            if (scenario == null)
                return NotFound();
            return Ok(scenario);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("training-scenarios/{id}")]
    public async Task<IActionResult> DeleteTrainingScenario(int id)
    {
        var result = await _adminService.DeleteTrainingScenarioAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ============ Enemies ============

    [HttpGet("enemies")]
    public async Task<IActionResult> GetAllEnemies()
    {
        var enemies = await _adminService.GetAllEnemiesAsync();
        return Ok(enemies);
    }

    [HttpGet("enemies/{id}")]
    public async Task<IActionResult> GetEnemy(int id)
    {
        var enemy = await _adminService.GetEnemyAsync(id);
        if (enemy == null)
            return NotFound();
        return Ok(enemy);
    }

    [HttpPost("enemies")]
    public async Task<IActionResult> CreateEnemy([FromBody] CreateEnemyDto dto)
    {
        try
        {
            var enemy = await _adminService.CreateEnemyAsync(dto);
            return CreatedAtAction(nameof(GetEnemy), new { id = enemy.Id }, enemy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("enemies/{id}")]
    public async Task<IActionResult> UpdateEnemy(int id, [FromBody] UpdateEnemyDto dto)
    {
        try
        {
            var enemy = await _adminService.UpdateEnemyAsync(id, dto);
            if (enemy == null)
                return NotFound();
            return Ok(enemy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("enemies/{id}")]
    public async Task<IActionResult> DeleteEnemy(int id)
    {
        var result = await _adminService.DeleteEnemyAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ============ Storybooks ============

    [HttpGet("storybooks")]
    public async Task<IActionResult> GetAllStorybooks()
    {
        var storybooks = await _adminService.GetAllStorybooksAsync();
        return Ok(storybooks);
    }

    [HttpGet("storybooks/{id}")]
    public async Task<IActionResult> GetStorybook(int id)
    {
        var storybook = await _adminService.GetStorybookAsync(id);
        if (storybook == null)
            return NotFound();
        return Ok(storybook);
    }

    [HttpPost("storybooks")]
    public async Task<IActionResult> CreateStorybook([FromBody] CreateStorybookDto dto)
    {
        try
        {
            var storybook = await _adminService.CreateStorybookAsync(dto);
            return CreatedAtAction(nameof(GetStorybook), new { id = storybook.Id }, storybook);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("storybooks/{id}")]
    public async Task<IActionResult> UpdateStorybook(int id, [FromBody] UpdateStorybookDto dto)
    {
        try
        {
            var storybook = await _adminService.UpdateStorybookAsync(id, dto);
            if (storybook == null)
                return NotFound();
            return Ok(storybook);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("storybooks/{id}")]
    public async Task<IActionResult> DeleteStorybook(int id)
    {
        var result = await _adminService.DeleteStorybookAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ============ Random Events ============

    [HttpGet("random-events")]
    public async Task<IActionResult> GetAllRandomEvents()
    {
        var events = await _adminService.GetAllRandomEventsAsync();
        return Ok(events);
    }

    [HttpGet("random-events/{id}")]
    public async Task<IActionResult> GetRandomEvent(int id)
    {
        var evt = await _adminService.GetRandomEventAsync(id);
        if (evt == null)
            return NotFound();
        return Ok(evt);
    }

    [HttpPost("random-events")]
    public async Task<IActionResult> CreateRandomEvent([FromBody] CreateRandomEventDto dto)
    {
        try
        {
            var evt = await _adminService.CreateRandomEventAsync(dto);
            return CreatedAtAction(nameof(GetRandomEvent), new { id = evt.Id }, evt);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("random-events/{id}")]
    public async Task<IActionResult> UpdateRandomEvent(int id, [FromBody] UpdateRandomEventDto dto)
    {
        try
        {
            var evt = await _adminService.UpdateRandomEventAsync(id, dto);
            if (evt == null)
                return NotFound();
            return Ok(evt);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("random-events/{id}")]
    public async Task<IActionResult> DeleteRandomEvent(int id)
    {
        var result = await _adminService.DeleteRandomEventAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ============ Event Choices ============

    [HttpGet("random-events/{eventId}/choices")]
    public async Task<IActionResult> GetEventChoices(int eventId)
    {
        var choices = await _adminService.GetEventChoicesAsync(eventId);
        return Ok(choices);
    }

    [HttpGet("event-choices/{id}")]
    public async Task<IActionResult> GetEventChoice(int id)
    {
        var choice = await _adminService.GetEventChoiceAsync(id);
        if (choice == null)
            return NotFound();
        return Ok(choice);
    }

    [HttpPost("event-choices")]
    public async Task<IActionResult> CreateEventChoice([FromBody] CreateEventChoiceDto dto)
    {
        try
        {
            var choice = await _adminService.CreateEventChoiceAsync(dto);
            return CreatedAtAction(nameof(GetEventChoice), new { id = choice.Id }, choice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("event-choices/{id}")]
    public async Task<IActionResult> UpdateEventChoice(int id, [FromBody] UpdateEventChoiceDto dto)
    {
        try
        {
            var choice = await _adminService.UpdateEventChoiceAsync(id, dto);
            if (choice == null)
                return NotFound();
            return Ok(choice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("event-choices/{id}")]
    public async Task<IActionResult> DeleteEventChoice(int id)
    {
        var result = await _adminService.DeleteEventChoiceAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ============ Skills ============

    [HttpGet("skills")]
    public async Task<IActionResult> GetAllSkills()
    {
        var skills = await _adminService.GetAllSkillsAsync();
        return Ok(skills);
    }

    [HttpGet("skills/{id}")]
    public async Task<IActionResult> GetSkill(int id)
    {
        var skill = await _adminService.GetSkillAsync(id);
        if (skill == null)
            return NotFound();
        return Ok(skill);
    }

    [HttpPost("skills")]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
    {
        try
        {
            var skill = await _adminService.CreateSkillAsync(dto);
            return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("skills/{id}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto dto)
    {
        try
        {
            var skill = await _adminService.UpdateSkillAsync(id, dto);
            if (skill == null)
                return NotFound();
            return Ok(skill);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("skills/{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var result = await _adminService.DeleteSkillAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}

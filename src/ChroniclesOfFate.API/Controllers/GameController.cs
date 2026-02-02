using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChroniclesOfFate.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameSessionService _sessionService;
    private readonly ITurnService _turnService;
    private readonly IRandomEventService _eventService;
    private readonly IBattleService _battleService;
    private readonly ITrainingService _trainingService;
    private readonly IStorybookService _storybookService;

    public GameController(
        IGameSessionService sessionService,
        ITurnService turnService,
        IRandomEventService eventService,
        IBattleService battleService,
        ITrainingService trainingService,
        IStorybookService storybookService)
    {
        _sessionService = sessionService;
        _turnService = turnService;
        _eventService = eventService;
        _battleService = battleService;
        _trainingService = trainingService;
        _storybookService = storybookService;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    // ============ Game Sessions ============

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _sessionService.GetUserSessionsAsync(UserId);
        return Ok(sessions);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateGameSessionDto dto)
    {
        try
        {
            var session = await _sessionService.CreateSessionAsync(UserId, dto);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var session = await _sessionService.GetSessionAsync(id, UserId);
        if (session == null)
            return NotFound();
        return Ok(session);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var result = await _sessionService.DeleteSessionAsync(id, UserId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("sessions/{id}/state")]
    public async Task<IActionResult> GetGameState(int id)
    {
        try
        {
            var state = await _sessionService.GetGameStateAsync(id, UserId);
            return Ok(state);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ============ Turn Actions ============

    [HttpPost("sessions/{id}/turn")]
    public async Task<IActionResult> ProcessTurn(int id, [FromBody] TurnActionDto action)
    {
        try
        {
            var result = await _turnService.ProcessTurnAsync(id, action);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============ Training ============

    [HttpGet("sessions/{id}/training")]
    public async Task<IActionResult> GetTrainingScenarios(int id)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id, UserId);
            if (session?.Character == null)
                return NotFound();

            var scenarios = await _trainingService.GetAvailableScenariosAsync(session.Character.Id);
            return Ok(scenarios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============ Events ============

    [HttpPost("sessions/{sessionId}/events/{eventId}/choice/{choiceId}")]
    public async Task<IActionResult> ProcessEventChoice(int sessionId, int eventId, int choiceId)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId, UserId);
            if (session?.Character == null)
                return NotFound();

            var result = await _eventService.ProcessChoiceAsync(session.Character.Id, eventId, choiceId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============ Battle ============

    [HttpGet("sessions/{id}/enemies")]
    public async Task<IActionResult> GetAvailableEnemies(int id)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id, UserId);
            if (session?.Character == null)
                return NotFound();

            var enemies = await _battleService.GetAvailableEnemiesAsync(session.Character.Id);
            return Ok(enemies);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============ Storybooks ============

    [HttpGet("storybooks")]
    public async Task<IActionResult> GetAllStorybooks()
    {
        var storybooks = await _storybookService.GetAllStorybooksAsync();
        return Ok(storybooks);
    }

    [HttpGet("sessions/{id}/storybooks")]
    public async Task<IActionResult> GetSessionStorybooks(int id)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id, UserId);
            if (session == null)
                return NotFound();

            var unlocked = await _storybookService.GetUnlockedStorybooksAsync(id);
            return Ok(unlocked);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sessions/{sessionId}/storybooks/equip")]
    public async Task<IActionResult> EquipStorybook(int sessionId, [FromBody] EquipStorybookDto dto)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId, UserId);
            if (session?.Character == null)
                return NotFound();

            var result = await _storybookService.EquipStorybookAsync(
                session.Character.Id, dto.StorybookId, dto.SlotPosition);
            
            if (!result)
                return BadRequest(new { message = "Failed to equip storybook. Check slot (1-5) and availability." });
            
            return Ok(new { message = "Storybook equipped successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sessions/{sessionId}/storybooks/loadout")]
    public async Task<IActionResult> SetLoadout(int sessionId, [FromBody] LoadoutDto loadout)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId, UserId);
            if (session?.Character == null)
                return NotFound();

            var result = await _storybookService.SetLoadoutAsync(session.Character.Id, loadout);
            
            if (!result)
                return BadRequest(new { message = "Failed to set loadout" });
            
            return Ok(new { message = "Loadout updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("sessions/{sessionId}/storybooks/slot/{slot}")]
    public async Task<IActionResult> UnequipStorybook(int sessionId, int slot)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId, UserId);
            if (session?.Character == null)
                return NotFound();

            var result = await _storybookService.UnequipStorybookAsync(session.Character.Id, slot);
            
            if (!result)
                return BadRequest(new { message = "No storybook in that slot" });
            
            return Ok(new { message = "Storybook unequipped" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

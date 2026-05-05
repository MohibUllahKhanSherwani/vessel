using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.AI;
using Vessel.Application.Interfaces.AI;

namespace Vessel.API.Controllers;

/// <summary>
/// Provides AI-powered market insight endpoints.
/// </summary>
[Route("api/ai")]
[ApiController]
[Tags("AI")]
[Authorize]
public class AiInsightsController : ControllerBase
{
    private readonly IRagQueryService _ragService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiInsightsController"/> class.
    /// </summary>
    /// <param name="ragService">The RAG query service.</param>
    public AiInsightsController(IRagQueryService ragService)
    {
        _ragService = ragService;
    }

    /// <summary>
    /// Ask a question to the AI assistant using RAG (Retrieval-Augmented Generation) over Vessel market data.
    /// </summary>
    /// <param name="request">The question to ask.</param>
    /// <returns>The AI generated answer and source references.</returns>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AiResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AiResponseDto>> AskQuestion([FromBody] AiRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var response = await _ragService.AskQuestionAsync(request.Question);
        return Ok(response);
    }
}

using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.Commands.CalculateFinancialScore;
using FinanceDAMT.Application.Features.AI.Commands.GenerateRecommendations;
using FinanceDAMT.Application.Features.AI.Commands.SendChatMessage;
using FinanceDAMT.Application.Features.AI.Queries.GetChatHistory;
using FinanceDAMT.Application.Features.AI.Queries.GetFinancialScore;
using FinanceDAMT.Application.Features.AI.Queries.GetRecommendations;
using FinanceDAMT.Application.Features.AI.Queries.GetSpendingPrediction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for AI-powered insights and assistant workflows.
/// </summary>
[ApiController]
[Authorize]
[Route("api/ai")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Creates an instance of <see cref="AIController"/>.
    /// </summary>
    public AIController(IMediator mediator, IAIService aiService, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _aiService = aiService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Sends a chat message and returns assistant response.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendChatMessageCommand(request.Message), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns last 20 chat messages.
    /// </summary>
    [HttpGet("chat/history")]
    public async Task<IActionResult> GetChatHistory(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatHistoryQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Parses natural language expense text into amount and category JSON.
    /// </summary>
    [HttpPost("parse-expense")]
    public async Task<IActionResult> ParseExpense([FromBody] ParseExpenseRequest request)
    {
        var result = await _aiService.ParseNaturalLanguageExpense(request.Input);
        return Ok(new { parsed = result });
    }

    /// <summary>
    /// Returns latest recommendations.
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRecommendationsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Triggers recommendation generation.
    /// </summary>
    [HttpPost("recommendations/generate")]
    public async Task<IActionResult> GenerateRecommendations(CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateRecommendationsCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns latest financial health score.
    /// </summary>
    [HttpGet("score")]
    public async Task<IActionResult> GetScore(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFinancialScoreQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Triggers financial score calculation.
    /// </summary>
    [HttpPost("score/calculate")]
    public async Task<IActionResult> CalculateScore(CancellationToken ct)
    {
        var result = await _mediator.Send(new CalculateFinancialScoreCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns next month spending prediction.
    /// </summary>
    [HttpGet("prediction")]
    public async Task<IActionResult> GetPrediction(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSpendingPredictionQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Chat request payload.
    /// </summary>
    public sealed class ChatRequest
    {
        /// <summary>
        /// User message for the assistant.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Natural language expense parse payload.
    /// </summary>
    public sealed class ParseExpenseRequest
    {
        /// <summary>
        /// Free-text expense description.
        /// </summary>
        public string Input { get; set; } = string.Empty;
    }
}

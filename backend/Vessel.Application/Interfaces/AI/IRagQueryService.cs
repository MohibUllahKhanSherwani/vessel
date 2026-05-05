using Vessel.Application.DTOs.AI;

namespace Vessel.Application.Interfaces.AI;

public interface IRagQueryService
{
    /// <summary>
    /// Processes a natural language question using Retrieval-Augmented Generation (RAG).
    /// Finds relevant market context from the vector database and uses an LLM to generate an informed response.
    /// </summary>
    /// <param name="question">The user's question about market rates or alerts.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the AI answer and source references.</returns>
    Task<AiResponseDto> AskQuestionAsync(string question, CancellationToken cancellationToken = default);
}

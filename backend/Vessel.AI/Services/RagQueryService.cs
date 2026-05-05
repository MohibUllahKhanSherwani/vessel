using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Vessel.Application.DTOs.AI;
using Vessel.Application.Interfaces.AI;
using Vessel.Application.Interfaces.Repositories;
using System.Text;

namespace Vessel.AI.Services;

public class RagQueryService : IRagQueryService
{
    private readonly IEmbeddingGeneratorService _embeddingGenerator;
    private readonly IRateEmbeddingRepository _embeddingRepository;
    private readonly IChatCompletionService _chatService;

    public RagQueryService(
        IEmbeddingGeneratorService embeddingGenerator,
        IRateEmbeddingRepository embeddingRepository,
        IConfiguration configuration)
    {
        _embeddingGenerator = embeddingGenerator;
        _embeddingRepository = embeddingRepository;
        
        var apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
        var modelId = configuration["Gemini:ChatModelId"] ?? "gemini-1.5-flash";
        
        _chatService = new GoogleAIGeminiChatCompletionService(modelId, apiKey);
    }

    public async Task<AiResponseDto> AskQuestionAsync(string question, CancellationToken cancellationToken = default)
    {
        // 1. Embed the question
        var questionEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(question, cancellationToken);
        
        // 2. Retrieve top matching rows
        var matches = await _embeddingRepository.GetNearestNeighborsAsync(questionEmbedding, limit: 10, cancellationToken: cancellationToken);
        
        if (!matches.Any())
        {
            return new AiResponseDto
            {
                Answer = "I couldn't find any relevant data to answer your question.",
                SourceEmbeddingIds = new List<Guid>()
            };
        }

        // 3. Build prompt from retrieved context
        var contextBuilder = new StringBuilder();
        foreach (var match in matches)
        {
            contextBuilder.AppendLine($"- {match.ContentText}");
        }

        var prompt = $@"
You are an AI assistant for Vessel, a fuel delivery and price tracking platform.
Use the following context to answer the user's question. 
If the context doesn't contain enough information, use your general knowledge but mention it's not based on specific Vessel data.
Keep your answer concise and professional.

Context:
{contextBuilder}

User Question: {question}
";

        // 4. Ask chat model
        var response = await _chatService.GetChatMessageContentAsync(prompt, cancellationToken: cancellationToken);
        
        return new AiResponseDto
        {
            Answer = response.Content ?? "I'm sorry, I couldn't generate an answer.",
            SourceEmbeddingIds = matches.Select(m => m.Id).ToList()
        };
    }
}

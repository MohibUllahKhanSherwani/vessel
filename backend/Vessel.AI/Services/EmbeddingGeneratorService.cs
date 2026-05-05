using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Google;
using Vessel.Application.Interfaces.AI;

namespace Vessel.AI.Services;

public class EmbeddingGeneratorService : IEmbeddingGeneratorService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public EmbeddingGeneratorService(IConfiguration configuration)
    {
        var apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
        var modelId = configuration["Gemini:EmbeddingModelId"] ?? "text-embedding-004";
        
        _embeddingService = new GoogleAITextEmbeddingGenerationService(modelId, apiKey);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<float>();
        }

        var result = await _embeddingService.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.ToArray();
    }
}

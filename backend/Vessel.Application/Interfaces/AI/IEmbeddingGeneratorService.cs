namespace Vessel.Application.Interfaces.AI;

public interface IEmbeddingGeneratorService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}

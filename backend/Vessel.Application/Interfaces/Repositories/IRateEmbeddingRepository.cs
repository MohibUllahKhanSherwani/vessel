using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IRateEmbeddingRepository
{
    Task AddAsync(RateEmbedding entity, CancellationToken cancellationToken = default);
    Task<List<RateEmbedding>> GetNearestNeighborsAsync(float[] embedding, int limit = 5, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Repositories;

public class RateEmbeddingRepository : IRateEmbeddingRepository
{
    private readonly ApplicationDbContext _context;

    public RateEmbeddingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RateEmbedding entity, CancellationToken cancellationToken = default)
    {
        await _context.RateEmbeddings.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<RateEmbedding>> GetNearestNeighborsAsync(float[] embedding, int limit = 5, CancellationToken cancellationToken = default)
    {
        var embeddings = await _context.RateEmbeddings.ToListAsync(cancellationToken);

        // Simple cosine similarity in memory
        return embeddings
            .Select(e => new
            {
                Item = e,
                Similarity = CosineSimilarity(e.Embedding, embedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(limit)
            .Select(x => x.Item)
            .ToList();
    }

    private static double CosineSimilarity(float[] vecA, float[] vecB)
    {
        if (vecA == null || vecB == null || vecA.Length == 0 || vecB.Length == 0 || vecA.Length != vecB.Length)
            return 0;

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < vecA.Length; i++)
        {
            dotProduct += vecA[i] * vecB[i];
            normA += vecA[i] * vecA[i];
            normB += vecB[i] * vecB[i];
        }

        if (normA == 0 || normB == 0) return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}

using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
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
        var vector = new Pgvector.Vector(embedding);
        
        return await _context.RateEmbeddings
            .OrderBy(e => e.Embedding.L2Distance(vector))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

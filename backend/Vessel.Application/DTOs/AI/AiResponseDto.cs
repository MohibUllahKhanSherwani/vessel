namespace Vessel.Application.DTOs.AI;

public class AiResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public List<Guid> SourceEmbeddingIds { get; set; } = new();
}

using Vessel.Application.Interfaces.AI;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Application.Services.AI;

public class AiEmbeddingJob
{
    private readonly IRateEmbeddingRepository _embeddingRepository;
    private readonly IEmbeddingGeneratorService _embeddingGenerator;
    private readonly IProviderRateRepository _rateRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IPriceAlertRepository _alertRepository;

    public AiEmbeddingJob(
        IRateEmbeddingRepository embeddingRepository,
        IEmbeddingGeneratorService embeddingGenerator,
        IProviderRateRepository rateRepository,
        IProviderRepository providerRepository,
        IAreaRepository areaRepository,
        IPriceAlertRepository alertRepository)
    {
        _embeddingRepository = embeddingRepository;
        _embeddingGenerator = embeddingGenerator;
        _rateRepository = rateRepository;
        _providerRepository = providerRepository;
        _areaRepository = areaRepository;
        _alertRepository = alertRepository;
    }

    public async Task ProcessRateChangeAsync(Guid rateId)
    {
        var rate = await _rateRepository.GetByIdAsync(rateId);
        if (rate == null) return;

        var provider = await _providerRepository.GetByIdAsync(rate.ProviderId);
        var area = await _areaRepository.GetByIdAsync(rate.AreaId);

        var text = $"Provider {provider?.CompanyName} updated rate for {area?.Name}, {area?.City} to ${rate.PricePerGallon}/gallon on {rate.EffectiveFrom:f}.";

        var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(text);

        await _embeddingRepository.AddAsync(new RateEmbedding
        {
            Id = Guid.NewGuid(),
            SourceType = SourceType.ProviderRate,
            SourceId = rateId,
            ContentText = text,
            Embedding = embedding
        });
    }

    public async Task ProcessAlertTriggerAsync(Guid alertId, Guid triggeredRateId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return;
        
        var rate = await _rateRepository.GetByIdAsync(triggeredRateId);
        if (rate == null) return;

        var area = await _areaRepository.GetByIdAsync(alert.AreaId);

        var text = $"Price alert triggered for {area?.Name}, {area?.City}. Threshold ${alert.ThresholdTotalPrice} for {alert.TargetVolumeInGallons} gallons was met by a rate of ${rate.PricePerGallon}/gallon.";

        var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(text);

        await _embeddingRepository.AddAsync(new RateEmbedding
        {
            Id = Guid.NewGuid(),
            SourceType = SourceType.PriceAlert,
            SourceId = alertId,
            ContentText = text,
            Embedding = embedding
        });
    }
}

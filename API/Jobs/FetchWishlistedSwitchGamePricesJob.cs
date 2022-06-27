using Core.Pricing;
using Core.Pricing.Switch;
using MediatR;
using Quartz;

namespace API.Jobs;

public class FetchWishlistedSwitchGamePricesJob : IJob
{
    private readonly IMediator _mediator;

    public FetchWishlistedSwitchGamePricesJob(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public Task Execute(IJobExecutionContext context)
    {
        return _mediator.Send(new FetchWishlistedSwitchGamePricesCommand());
    }
}
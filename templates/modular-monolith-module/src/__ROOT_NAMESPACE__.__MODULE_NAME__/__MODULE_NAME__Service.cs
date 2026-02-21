using MediatR;

namespace __ROOT_NAMESPACE__.__MODULE_NAME__;

public sealed class __MODULE_NAME__Service(IMediator mediator) : I__MODULE_NAME__Service
{
    public Task<string> Ping(CancellationToken cancellationToken = default)
        => mediator.Send(new PingQuery(), cancellationToken);
}

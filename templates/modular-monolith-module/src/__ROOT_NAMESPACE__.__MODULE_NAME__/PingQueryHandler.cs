using MediatR;

namespace __ROOT_NAMESPACE__.__MODULE_NAME__;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult("pong");
    }
}

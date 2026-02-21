namespace __ROOT_NAMESPACE__.__MODULE_NAME__;

public interface I__MODULE_NAME__Service
{
    Task<string> Ping(CancellationToken cancellationToken = default);
}

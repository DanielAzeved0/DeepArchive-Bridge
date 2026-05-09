namespace DeepArchiveBridge.API.Controllers;

public interface IDependencyHealthCheck
{
    Task<(bool IsHealthy, string Details)> CheckAsync();
}

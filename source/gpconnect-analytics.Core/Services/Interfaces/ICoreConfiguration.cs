namespace Core.Services.Interfaces;

public interface ICoreConfigurationService
{
    string GetConnectionString(string name);
}
namespace rambo.Interfaces
{
    public interface IConfiguration
    {
        IConfigurationIndex Key { get; }

        ConfigurationState State { get; }
    }
}
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class GarbageCollectedConfiguration : IConfiguration
    {
        public GarbageCollectedConfiguration(IConfigurationIndex key)
        {
            State = ConfigurationState.GCed;
            Key = key;
        }

        public ConfigurationState State { get; private set; }

        public IConfigurationIndex Key { get; private set; }
    }
}
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ConfigurationIndex : IConfigurationIndex
    {
        public ConfigurationIndex(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
    }
}
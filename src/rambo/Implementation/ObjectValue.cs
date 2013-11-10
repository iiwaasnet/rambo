using rambo.Interfaces;

namespace rambo.Implementation
{
    class ObjectValue<T> : IObjectValue<T>
    {
        public T Value { get; set; }
    }
}
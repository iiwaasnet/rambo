using System;

namespace rambo.Interfaces
{
    public interface IPrecondition
    {
        bool Wait(TimeSpan timeout);
    }
}
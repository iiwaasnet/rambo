﻿namespace rambo.Interfaces
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object obj);

        T Deserialize<T>(byte[] buffer); 
    }
}
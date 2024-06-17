using System;

namespace Appegy.BinaryStorage
{
    internal interface IReactiveCollection : IDisposable
    {
        public event Action OnChanged;
    }
}
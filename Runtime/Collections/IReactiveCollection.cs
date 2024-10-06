using System;

namespace Appegy.Storage
{
    internal interface IReactiveCollection : IDisposable
    {
        public event Action<IReactiveCollection> OnChanged;
    }
}
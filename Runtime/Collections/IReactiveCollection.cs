using System;

namespace Appegy.BinaryStorage
{
    internal interface IReactiveCollection : IDisposable
    {
        public string Key { get; internal set; }
        public event Action OnChanged;
        public void Clear();
    }
}
using System;

namespace Appegy.Storage
{
    public class KeyLoadFailedException : Exception
    {
        public KeyLoadFailedException(string key, string type, long size, string reason)
            : base($"Failed to load key {key} of type {type} with size {size}b. Reason: {reason}")
        {
        }
    }
}
using System;

namespace Appegy.BinaryStorage
{
    public class IncorrectUsageOfCollectionException : Exception
    {
        public IncorrectUsageOfCollectionException(string action, Type actualType)
            : base($"Can't handle {actualType.Name} in {action}. Use corresponding methods for collections instead.")
        {
        }
    }
}
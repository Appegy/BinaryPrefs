using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class CollectionKindMismatchTests : BaseStorageTests
    {
        [Test]
        public void WhenCollectionKindMismatched_ThenErrorMentionsActualCallAndType()
        {
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportListsOf<int>()
                .SupportSetsOf<int>()
                .Build();

            storage.GetListOf<int>("k").Add(1);

            Action action = () => storage.GetSetOf<int>("k");

            action.Should().Throw<UnexpectedTypeException>()
                .Where(e => e.Message.Contains("GetSetOf") && e.Message.Contains("ReactiveSet"));
        }
    }
}

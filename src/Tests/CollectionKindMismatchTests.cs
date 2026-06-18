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

            storage.Invoking(s => s.GetSetOf<int>("k"))
                .Should().Throw<UnexpectedTypeException>()
                .Where(e => e.Message.Contains("GetSetOf") && e.Message.Contains("ReactiveSet"));
        }
    }
}

using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.UnitTests.Common;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
        public TestEntity() { }

        public TestEntity(Guid id) => Id = id;
    }

    private sealed class OtherEntity : BaseEntity
    {
        public OtherEntity(Guid id) => Id = id;
    }

    [Fact]
    public void Equals_SameTypeAndId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var first = new TestEntity(id);
        var second = new TestEntity(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalseEvenWithSameId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var other = new OtherEntity(id);

        Assert.False(entity.Equals(other));
    }

    [Fact]
    public void Equals_DifferentInstancesWithNewIds_ReturnsFalse()
    {
        var first = new TestEntity();
        var second = new TestEntity();

        Assert.NotEqual(first, second);
    }
}

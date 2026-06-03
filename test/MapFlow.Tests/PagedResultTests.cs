namespace MapFlow.Tests;

public class PagedResultTests
{
    [Fact]
    public void DefaultValues_AreSane()
    {
        var paged = new PagedResult<string>();

        Assert.Equal(1, paged.PageNumber);
        Assert.Equal(10, paged.PageSize);
        Assert.Equal(0, paged.RowCount);
        Assert.Equal(0, paged.PageCount);
        Assert.Empty(paged.Items);
    }

    [Fact]
    public void CanSetProperties()
    {
        var paged = new PagedResult<int>
        {
            Items = [1, 2, 3],
            RowCount = 100,
            PageNumber = 2,
            PageSize = 3,
            PageCount = 34
        };

        Assert.Equal([1, 2, 3], paged.Items);
        Assert.Equal(100, paged.RowCount);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(3, paged.PageSize);
        Assert.Equal(34, paged.PageCount);
    }

    [Fact]
    public void Map_WithSelector_ReturnsNewType()
    {
        var source = new PagedResult<int>
        {
            Items = [1, 2, 3],
            RowCount = 100,
            PageNumber = 2,
            PageSize = 3,
            PageCount = 34
        };

        var result = source.Map(x => x.ToString());

        Assert.Equal(["1", "2", "3"], result.Items);
    }

    [Fact]
    public void Map_WithSelector_PreservesMetadata()
    {
        var source = new PagedResult<int>
        {
            Items = [1, 2, 3],
            RowCount = 100,
            PageNumber = 2,
            PageSize = 3,
            PageCount = 34
        };

        var result = source.Map(x => $"item-{x}");

        Assert.Equal(100, result.RowCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(34, result.PageCount);
    }

    [Fact]
    public void Map_Chain_KeepsMetadata()
    {
        var source = new PagedResult<int>
        {
            Items = [1, 2, 3],
            RowCount = 100,
            PageNumber = 2,
            PageSize = 3,
            PageCount = 34
        };

        var result = source
            .Map(x => x * 10)
            .Map(x => x.ToString());

        Assert.Equal(["10", "20", "30"], result.Items);
        Assert.Equal(100, result.RowCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(34, result.PageCount);
    }

    [Fact]
    public void Map_WithInterface_PreservesMetadata()
    {
        var source = new PagedResult<IdEntity>
        {
            Items = [new() { Id = 1 }, new() { Id = 2 }],
            RowCount = 50,
            PageNumber = 1,
            PageSize = 2,
            PageCount = 25
        };

        var result = Mapper.Map<IdEntity, IdDto>(source);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(50, result.RowCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(25, result.PageCount);
    }

    private class IdEntity
    {
        public int Id { get; set; }
    }

    private class IdDto : IMapFrom<IdEntity>
    {
        public int Id { get; set; }

        public void MapFrom(IdEntity source)
        {
            Id = source.Id;
        }
    }
}

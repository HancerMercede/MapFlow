namespace MapFlow.Tests;

// ─── Test Models ──────────────────────────────────────────────────────────────

public class UserEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserDto : IMapFrom<UserEntity>
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MemberSince { get; set; } = string.Empty;

    public void MapFrom(UserEntity source)
    {
        Id = source.Id;
        FullName = $"{source.FirstName} {source.LastName}";
        Email = source.Email;
        MemberSince = source.CreatedAt.Year.ToString();
    }
}

// ─── Tests ────────────────────────────────────────────────────────────────────

public class MapperTests
{
    private static readonly UserEntity Alice = new()
    {
        Id = 1,
        FirstName = "Alice",
        LastName = "Wonders",
        Email = "alice@example.com",
        CreatedAt = new(2023, 6, 15)
    };

    // ============================================================
    // Selector-based (fluent)
    // ============================================================

    [Fact]
    public void Selector_Map_Single()
    {
        var dto = Alice.Map(e => new UserDto
        {
            Id = e.Id,
            FullName = $"{e.FirstName} {e.LastName}",
            Email = e.Email,
            MemberSince = e.CreatedAt.Year.ToString()
        });

        Assert.Equal(1, dto.Id);
        Assert.Equal("Alice Wonders", dto.FullName);
        Assert.Equal("alice@example.com", dto.Email);
        Assert.Equal("2023", dto.MemberSince);
    }

    [Fact]
    public void Selector_Collection()
    {
        var entities = new[]
        {
            Alice,
            new UserEntity { Id = 2, FirstName = "Bob", LastName = "Builder" }
        };

        var dtos = entities.Map(e => new UserDto
        {
            Id = e.Id,
            FullName = $"{e.FirstName} {e.LastName}"
        });

        Assert.Equal(2, dtos.Count);
        Assert.Equal("Alice Wonders", dtos[0].FullName);
        Assert.Equal("Bob Builder", dtos[1].FullName);
    }

    [Fact]
    public void Selector_EmptyCollection_ReturnsEmpty()
    {
        var dtos = Array.Empty<UserEntity>().Map(e => new UserDto
        {
            FullName = $"{e.FirstName} {e.LastName}"
        });

        Assert.Empty(dtos);
    }

    [Fact]
    public void Selector_PagedResult()
    {
        var paged = new PagedResult<UserEntity>
        {
            Items = [Alice, new() { Id = 2, FirstName = "Bob" }],
            RowCount = 20,
            PageNumber = 1,
            PageSize = 2,
            PageCount = 10
        };

        var dtos = paged.Map(e => new UserDto
        {
            Id = e.Id,
            FullName = $"{e.FirstName} {e.LastName}"
        });

        Assert.Equal(2, dtos.Items.Count);
        Assert.Equal(20, dtos.RowCount);
        Assert.Equal(1, dtos.PageNumber);
        Assert.Equal(2, dtos.PageSize);
        Assert.Equal(10, dtos.PageCount);
        Assert.Equal("Alice Wonders", dtos.Items[0].FullName);
    }

    // ============================================================
    // Interface-based
    // ============================================================

    [Fact]
    public void Interface_Map_Single()
    {
        var dto = Mapper.Map<UserEntity, UserDto>(Alice);

        Assert.Equal(1, dto.Id);
        Assert.Equal("Alice Wonders", dto.FullName);
        Assert.Equal("alice@example.com", dto.Email);
        Assert.Equal("2023", dto.MemberSince);
    }

    [Fact]
    public void Interface_Collection()
    {
        var entities = new[]
        {
            Alice,
            new UserEntity { Id = 2, FirstName = "Bob", LastName = "Builder" }
        };

        var dtos = Mapper.Map<UserEntity, UserDto>(entities);

        Assert.Equal(2, dtos.Count);
        Assert.Equal("Alice Wonders", dtos[0].FullName);
        Assert.Equal("Bob Builder", dtos[1].FullName);
    }

    [Fact]
    public void Interface_EmptyCollection_ReturnsEmpty()
    {
        var dtos = Mapper.Map<UserEntity, UserDto>(Array.Empty<UserEntity>());

        Assert.Empty(dtos);
    }

    [Fact]
    public void Interface_PagedResult()
    {
        var paged = new PagedResult<UserEntity>
        {
            Items = [Alice, new() { Id = 2, FirstName = "Bob", LastName = "Builder" }],
            RowCount = 20,
            PageNumber = 1,
            PageSize = 2,
            PageCount = 10
        };

        var dtos = Mapper.Map<UserEntity, UserDto>(paged);

        Assert.Equal(2, dtos.Items.Count);
        Assert.Equal(20, dtos.RowCount);
        Assert.Equal(1, dtos.PageNumber);
        Assert.Equal(10, dtos.PageCount);
        Assert.Equal("Alice Wonders", dtos.Items[0].FullName);
    }

    // ============================================================
    // Interface-based fluent (MapTo)
    // ============================================================

    [Fact]
    public void MapTo_Fluent_Single()
    {
        var dto = Alice.MapTo<UserEntity, UserDto>();

        Assert.Equal(1, dto.Id);
        Assert.Equal("Alice Wonders", dto.FullName);
    }

    [Fact]
    public void MapTo_Fluent_Collection()
    {
        var entities = new[] { Alice };

        var dtos = entities.MapTo<UserEntity, UserDto>();

        Assert.Single(dtos);
        Assert.Equal("Alice Wonders", dtos.First().FullName);
    }

    // ============================================================
    // Apply
    // ============================================================

    [Fact]
    public void Apply_Action_MutatesInPlace()
    {
        var entity = new UserEntity { FirstName = "Alice" };
        var same = entity.Apply(e => e.FirstName = "Updated");

        Assert.Same(entity, same);
        Assert.Equal("Updated", entity.FirstName);
    }

    [Fact]
    public void Apply_Func_ReturnsNewInstance()
    {
        var entity = new UserEntity { FirstName = "Alice" };
        var result = entity.Apply(e => new UserEntity { FirstName = e.FirstName.ToUpper() });

        Assert.NotSame(entity, result);
        Assert.Equal("ALICE", result.FirstName);
        Assert.Equal("Alice", entity.FirstName);
    }

    [Fact]
    public void Apply_Interface_UpdatesExisting()
    {
        var existing = new UserDto();
        Mapper.Apply(Alice, existing);

        Assert.Equal(1, existing.Id);
        Assert.Equal("Alice Wonders", existing.FullName);
    }

    // ============================================================
    // Chaining
    // ============================================================

    [Fact]
    public void Chain_MultipleApply()
    {
        var entity = new UserEntity { FirstName = "  Alice  ", LastName = "Wonders" };

        entity
            .Apply(e => e.FirstName = e.FirstName.Trim())
            .Apply(e => e.LastName = e.LastName.ToUpper());

        Assert.Equal("Alice", entity.FirstName);
        Assert.Equal("WONDERS", entity.LastName);
    }

    [Fact]
    public void Chain_Apply_Then_Map()
    {
        var entity = new UserEntity { FirstName = "alice", LastName = "wonders" };

        var dto = entity
            .Apply(e =>
            {
                e.FirstName = char.ToUpper(e.FirstName[0]) + e.FirstName[1..];
                e.LastName = e.LastName.ToUpper();
            })
            .Map(e => new UserDto
            {
                Id = e.Id,
                FullName = $"{e.FirstName} {e.LastName}"
            });

        Assert.Equal("Alice WONDERS", dto.FullName);
    }

    [Fact]
    public void Chain_Collection_Then_Map()
    {
        var entities = new[] { Alice, new UserEntity { Id = 2, FirstName = "Bob" } };

        var dtos = entities
            .Where(e => e.Id > 0)
            .Map(e => new UserDto { Id = e.Id, FullName = $"{e.FirstName} {e.LastName}" });

        Assert.Equal(2, dtos.Count);
    }

    // ============================================================
    // Null guards
    // ============================================================

    [Fact]
    public void Map_NullSource_ThrowsArgumentNullException()
    {
        UserEntity? nullEntity = null;

        var ex = Assert.Throws<ArgumentNullException>(
            () => Mapper.Map<UserEntity, UserDto>(nullEntity!));

        Assert.Equal("source", ex.ParamName);
    }

    [Fact]
    public void Map_NullSelector_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Alice.Map<UserEntity, UserDto>(null!));

        Assert.Equal("selector", ex.ParamName);
    }

    [Fact]
    public void Map_NullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<UserEntity>? nullCollection = null;

        var ex = Assert.Throws<ArgumentNullException>(
            () => Mapper.Map<UserEntity, UserDto>(nullCollection!));

        Assert.Equal("source", ex.ParamName);
    }

    [Fact]
    public void Map_NullPagedResult_ThrowsArgumentNullException()
    {
        PagedResult<UserEntity>? nullPaged = null;

        var ex = Assert.Throws<ArgumentNullException>(
            () => Mapper.Map<UserEntity, UserDto>(nullPaged!));

        Assert.Equal("source", ex.ParamName);
    }

    [Fact]
    public void Apply_NullAction_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Alice.Apply((Action<UserEntity>?)null!));

        Assert.Equal("mutator", ex.ParamName);
    }

    [Fact]
    public void Apply_NullFunc_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Alice.Apply((Func<UserEntity, UserEntity>?)null!));

        Assert.Equal("transform", ex.ParamName);
    }

    [Fact]
    public void Apply_Interface_NullDestination_ThrowsArgumentNullException()
    {
        UserDto? nullDto = null;

        var ex = Assert.Throws<ArgumentNullException>(
            () => Mapper.Apply(Alice, nullDto!));

        Assert.Equal("destination", ex.ParamName);
    }
}

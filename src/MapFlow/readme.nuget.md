# MapFlow

Zero-reflection, zero-dependency object mapper for .NET. **AOT compatible**, no `System.Reflection`, no runtime code generation, no DI container required.

MapFlow sits between writing mapping code by hand (tedious) and pulling in AutoMapper (heavy, reflection-based, profile hell). It gives you three mapping strategies that scale with your needs.

---

## Quick start

```csharp
using MapFlow;

// ─── 1. Lambda-based (ad-hoc, no setup) ───
ProductDto dto = product.Map(p => new ProductDto
{
    Id = p.Id,
    Name = p.Name,
    Price = p.Price
});

// ─── 2. Interface-based (declarative, reusable) ───
ProductDto dto = Mapper.Map<Product, ProductDto>(product);
List<ProductDto> dtos = Mapper.Map<Product, ProductDto>(products);

// ─── 3. Source Generator (zero-boilerplate) ───
public partial class ProductDto : IMapFrom<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // Source Generator auto-generates MapFrom(Product source)
    // You can still add CustomMapFrom(Product) for edge cases
}

// ─── In-place mutation ───
product.Apply(p => { p.Name = request.Name; });

// ─── PagedResult ───
PagedResult<ProductDto> dtos = pagedProducts.Map(p => new ProductDto(/*...*/));
```

---

## Why MapFlow?

| Concern | AutoMapper | MapFlow |
|---|---|---|
| Reflection | Yes — startup cost + runtime | **Zero** |
| Dependencies | Massive graph | **Zero dependencies** |
| AOT / Native | ❌ | **✅ Compatible** |
| DI required | Usually | **Not required** |
| Profiles | Yes — separate config | **Interfaces or lambdas** |
| Source Generator | ❌ | **✅ Auto-maps matching props** |
| Null safety | NullReferenceException | **ArgumentNullException everywhere** |

---

## API at a glance

| Method | What it does |
|---|---|
| `source.Map(selector)` | Projects a single object |
| `source.Map(selector)` (IEnumerable) | Projects a list |
| `Mapper.Map<TSrc,TDst>(source)` | Interface-based mapping |
| `source.MapTo<TDest>()` | Fluent interface-based mapping |
| `source.Apply(mutator)` | Mutates and returns the same instance |
| `source.Apply(transform)` | Transforms and returns a new instance |
| `pagedResult.Map(selector)` | Projects PagedResult preserving metadata |
| `Mapper.Map<TSrc,TDst>(items)` | Maps a list via interfaces |

All methods validate arguments and throw `ArgumentNullException` — no silent NPEs.

---

## Supported types

- `class` ✅
- `record` ✅
- `record struct` ✅
- `struct` ✅
- `readonly struct` ✅

The Source Generator detects `class`, `struct`, `record`, and `record struct` keywords automatically — no manual configuration.

---

## Learn more

Full documentation, samples, and contribution guide:

https://github.com/HancerMercede/MapFlow

# MapFlow

Zero-reflection, zero-dependency object mapper for .NET.

## Quick start

```csharp
using MapFlow;

// Lambda-based (ad-hoc)
var dto = product.Map(p => new ProductDto { Id = p.Id, Name = p.Name });

// Interface-based (declarative)
var dto = Mapper.Map<Product, ProductDto>(product);

// With Source Generator (auto-map)
public partial class ProductDto : IMapFrom<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // SG generates MapFrom automatically
}

// In-place mutation
product.Apply(p => { p.Name = request.Name; });
```

## Features
- **Zero reflection, zero dependencies, AOT compatible**
- **Lambda-based** for ad-hoc mappings
- **Interface-based** (IMapFrom/IMapTo) for repeatable mappings
- **Source Generator** auto-generates MapFrom/MapTo for matching properties
- **PagedResult** built-in with mapping support
- **Apply / mutation** for in-place updates and chaining
- **Null-safe** — all entry points validate arguments
- **Records, structs, classes** — all supported

## Learn more
https://github.com/HancerMercede/MapFlow

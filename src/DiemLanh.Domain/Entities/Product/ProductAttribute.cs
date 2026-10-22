using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Product;

public class ProductAttribute : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Công suất (BTU), Công nghệ Inverter, Loại gas
    public string Code { get; set; } = string.Empty; // e.g. capacity_btu, inverter_tech, gas_type
    public string? GroupName { get; set; } // e.g. Thông số kỹ thuật, Công nghệ & Tính năng
    public string AttributeType { get; set; } = "Text"; // Text, Number, Range, Select
    public string? Unit { get; set; } // BTU, HP, m², kW/h, Liter, dB
    public bool IsFilterable { get; set; } = true;
    public bool IsSearchable { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    // Navigation
    public ProductCategory Category { get; set; } = null!;
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
}

public class ProductAttributeValue : BaseEntity
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty; // e.g. 9000 BTU, Dual Inverter, R32
    public double? NumberValue { get; set; } // For numeric range filtering e.g. 9000, 1.5, 300

    // Navigation
    public Product Product { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}

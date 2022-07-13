using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class Product
{
    public virtual Guid Id { get; set; }

    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual IList<ProductAvailability> Availability { get; set; }
}

public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Schema("public");
        Table("product");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");
        Map(x => x.Description, "description");
        HasMany(x => x.Availability);
    }
}
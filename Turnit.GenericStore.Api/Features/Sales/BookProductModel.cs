using System;

namespace Turnit.GenericStore.Api.Features.Sales;

public class BookProductModel
{
    public Guid StoreId { get; set; }
    public int Quantity { get; set; }
}

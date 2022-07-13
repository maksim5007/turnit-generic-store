using System;

namespace Turnit.GenericStore.Api.Features.Sales;

public class StoreRestockModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
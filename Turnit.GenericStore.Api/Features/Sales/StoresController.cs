using NHibernate;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Turnit.GenericStore.Api.Entities;
using System;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("store")]
public class StoresController : ApiControllerBase
{
    private readonly ISession _session;

    public StoresController(ISession session)
    {
        _session = session;
    }

    [HttpPost, Route("{storeId:guid}/restock")]
    public async Task<ActionResult> Restock(Guid storeId, [FromBody] StoreRestockModel[] storeRestocks)
    {
        using (ITransaction transaction = _session.BeginTransaction())
        {
            foreach (StoreRestockModel storeRestock in storeRestocks)
            {
                var availabilities = await _session.QueryOver<ProductAvailability>()
               .Where(pa => pa.Product.Id == storeRestock.ProductId && pa.Store.Id == storeId)
               .ListAsync();

                foreach (ProductAvailability availability in availabilities)
                {
                    availability.Availability += storeRestock.Quantity;

                    _session.SaveOrUpdate(availability);
                }
            }

            transaction.Commit();
        }

        return Ok();
    }

}
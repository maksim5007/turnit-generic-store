using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Turnit.GenericStore.Api.Entities;
using NHibernate.Type;
using AutoMapper;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("products")]
public class ProductsController : ApiControllerBase
{
    private readonly ISession _session;
    private readonly ISessionFactory _sessionFactory;
    private readonly IMapper _mapper;

    public ProductsController(ISession session, ISessionFactory sessionFactory, IMapper mapper)
    {
        _session = session;
        _sessionFactory = sessionFactory;
        _mapper = mapper;
    }

    [HttpGet, Route("by-category/{categoryId:guid}")]
    public async Task<ProductModel[]> ProductsByCategory(Guid categoryId)
    {
        var products = await _session.QueryOver<ProductCategory>()
            .Where(x => x.Category.Id == categoryId)
            .Select(x => x.Product)
            .ListAsync<Product>();

        var result = _mapper.Map<ProductModel[]>(products);

        return result;
    }

    [HttpGet, Route("")]
    public async Task<ProductCategoryModel[]> AllProducts()
    {
        //TODO: this could be a performance issue, needs to be rewrited so all the query grouping logic would be executed on db server.
        //Could even consider using plain SP or HQL
        var products = await _session.QueryOver<Product>().ListAsync<Product>();
        var productCategories = await _session.QueryOver<ProductCategory>().ListAsync();

        var productModels = _mapper.Map<List<ProductModel>>(products);

        var result = new List<ProductCategoryModel>();
        foreach (var category in productCategories.GroupBy(x => x.Category.Id))
        {
            var productIds = category.Select(x => x.Product.Id).ToHashSet();
            result.Add(new ProductCategoryModel
            {
                CategoryId = category.Key,
                Products = productModels
                    .Where(x => productIds.Contains(x.Id))
                    .ToArray()
            });
        }

        var uncategorizedProducts = productModels.Except(result.SelectMany(x => x.Products));
        if (uncategorizedProducts.Any())
        {
            result.Add(new ProductCategoryModel
            {
                Products = uncategorizedProducts.ToArray()
            });
        }

        return result.ToArray();
    }

    [HttpPut, Route("{productId:guid}/category/{categoryId:guid}")]
    public async Task<ActionResult> AddProductToCategory(Guid productId, Guid categoryId)
    {
        var product = await _session.QueryOver<Product>().Where(p => p.Id == productId).SingleOrDefaultAsync();

        if (product == null)
        {
            return NotFound($"No product with id '{productId}' found");
        }

        var category = await _session.QueryOver<Category>().Where(c => c.Id == categoryId).SingleOrDefaultAsync();

        if (category == null)
        {
            return NotFound($"No category with id '{categoryId}' found");
        }

        bool alreadyExists = _session.QueryOver<ProductCategory>().Where(pc => pc.Product.Id == productId && pc.Category.Id == categoryId).RowCount() > 0;
        if (alreadyExists)
            return Ok();


        var productCategory = new ProductCategory
        {
            Product = product,
            Category = category
        };
        await _session.SaveAsync(productCategory);
        _session.Flush();

        return Ok();
    }

    [HttpDelete, Route("{productId:guid}/category/{categoryId:guid}")]
    public async Task<ActionResult> RemoveProductFromCategory(Guid productId, Guid categoryId)
    {
        await _session.DeleteAsync("from ProductCategory pc where pc.Product.Id = ? and pc.Category.id = ?", new object[] { productId, categoryId }, new IType[] { NHibernateUtil.Guid, NHibernateUtil.Guid });
        _session.Flush();

        return Ok();

    }

    [HttpPost, Route("{productId:guid}/book")]
    public async Task<ActionResult> BookProduct(Guid productId, [FromBody] BookProductModel[] bookProducts)
    {
        using (IStatelessSession session = _sessionFactory.OpenStatelessSession())
        using (ITransaction transaction = session.BeginTransaction())
        {
            foreach (BookProductModel bookProduct in bookProducts)
            {
                var availabilities = await _session.QueryOver<ProductAvailability>()
               .Where(pa => pa.Product.Id == productId && pa.Store.Id == bookProduct.StoreId)
               .ListAsync();

                foreach (ProductAvailability availability in availabilities)
                {
                    availability.Availability -= bookProduct.Quantity;

                    if (availability.Availability < 0)
                        availability.Availability = 0;

                    session.Update(availability);
                }
            }

            transaction.Commit();
        }

        return Ok();
    }
}
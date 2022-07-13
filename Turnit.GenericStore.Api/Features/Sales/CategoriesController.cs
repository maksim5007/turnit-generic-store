using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Turnit.GenericStore.Api.Entities;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly ISession _session;
    private readonly IMapper _mapper;

    public CategoriesController(ISession session, IMapper mapper)
    {
        _session = session;
        _mapper = mapper;
    }

    [HttpGet, Route("")]
    public async Task<CategoryModel[]> AllCategories()
    {
        var categories = await _session.QueryOver<Category>().ListAsync();

        var result = _mapper.Map<CategoryModel[]>(categories);

        return result;
    }
}

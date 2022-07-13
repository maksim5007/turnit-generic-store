using Turnit.GenericStore.Api.Entities;
using Turnit.GenericStore.Api.Features.Sales;

namespace Turnit.GenericStore.Api
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductAvailability, ProductModel.AvailabilityModel>()
                .ForMember(d => d.StoreId, o => o.MapFrom(s => s.Store.Id));
            CreateMap<Product, ProductModel>();
            CreateMap<Category, CategoryModel>();
        }
    }
}

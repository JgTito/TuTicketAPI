using AutoMapper;
using TuTicketAPI.Dtos.SubcategoriaTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class SubcategoriaTicketProfile : Profile
    {
        public SubcategoriaTicketProfile()
        {
            CreateMap<SubcategoriaTicket, SubcategoriaTicketDto>()
                .ForMember(dest => dest.NombreCategoria, opt => opt.MapFrom(src => src.CategoriaTicket.Nombre));
            CreateMap<CrearSubcategoriaTicketDto, SubcategoriaTicket>();
            CreateMap<ActualizarSubcategoriaTicketDto, SubcategoriaTicket>();
        }
    }
}

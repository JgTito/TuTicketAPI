using AutoMapper;
using TuTicketAPI.Dtos.CategoriaResponsable;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class CategoriaResponsableProfile : Profile
    {
        public CategoriaResponsableProfile()
        {
            CreateMap<CategoriaResponsable, CategoriaResponsableDto>()
                .ForMember(dest => dest.NombreCategoriaTicket, opt => opt.MapFrom(src => src.CategoriaTicket.Nombre))
                .ForMember(dest => dest.NombreUsuarioResponsable, opt => opt.MapFrom(src => src.UsuarioResponsable.NombreCompleto))
                .ForMember(dest => dest.EmailUsuarioResponsable, opt => opt.MapFrom(src => src.UsuarioResponsable.Email));
            CreateMap<CrearCategoriaResponsableDto, CategoriaResponsable>();
            CreateMap<ActualizarCategoriaResponsableDto, CategoriaResponsable>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}

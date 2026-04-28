using AutoMapper;
using TuTicketAPI.Dtos.CategoriaEquipoSoporte;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class CategoriaEquipoSoporteProfile : Profile
    {
        public CategoriaEquipoSoporteProfile()
        {
            CreateMap<CategoriaEquipoSoporte, CategoriaEquipoSoporteDto>()
                .ForMember(dest => dest.NombreCategoriaTicket, opt => opt.MapFrom(src => src.CategoriaTicket.Nombre))
                .ForMember(dest => dest.NombreEquipoSoporte, opt => opt.MapFrom(src => src.EquipoSoporte.Nombre));
            CreateMap<CrearCategoriaEquipoSoporteDto, CategoriaEquipoSoporte>();
            CreateMap<ActualizarCategoriaEquipoSoporteDto, CategoriaEquipoSoporte>();
        }
    }
}

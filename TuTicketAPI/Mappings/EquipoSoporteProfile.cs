using AutoMapper;
using TuTicketAPI.Dtos.EquipoSoporte;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class EquipoSoporteProfile : Profile
    {
        public EquipoSoporteProfile()
        {
            CreateMap<EquipoSoporte, EquipoSoporteDto>();
            CreateMap<CrearEquipoSoporteDto, EquipoSoporte>();
            CreateMap<ActualizarEquipoSoporteDto, EquipoSoporte>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}

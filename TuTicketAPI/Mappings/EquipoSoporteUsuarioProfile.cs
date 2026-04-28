using AutoMapper;
using TuTicketAPI.Dtos.EquipoSoporteUsuario;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class EquipoSoporteUsuarioProfile : Profile
    {
        public EquipoSoporteUsuarioProfile()
        {
            CreateMap<EquipoSoporteUsuario, EquipoSoporteUsuarioDto>()
                .ForMember(dest => dest.NombreEquipoSoporte, opt => opt.MapFrom(src => src.EquipoSoporte.Nombre))
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.NombreCompleto))
                .ForMember(dest => dest.EmailUsuario, opt => opt.MapFrom(src => src.Usuario.Email));
            CreateMap<CrearEquipoSoporteUsuarioDto, EquipoSoporteUsuario>();
            CreateMap<ActualizarEquipoSoporteUsuarioDto, EquipoSoporteUsuario>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}

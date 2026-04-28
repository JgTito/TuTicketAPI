using AutoMapper;
using TuTicketAPI.Dtos.Ticket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.NombreEstadoTicket, opt => opt.MapFrom(src => src.EstadoTicket.Nombre))
                .ForMember(dest => dest.NombrePrioridadTicket, opt => opt.MapFrom(src => src.PrioridadTicket.Nombre))
                .ForMember(dest => dest.NombreSubcategoriaTicket, opt => opt.MapFrom(src => src.SubcategoriaTicket.Nombre))
                .ForMember(dest => dest.IdCategoriaTicket, opt => opt.MapFrom(src => src.SubcategoriaTicket.IdCategoriaTicket))
                .ForMember(dest => dest.NombreCategoriaTicket, opt => opt.MapFrom(src => src.SubcategoriaTicket.CategoriaTicket.Nombre))
                .ForMember(dest => dest.NombreUsuarioSolicitante, opt => opt.MapFrom(src => src.UsuarioSolicitante.NombreCompleto))
                .ForMember(dest => dest.NombreUsuarioAsignado, opt => opt.MapFrom(src => src.UsuarioAsignado == null ? null : src.UsuarioAsignado.NombreCompleto));
            CreateMap<CrearTicketDto, Ticket>()
                .ForMember(dest => dest.Codigo, opt => opt.Ignore());
        }
    }
}

using AutoMapper;
using TuTicketAPI.Dtos.Notificacion;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class NotificacionProfile : Profile
    {
        public NotificacionProfile()
        {
            CreateMap<Notificacion, NotificacionDto>()
                .ForMember(dest => dest.NombreUsuarioDestino, opt => opt.MapFrom(src => src.UsuarioDestino.NombreCompleto))
                .ForMember(dest => dest.CodigoTicket, opt => opt.MapFrom(src => src.Ticket == null ? null : src.Ticket.Codigo));
            CreateMap<CrearNotificacionDto, Notificacion>();
        }
    }
}

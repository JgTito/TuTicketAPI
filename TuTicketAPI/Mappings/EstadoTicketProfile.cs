using AutoMapper;
using TuTicketAPI.Dtos.EstadoTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class EstadoTicketProfile : Profile
    {
        public EstadoTicketProfile()
        {
            CreateMap<EstadoTicket, EstadoTicketDto>();
            CreateMap<CrearEstadoTicketDto, EstadoTicket>();
            CreateMap<ActualizarEstadoTicketDto, EstadoTicket>();
        }
    }
}

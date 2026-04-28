using AutoMapper;
using TuTicketAPI.Dtos.PrioridadTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class PrioridadTicketProfile : Profile
    {
        public PrioridadTicketProfile()
        {
            CreateMap<PrioridadTicket, PrioridadTicketDto>();
            CreateMap<CrearPrioridadTicketDto, PrioridadTicket>();
            CreateMap<ActualizarPrioridadTicketDto, PrioridadTicket>();
        }
    }
}

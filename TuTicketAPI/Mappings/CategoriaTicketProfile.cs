using AutoMapper;
using TuTicketAPI.Dtos.CategoriaTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class CategoriaTicketProfile : Profile
    {
        public CategoriaTicketProfile()
        {
            CreateMap<CategoriaTicket, CategoriaTicketDto>();
            CreateMap<CrearCategoriaTicketDto, CategoriaTicket>();
            CreateMap<ActualizarCategoriaTicketDto, CategoriaTicket>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}

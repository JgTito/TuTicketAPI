using AutoMapper;
using TuTicketAPI.Dtos.SlaPolitica;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class SlaPoliticaProfile : Profile
    {
        public SlaPoliticaProfile()
        {
            CreateMap<SlaPolitica, SlaPoliticaDto>();
            CreateMap<CrearSlaPoliticaDto, SlaPolitica>();
            CreateMap<ActualizarSlaPoliticaDto, SlaPolitica>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}

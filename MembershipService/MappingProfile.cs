using AutoMapper;
using MembershipService.DTOs;
using MembershipService.Models;

namespace MembershipService
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // TipoMembresia Mappings
            CreateMap<TipoMembresia, TipoMembresiaDto>().ReverseMap();
            CreateMap<CreateTipoMembresiaDto, TipoMembresia>();
            CreateMap<UpdateTipoMembresiaDto, TipoMembresia>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Membresia Mappings
            CreateMap<Membresia, MembresiaDto>()
                .ForMember(dest => dest.NombreTipoMembresia, opt => opt.MapFrom(src => src.TipoMembresia != null ? src.TipoMembresia.Nombre : null));
            CreateMap<CreateMembresiaDto, Membresia>();
            CreateMap<UpdateMembresiaDto, Membresia>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<Pago, PagoDto>().ReverseMap();
            CreateMap<CreatePagoDto, Pago>();            
        }
    }
}
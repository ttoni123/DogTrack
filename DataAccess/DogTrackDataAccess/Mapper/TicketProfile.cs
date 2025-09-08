using AutoMapper;
using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using DogTrack.Models.Enums;
using TicketStatus = DogTrack.Models.Enums.TicketStatus;

namespace DogTrack.DataAccess.DogTrackDataAccess.Mapper
{
    internal class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<Ticket, Models.Ticket>()
                .ForMember
                (
                    dest => dest.TicketStatus,
                    map => map.MapFrom(src => (TicketStatus) src.TicketStatusId)
                )
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<InvalidTicket, Models.InvalidTicket>()
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<WonTicket, Models.WonTicket>()
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }


    }
}

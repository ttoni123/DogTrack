using AutoMapper;
using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using DogTrack.Models;
using BetType = DogTrack.Models.Enums.BetType;

namespace DogTrack.DataAccess.DogTrackDataAccess.Mapper
{
    internal class BetProfile : Profile
    {
        public BetProfile()
        {
            CreateMap<AvailableBet, Bet>()
                .ForMember
                (
                    dest => dest.BetId,
                    map => map.MapFrom(src => src.AvailableBetId)
                )
                .ForMember
                (
                    dest => dest.BetType,
                    map => map.MapFrom(src => (BetType) src.BetType.BetTypeId)
                )
                .ForMember
                (
                    dest => dest.RaceId,
                    map => map.MapFrom(src => src.RaceParticipant.RaceId)
                )
                .ForMember
                (
                    dest => dest.ParticipantId,
                    map => map.MapFrom(src => src.RaceParticipant.ParticipantId)
                )
                .ForMember
                (
                    dest => dest.Odds,
                    map => map.MapFrom(src => src.Odds)
                )
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }


    }
}

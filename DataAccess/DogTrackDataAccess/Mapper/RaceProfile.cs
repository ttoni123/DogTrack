using AutoMapper;
using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using DogTrack.Models;

namespace DogTrack.DataAccess.DogTrackDataAccess.Mapper
{
    internal class RaceProfile : Profile
    {
        public RaceProfile()
        {
            CreateMap<Entities.Race, Models.Race>()
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }


    }
}

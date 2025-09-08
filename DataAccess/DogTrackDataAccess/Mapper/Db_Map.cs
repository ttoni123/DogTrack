using AutoMapper;
using DogTrack.DataAccess.DogTrackDataAccess.Entities;
using Serilog;
using System.Reflection;

namespace DogTrack.DataAccess.DogTrackDataAccess.Mapper
{
    public static class Db_Map
    {
        public static IMapper Mapper
        {
            get;
        }

        static Db_Map()
        {
            var configuration = new MapperConfiguration
            (
                cfg =>
                {
                    cfg.AddMaps(typeof(XSdbContext).Assembly);
                    cfg.AddMaps(Assembly.GetExecutingAssembly());
                }
            );

            Mapper = configuration.CreateMapper();
        }

    }
}




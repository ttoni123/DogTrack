
using DogTrack.Models.Enums;

namespace DogTrack.Models
{
    public class Bet
    {        
        public int BetId
        {
            get; set; 
        }

        public int ParticipantId
        {
            get; set;
        }

        public int RaceId
        {
            get; set;
        }

        public BetType BetType
        {
            get; set;
        }

        public decimal Odds
        {
            get; set;
        }
    }
}

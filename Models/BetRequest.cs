using DogTrack.Models.Enums;
using FluentValidation;

namespace DogTrack.Models
{
    public class BetRequest
    {
        public int BetId 
        {
            get; set; 
        }

        public int RaceParticipantId
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

    public class BetRequestValidator : AbstractValidator<BetRequest>
    {
        public BetRequestValidator()
        {
            RuleFor(x => x.BetId).NotNull().GreaterThan(0);
            RuleFor(x => x.RaceParticipantId).NotNull().GreaterThan(0);
        }
    }
}

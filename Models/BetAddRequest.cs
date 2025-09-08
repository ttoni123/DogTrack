using DogTrack.Models.Enums;
using FluentValidation;

namespace DogTrack.Models
{
    public class BetAddRequest
    {
        public decimal BetAmount
        {
            get; set;
        }

        public List<BetRequest> Bets
        {
            get; set;
        } = null!;
    }

    public class BetAddRequestValidator : AbstractValidator<BetAddRequest>
    {
        public BetAddRequestValidator()
        {
            RuleFor(x => x.BetAmount).NotNull().GreaterThan(0);
            RuleForEach(x => x.Bets).SetValidator(new BetRequestValidator());
        }
    }
}

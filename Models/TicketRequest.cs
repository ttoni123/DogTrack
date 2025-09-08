using FluentValidation;

namespace DogTrack.Models
{

    public class TicketRequest
    {
        public int TicketId
        {
            get; set;
        }
    }

    public class TicketRequestValidator : AbstractValidator<TicketRequest>
    {
        public TicketRequestValidator()
        {
            RuleFor(x => x.TicketId).NotNull().GreaterThan(0);
        }
    }
}
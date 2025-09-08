namespace DogTrack.Models
{
    public class TicketDetails : Ticket
    {
        public List<Bet> Bets
        {
            get; set;
        } = null!;
    }
}


using DogTrack.Models.Enums;

namespace DogTrack.Models
{
    public class WonTicket
    {
        public int TicketId
        {
            get; set;
        }

        public int UserId
        {
            get; set;
        }

        public decimal WinAmount
        {
            get; set;
        }

    }
}

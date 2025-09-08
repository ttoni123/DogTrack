
using DogTrack.Models.Enums;

namespace DogTrack.Models
{
    public class Ticket
    {
        public int TicketId 
        {
            get; set; 
        }

        public TicketStatus TicketStatus
        {
            get; set;
        }

        public decimal WinAmount 
        {
            get; set;
        }

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CTC.Models.Leader
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]

        public int UserId { get; set; }
        [Required]
        public DateTime? AppointmentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<Appointment>? Waiting { get; set; }
        public List<Appointment>? Pending { get; set; }
        public List<Appointment>? Accepted { get; set; }
        [ForeignKey("UserId")]
        public Joiner Joiner { get; set; }
        public string LinkMeeting { get; set; }

    }
}

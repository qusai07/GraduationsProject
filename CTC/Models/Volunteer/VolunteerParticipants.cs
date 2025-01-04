using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Volunteer
{
    public class VolunteerParticipants
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VolunteerId { get; set; }  // Foreign key for User (volunteer)

        [Required]
        public DateTime ParticipationDate { get; set; }  // Date when they joined

        [Required]
        public string Status { get; set; }  // Status of participation (e.g., Pending, Confirmed)

        [Required]
        public int EventId { get; set; }  // Foreign key for event

        [Required]
        public string ParticipateName { get; set; }
        [Required]
        public string ParticipateEmail { get; set; }


        // Navigation property
        public Volunteering Volunteering { get; set; }
        [ForeignKey("EventId")]
        public int VolunteeringId { get; set; }
    }
}

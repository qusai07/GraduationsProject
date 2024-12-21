using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CTC.Models.Volunteer
{
    public class Volunteering
    {
        [Key]
        public int Id { get; set; }
        public string Organization { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Type { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        // New properties to hold the count of current participants and remaining seats
        [Required]
        public int CurrentParticipants { get; set; }
        [NotMapped]
        public int RemainingSeats => MaxParticipants - CurrentParticipants;

        public ICollection<VolunteerParticipants>? VolunteerParticipants { get; set; }  // Add this line


    }

}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CTC.Models.Event
{
    public class AttendanceAtEveryEvent
    {
        [Key]
        public int AttendanceId { get; set; }
        [Required]
        public User Student { get; set; }
        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public EventsCTC Event { get; set; }
        public ICollection<AttendanceAtEveryEvent> Attendances { get; set; }

    }

}

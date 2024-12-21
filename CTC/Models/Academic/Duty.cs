namespace CTC.Models.Academic
{
    public class Duty
    {
        public int DutyId { get; set; }

        public int MemberId { get; set; }

        public string Description { get; set; }

        public DateTime AssignedDate { get; set; }


        public DateTime? Deadline { get; set; }

        public string Status { get; set; }


        public virtual User Member { get; set; }
    }
}

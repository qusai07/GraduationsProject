namespace CTC.Models.Admin
{
    public class FormJoinsSettings
    {
        public int Id { get; set; }
        public bool IsJoinFormEnabled { get; set; }
        public DateTime? FormStartDate { get; set; }
        public DateTime? FormEndDate { get; set; }
        public string DisabledMessage { get; set; }
    }
}


namespace CTC.Models.Admin
{
    public class JoinFormSetting
    {
        public int Id { get; set; }
        public bool IsJoinFormEnabledBool { get; set; }
        public string IsJoinFormEnabled
        {
            get { return IsJoinFormEnabledBool ? "open" : "closed"; }
            set { IsJoinFormEnabledBool = value == "open"; }
        }
        public DateTime? FormStartDate { get; set; }
        public DateTime? FormEndDate { get; set; }
        public string? DisabledMessage { get; set; }
    }

}

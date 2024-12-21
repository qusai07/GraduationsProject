using CTC.Repository.Enum;

namespace CTC.Models.Academic
{
    public class Facultymembers
    {

        public string MemberName { get; set; }

        public int Id { get; set; }
        public string Email { get; set; }
        public string NameDoctor { get; set; }
        public string prefx { get; set; }
        public Department department { get; set; }
        public bool Approved { get; set; }
        public string UserId { get; set; }

    }
}

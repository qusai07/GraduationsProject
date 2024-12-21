using CTC.Repository.Enum;
using System.ComponentModel.DataAnnotations;
namespace CTC.Models.Leader
{
    public class Joiner
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string UniversityID { get; set; }
        public string UniversityEmail { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }
        public Department Department { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string LinkedIn { get; set; }
        public string Facebook { get; set; }
        public string YourMessage { get; set; }
        public string? Status { get; set; }
        public ICollection<Appointment> Appointment { get; set; }


    }

}

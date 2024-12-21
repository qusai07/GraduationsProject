using CTC.Repository.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Event
{
    public class EventsCTC
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Location { get; set; }
        public DateTime EventDate { get; set; }

        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
     
        public List<User> volunteers { get; set; } = new List<User>();
        public string? QRCodeText { get; set; }

        public  string EventType { get; set; }


    }
}

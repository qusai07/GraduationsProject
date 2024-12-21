using System.ComponentModel.DataAnnotations;

namespace CTC.Models.Admin
{
    public class QRCodeModel
    {
        [Display(Name = "Enter QRCode Text")]
        [MaxLength(100)]
        public string QRCodeText { get; set; }
        public int EventId { get; set; }
        public int Id { get; set; }


        //public DateOnly StartDate { get; set; }
        //public DateOnly EndDate { get; set; }


    }
}

using CTC.Models.Academic;

namespace CTC.ViewModels
{
    public class ListOfInfo
    {

        public IEnumerable<MaterialSummary> materialSummaries { get; set; }
        public IEnumerable<Facultymembers> facultymembers { get; set; }

    }
}

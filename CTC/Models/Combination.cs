using CTC.Models.Academic;
using CTC.Models.Admin;
using CTC.Models.MediaModels;

namespace CTC.Models
{
    public class Combination
    {
        public WhoWeAre WhoWeAre { get; set; }
        public Videohome VideoHome { get; set; }
        public FeaturesApp featureApp { get; set; }
        public Nahno Nahno { get; set; }
        public ContactMessage ContactMessage { get; set; }
        public List<FeaturesApp> FeatureApp { get; set; }
        public List<Founders> Founders { get; set; }
        public Esports esports { get; set; }

        public List<Sponser> sponsers { get; set; }


        public List<BachelorPrograms> BachelorPrograms { get; set; }

        public CtcData CtcData { get; set; }
        public FormJoinsSettings FormJoinsSettings { get; set; }  

    }
}

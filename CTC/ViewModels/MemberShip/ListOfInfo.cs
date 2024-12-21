using CTC.Models;
using CTC.Models.Event;
using CTC.Models.Leader;

namespace CTC.ViewModels.MemberShip
{
    public class ListOfInfo
    {
        public List<User> users {  get; set; }
        public IEnumerable<Joiner> joiner { get; set; }
        public IEnumerable<EventsCTC> events { get; set; }

    }
}

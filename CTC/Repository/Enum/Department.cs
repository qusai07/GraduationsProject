using System.ComponentModel.DataAnnotations;

namespace CTC.Repository.Enum
{
    public enum Department
    {
        [Display(Name = "Computer Engineering")]
        ComputerEngineering = 1,

        [Display(Name = "Computer Science")]
        ComputerScience = 2,

        [Display(Name = "Computer Network Engineering and Security")]
        ComputerNetworkEngineeringAndSecurity = 3,

        [Display(Name = "Software Engineering")]
        SoftwareEngineering = 4,

        [Display(Name = "Cyber Security")]
        CyberSecurity = 5,

        [Display(Name = "Computer Game Design and Development")]
        ComputerGameDesignAndDevelopment = 6,

        [Display(Name = "Health Information System")]
        HealthInformationSystem = 7,

        [Display(Name = "Internet of Things")]
        InternetOfThings = 8,

        [Display(Name = "Data Science")]
        DataScience = 9,

        [Display(Name = "Artificial Intelligence")]
        ArtificialIntelligence = 10,

        [Display(Name = "Robotics")]
        Robotics = 11
    }
}

using System;

namespace thwala_attorneys.Models
{
    public class AttorneyProfileViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Qualification { get; set; }
        public string Specialization { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Bio { get; set; }
        public List<string> PracticeAreas { get; set; }
        public List<string> Education { get; set; }
        public List<string> Achievements { get; set; }
        public int YearsOfExperience { get; set; }
        public string Languages { get; set; }
        public List<CaseStudy> CaseStudies { get; set; }

        public AttorneyProfileViewModel()
        {
            PracticeAreas = new List<string>();
            Education = new List<string>();
            Achievements = new List<string>();
            CaseStudies = new List<CaseStudy>();
        }
    }

    public class CaseStudy
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Outcome { get; set; }
    }
}
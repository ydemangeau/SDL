using System;

namespace SDLL
{
    [Serializable]
    public class ApplicationUser
    {
        public int IDUser { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

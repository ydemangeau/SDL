using System;

namespace SDLL
{
    [Serializable]
    public class Student : ApplicationUser
    {

        public Class Class { get; set; }

        public static string GetClass(string group) => group.Substring(3, group.IndexOf(',') - 3);

        public Student()
        {
            Class = new Class();
        }
    }
}

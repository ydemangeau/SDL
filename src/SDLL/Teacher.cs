using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SDLL
{
    [Serializable]
    public class Teacher : ApplicationUser
    {
        public string Subject { get; set; }

        public static string GetSubject(string group)
        {
            // return group.Substring(group.IndexOf(",OU="), group.IndexOf(',') - 3);

            Regex regex = new Regex(@"OU=([\w]+)");

            Match m = regex.Match(group);
            return m.Groups[0].Value.Substring(3);
        }

        public void Save(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Teacher));
            StreamWriter streamWriter = new StreamWriter(path);
            xmlSerializer.Serialize(streamWriter, this);
            streamWriter.Close();
        }

        public static Teacher Load(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Teacher));
            StreamReader streamReader = new StreamReader(path);
            Teacher teacher = xmlSerializer.Deserialize(streamReader) as Teacher;
            streamReader.Close();

            return teacher;
        }
    }
}

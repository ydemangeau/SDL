using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SDLE.Models
{
    public class User
    {
        [NonSerialized]
        private List<string> groups;

        // Valeur par défaut : -2
        // Si existe pas dans la base de données : -1
        public int IdEtudiant;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<string> Groups { get => groups; set => groups = value; }

        public User()
        {
            Groups = new List<string>();
        }
    }

    [Serializable]
    public class Users : List<User>
    {
        public void Save(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Users));
            StreamWriter streamWriter = new StreamWriter(path);
            xmlSerializer.Serialize(streamWriter, this);
            streamWriter.Close();
        }

        public static Users Load(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Users));
            StreamReader streamReader = new StreamReader(path);
            Users users = xmlSerializer.Deserialize(streamReader) as Users;
            streamReader.Close();

            return users;
        }
    }
}
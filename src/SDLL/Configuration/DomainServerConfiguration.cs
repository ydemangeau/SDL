using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SDLL.Configuration
{
    [Serializable]
    public class DomainServerConfiguration
    {
        [Required]
        [Display(Name = "Adresse IP du serveur")]
        public string IPAddress { get; set; }

        [Required]
        [Display(Name = "Nom du domaine")]
        public string DomainName { get; set; }

        public static DomainServerConfiguration Charger() => Charger(Path.Combine(Directory.GetCurrentDirectory(), "serveurDomaine.bin"));

        public static DomainServerConfiguration Charger(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream flux = null;

            try
            {
                flux = new FileStream(path, FileMode.Open, FileAccess.Read);

                return formatter.Deserialize(flux) as DomainServerConfiguration;
            }
            catch (Exception)
            {
                return default(DomainServerConfiguration);
            }
            finally
            {
                if (flux != null)
                    flux.Close();
            }
        }

        public void Sauvegarder() => Sauvegarder(Path.Combine(Directory.GetCurrentDirectory(), "serveurDomaine.bin"));

        public void Sauvegarder(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                formatter.Serialize(fileStream, this);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDLL.Configuration
{
    [Serializable]
    public class BDDConfiguration
    {
        /// <summary>
        /// Obtient ou définit l'adresse IP du serveur de base de données.
        /// </summary>
        [Required]
        [Display(Name = "Adresse IP")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de la base de données.
        /// </summary>
        
        [Required]
        [Display(Name = "Nom de la base de données")]
        public string BDDName { get; set; }
        /// <summary>
        /// Obtient ou définit le nom d'utilisateur qui se connecte à la base de données.
        /// </summary>

        [Required]
        [Display(Name = "Nom d'utilisateur")]
        public string User { get; set; }
        /// <summary>
        /// Obtient ou définit le mot de passe associé à l'utilisateur qui se connecte à la base de données.
        /// </summary>

        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        /// <summary>
        /// Sauvegarde les informations de connexion à la base de données dans un fichier.
        /// </summary>
        /// <remarks>Le fichier est chiffré pour sécuriser les informations sensibles.</remarks>
        public void Save() => Save(Path.Combine(Directory.GetCurrentDirectory(), "bdd.bin"));
        /// <summary>
        /// Sauvegarde les informations de connexion à la base de données dans un fichier.
        /// </summary>
        /// <param name="path">Le chemin ou sera enregistré le fichier de configuration.</param>
        /// <remarks>Le fichier est chiffré pour sécuriser les informations sensibles.</remarks>
        public void Save(string path)
        {
            byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                using (var cryptoStream = new CryptoStream(fileStream, des.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(cryptoStream, this);
                }
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// Charge les informations de connexion à la base de données.
        /// </summary>
        /// <returns>Les informations de connexion  à la base de données.</returns>
        public static BDDConfiguration Load() => Load(Path.Combine(Directory.GetCurrentDirectory(), "bdd.bin"));

        public static BDDConfiguration Load(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream flux = null;

            try
            {
                byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };

                DESCryptoServiceProvider des = new DESCryptoServiceProvider();

                flux = new FileStream(path, FileMode.Open, FileAccess.Read);

                using (var cryptoStream = new CryptoStream(flux, des.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    formatter = new BinaryFormatter();

                    return formatter.Deserialize(cryptoStream) as BDDConfiguration;
                }
            }
            catch
            {
                return default(BDDConfiguration);
            }
            finally
            {
                if (flux != null)
                    flux.Close();
            }
        }
    }
}

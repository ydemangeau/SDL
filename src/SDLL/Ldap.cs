using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace SDLL
{
    /// <summary>
    /// Lier un serveur LDAP à une application.
    /// </summary>
    public class Ldap : IMessage
    {
        private MessageBoxImage messageBoxImage;
        private string information;

        public static bool IsAuthificated;

        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public MessageBoxImage MessageBoxImage { get => messageBoxImage; }

        public string CodeErreur { get; }

        public string Information { get => information; }

        public Ldap(string domain) => Domain = domain;

        /// <summary>
        /// Initialiser une nouvelle instance de <see cref="Ldap"/>
        /// </summary>
        public Ldap(string domain, string username, string password)
        {
            Domain = domain;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Vérifier que l'identifiant et le mot de passe entré existe.
        /// </summary>
        public bool Authentification() => Authentification(Username, Password);

        /// <summary>
        /// Vérifier que l'identifiant et le mot de passe entre existe.
        /// </summary>
        /// <param name="login">Identifiant entre dans le formulaire.</param>
        /// <param name="motDePasse">Mot de pass entre dans le formulaire.</param>
        /// <returns></returns>
        public bool Authentification(string username, string password)
        {
            try
            {
                DirectoryEntry loginEntry = new DirectoryEntry("LDAP://" + Domain, username, password, AuthenticationTypes.Secure);
                DirectorySearcher loginSearcher = new DirectorySearcher(loginEntry) { Filter = "(&(objectClass=user)(SAMAccountName=" + username + "))",
                    SearchScope = SearchScope.Subtree };
                SearchResult loginResult = loginSearcher.FindOne();

                DirectoryEntry passwordEntry = new DirectoryEntry("LDAP://" + Domain, username, password, AuthenticationTypes.Secure);
                DirectorySearcher passwordSearcher = new DirectorySearcher(passwordEntry) { Filter = "(objectClass=user)" };
                SearchResult passwordResult = passwordSearcher.FindOne();

                return true;
            }
            catch (Exception)
            {
                information = "Nom d'utilisateur et/ou mot de passe incorrect. Veuillez vérifier vos informations de connexion";
                messageBoxImage = MessageBoxImage.Information;

                return false;
            }
        }

        /// <summary>
        /// Récupérer le Nom de l'utilisateur.
        /// </summary>
        public string GetLastName() => GetLastName(Username, Password);

        /// <summary>
        /// Récupérer le Nom de l'utilisateur.
        /// </summary>
        /// <param name="username">L'utilisateur dont le nom doit être récupéré.</param>
        /// <param name="password">Le mot de passe associé à l'utilisateur.</param>
        /// <returns></returns>
        public string GetLastName(string username, string password)
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain, username, password);
                DirectorySearcher search = new DirectorySearcher(entry) { Filter = "(&(objectClass=user)(SAMAccountName=" + username + "))" };
                SearchResult result = search.FindOne();
                DirectoryEntry DirEntry = result.GetDirectoryEntry();
                return (DirEntry.Properties["sn"].Value.ToString());
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Récupérer le prénom de l'utilisateur.
        /// </summary>
        public string GetFirstName() => GetFirstName(Username, Password);

        /// <summary>
        /// Récupérer le prénom de l'utilisateur.
        /// </summary>
        /// <param name="username">L'utilisateur dont le prénom doit être récupéré.</param>
        /// <param name="password">Le mot de passe associé à l'utilisateur.</param>
        /// <returns></returns>
        public string GetFirstName(string username, string password)
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain, username, password);
                DirectorySearcher search = new DirectorySearcher(entry) { Filter = "(&(objectClass=user)(SAMAccountName=" + username + "))" };
                SearchResult result = search.FindOne();
                DirectoryEntry DirEntry = result.GetDirectoryEntry();
                return (DirEntry.Properties["givenName"].Value.ToString());
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Récupérer le groupe de l'utilisateur.
        /// </summary>
        public List<string> GetGroup() => GetGroup(Username, Password);

        /// <summary>
        /// Récupérer le groupe de l'utilisateur.
        /// </summary>
        /// <param name="username">L'utilisateur dont le groupe doit être récupéré</param>
        /// <param name="password">Le mot de passe associé à l'utilisateur.</param>
        /// <returns></returns>
        public List<string> GetGroup(string username, string password)
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain, username, password);
                DirectorySearcher searcher = new DirectorySearcher(entry) { Filter = "(&(objectClass=user)(SAMAccountName=" + username + "))" };
                SearchResult result = searcher.FindOne();

                DirectoryEntry DirEntry = result.GetDirectoryEntry();

                List<string> memberof = new List<string>();

                foreach (object oMember in DirEntry.Properties["distinguishedName"])
                {
                    memberof.Add(oMember.ToString());
                }

                return memberof;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public BitmapImage GetUserPicture() => GetUserPicture(Username, Password);

        public BitmapImage GetUserPicture(string username, string password)
        {
            var fs = new MemoryStream();
            var bitmap = new BitmapImage();
            try
            {
                var de = new DirectoryEntry("LDAP://" + Domain) { Username = username, Password = password };
                var forceAuth = de.NativeObject;
                var wr = new BinaryWriter(fs);
                byte[] bb = (byte[])de.Properties["jpegPhoto"][0];
                wr.Write(bb);
                wr.Close();

                using (var ms = fs)
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return bitmap;
        }
    }
}

using Dev2Be.ClassExtension;
using SDLL.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SDLL
{
    /// <summary>
    /// Représente l'application utilisé.
    /// </summary>
    public enum Application
    {
        /// <summary>
        /// Définit l'utilisation de l'application élève.
        /// </summary>
        Student,
        /// <summary>
        /// Défnit l'utilisation de l'application professeur.
        /// </summary>
        Teacher
    }

    /// <summary>
    /// Permet de lier une application à une base de données et récupérer les informations comprises dedans.
    /// </summary>
    public class BDDAccess : IMessage
    {
        #region Variables
        private MessageBoxImage messageBoxImage;

        private string codeErreur;
        private string information;

        private SqlConnection sqlConnection;

        private BDDConfiguration bddConfiguration;

        public MessageBoxImage MessageBoxImage { get => messageBoxImage; }

        public string CodeErreur { get => codeErreur; }

        public string Information { get => information; }
        #endregion

        /// <summary>
        /// Initialiser une nouvelle instance de <see cref="AccesBdd"/>
        /// </summary>
        public BDDAccess() { }

        public bool Connect() => Connect(Path.Combine(Directory.GetCurrentDirectory(), "bdd.bin"));

        /// <summary>
        /// S'authentifier sur la base de données
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool Connect(string path)
        {
            if (!File.Exists(path))
            {
                information = "Une erreur a été rencontrée lors de la récupération des informations de connexion à la base de données." + Environment.NewLine + "Veuillez réenregistrer les informations avant de recommencer.";
                messageBoxImage = MessageBoxImage.Error;

                codeErreur = "0x0008";
                return false;
            }

            bddConfiguration = BDDConfiguration.Load(path);

            if (bddConfiguration == null)
            {
                codeErreur = "0x0009";
                return false;
            }

            if (bddConfiguration.IPAddress == null || bddConfiguration.BDDName == null || bddConfiguration.User == null)
                throw new ArgumentNullException();

            string connectionString = "Data Source=" + bddConfiguration.IPAddress + "; Initial Catalog =" + bddConfiguration.BDDName + "; User ID = " + bddConfiguration.User + "; Password = " + bddConfiguration.Password + ";";

            sqlConnection = new SqlConnection(connectionString);

            try
            {
                sqlConnection.Open();

                return true;
            }
            catch (SqlException)
            {
                information = "Impossible de se connecter à la base de données. Vérifier vos informations de connection." + Environment.NewLine + "Si le problème persite conctactez votre administrateur réseau.";
                messageBoxImage = MessageBoxImage.Error;
                codeErreur = "0x0002";

                return false;
            }
            catch (Exception ex)
            {
                information = ex.Message;
                messageBoxImage = MessageBoxImage.Error;

                codeErreur = "0x0000";

                return false;
            }
        }

        /// <summary>
        /// Se déconnecter de la base de données.
        /// </summary>
        private void Deconnect()
        {
            if (sqlConnection != null)
                sqlConnection.Close();
        }

        /// <summary>
        /// Vérifie le statut de connexion à la base de données.
        /// </summary>
        /// <returns>Le statut de connexion à la base de données.</returns>
        private bool IsConnected()
        {
            if (sqlConnection == null || sqlConnection.State != System.Data.ConnectionState.Open)
            {
                information = "Veuillez vous connecter à la base de données pour continuer.";
                messageBoxImage = MessageBoxImage.Information;

                codeErreur = "0x0002";

                return false;
            }

            return true;
        }

        public List<Activity> GetActivities(Teacher teacher)
        {
            List<Activity> activites = new List<Activity>();

            if (!IsConnected())
                return default(List<Activity>);

            try
            {
                // Création de la commande
                SqlCommand command = sqlConnection.CreateCommand();

                command.CommandText = "SELECT * FROM Activite WHERE NomProfesseur LIKE @teacherName";
                command.Parameters.AddWithValue("@teacherName", teacher.FirstName + " " + teacher.LastName);

                // Lecture du résultat de la commande
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    activites.Add(new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), ActivityVisibility = dataReader.GetBoolean(dataReader.GetOrdinal("Visibilite")) });

                dataReader.Close();

                return activites;
            }
            catch (Exception e)
            {
                information = e.Message;
                messageBoxImage = MessageBoxImage.Error;

                return default(List<Activity>);
            }
        }

        public List<Activity> GetActivities(Teacher teacher, string filter)
        {
            List<Activity> activites = new List<Activity>();

            if (!IsConnected())
                return default(List<Activity>);

            try
            {
                // Création de la commande
                SqlCommand command = sqlConnection.CreateCommand();

                command.CommandText = "SELECT * FROM Activite WHERE NomProfesseur LIKE @teacherName AND Nom LIKE @filter";
                command.Parameters.AddWithValue("@teacherName", teacher.FirstName + " " + teacher.LastName);
                command.Parameters.AddWithValue("@filter", "%"+filter+"%");

                // Lecture du résultat de la commande
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    activites.Add(new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), ActivityVisibility = dataReader.GetBoolean(dataReader.GetOrdinal("Visibilite")) });

                dataReader.Close();

                return activites;
            }
            catch (Exception e)
            {
                information = e.Message;
                messageBoxImage = MessageBoxImage.Error;

                return default(List<Activity>);
            }
        }
        
        /// <summary>
        /// Récuperer la liste des activités présentes sur la bases de données.
        /// </summary>
        /// <param name="message">Message d'erreur retourné par la fonction.</param>
        /// <returns></returns>
        public List<Activity> GetActivities(Student student, ActivitiesVisibility activitiesVisibility)
        {
            List<Activity> activites = new List<Activity>();
            
            if (!IsConnected())
                return default(List<Activity>);

            try
            {
                // Création de la commande
                SqlCommand command = sqlConnection.CreateCommand();
                switch (activitiesVisibility)
                {
                    case ActivitiesVisibility.Both:
                        command.CommandText = "SELECT * FROM Activite, AffectationClasse, Classe, Etudiant WHERE Activite.IDActivite = AffectationClasse.IDActivite AND AffectationClasse.IDClasse = Classe.IDClasse AND Classe.IDClasse = Etudiant.IDClasse AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName";
                        break;
                    case ActivitiesVisibility.Hidden:
                        command.CommandText = "SELECT * FROM Activite, AffectationClasse, Classe, Etudiant WHERE Visibilite = 0 AND Activite.IDActivite = AffectationClasse.IDActivite AND AffectationClasse.IDClasse = Classe.IDClasse AND Classe.IDClasse = Etudiant.IDClasse AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName";
                        break;
                    case ActivitiesVisibility.Visible:
                        command.CommandText = "SELECT * FROM Activite, AffectationClasse, Classe, Etudiant WHERE Visibilite = 1 AND Activite.IDActivite = AffectationClasse.IDActivite AND AffectationClasse.IDClasse = Classe.IDClasse AND Classe.IDClasse = Etudiant.IDClasse AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName";
                        break;
                    default:
                        command.CommandText = "SELECT * FROM Activite, AffectationClasse, Classe, Etudiant WHERE Activite.IDActivite = AffectationClasse.IDActivite AND AffectationClasse.IDClasse = Classe.IDClasse AND Classe.IDClasse = Etudiant.IDClasse AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName";
                        break;
                }

                command.Parameters.AddWithValue("@firstName", student.FirstName);
                command.Parameters.AddWithValue("@lastName", student.LastName);

                // Lecture du résultat de la commande
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    activites.Add(new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), Text = dataReader.GetString(dataReader.GetOrdinal("Texte")) });

                dataReader.Close();

              return activites;
            }
            catch (Exception e)
            {
                   information = e.Message;
                messageBoxImage = MessageBoxImage.Error;

                return default(List<Activity>);
            }
        }

        public Activity GetActivity(int id)
        {
            Activity activity = null;

            if (!IsConnected())
                return default(Activity);

            try
            {
                // Création de la commande
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT * FROM Activite WHERE Activite.IDActivite = @idActivite";
                command.Parameters.AddWithValue("@idActivite", id);

                // Lecture du résultat de la commande
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    activity = new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), TeacherName = dataReader.GetString(dataReader.GetOrdinal("NomProfesseur")), Text = dataReader.GetString(dataReader.GetOrdinal("Texte")), ProvidedWords = dataReader.GetString(dataReader.GetOrdinal("MotsFournis")).Split(';').ToList(), ActivityVisibility = dataReader.GetBoolean(dataReader.GetOrdinal("Visibilite")) };
                }

                dataReader.Close();

                return activity;
            }
            catch (Exception e)
            {
                information = e.Message;

                return default(Activity);
            }
        }

        public List<Activity> GetSavedActivities(Student student, ActivitiesVisibility activitiesVisibility)
        {
            List<Activity> activities = new List<Activity>();

            if (!IsConnected())
                return default(List<Activity>);

            try
            {
                SqlCommand command = sqlConnection.CreateCommand();

                switch (activitiesVisibility)
                {
                    case ActivitiesVisibility.Both:
                        command.CommandText = "SELECT * FROM Resultat, Activite, Etudiant, Classe WHERE Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName AND Resultat.IDActivite = Activite.IDActivite AND Etudiant.IDClasse = Classe.IDClasse AND Etudiant.IDEtudiant = Resultat.IDEtudiant";
                        break;
                    case ActivitiesVisibility.Hidden:
                        command.CommandText = "SELECT * FROM Resultat, Activite, Etudiant, Classe WHERE Visibilite = 0 AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName AND Resultat.IDActivite = Activite.IDActivite AND Etudiant.IDClasse = Classe.IDClasse AND Etudiant.IDEtudiant = Resultat.IDEtudiant";
                        break;
                    case ActivitiesVisibility.Visible:
                        command.CommandText = "SELECT * FROM Resultat, Activite, Etudiant, Classe WHERE Visibilite = 1 AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName AND Resultat.IDActivite = Activite.IDActivite AND Etudiant.IDClasse = Classe.IDClasse AND Etudiant.IDEtudiant = Resultat.IDEtudiant";
                        break;
                    default:
                        break;
                }

                command.Parameters.AddWithValue("@lastName", student.LastName);
                command.Parameters.AddWithValue("@firstName", student.FirstName);

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    activities.Add(new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), Text = dataReader.GetString(dataReader.GetOrdinal("Texte")) });

                dataReader.Close();

                return activities;
            }
            catch (Exception e)
            {
                information = e.Message;

                return default(List<Activity>);
            }
        }

        public Activity GetSavedActivity(int id, Student student)
        {
            Activity activity = null;

            if (!IsConnected())
                return default(Activity);

            try
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT * FROM Resultat, Activite, Etudiant, Classe WHERE Visibilite = 1 AND Etudiant.Nom LIKE @lastName AND Etudiant.Prenom LIKE @firstName AND Resultat.IDActivite = Activite.IDActivite AND Activite.IDActivite = @idActivite AND Etudiant.IDClasse = Classe.IDClasse AND Etudiant.IDEtudiant = Resultat.IDEtudiant";
                command.Parameters.AddWithValue("@idActivite", id);
                command.Parameters.AddWithValue("@lastName", student.LastName);
                command.Parameters.AddWithValue("@firstName", student.FirstName);

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    activity = new Activity() { IDActivity = dataReader.GetInt32(dataReader.GetOrdinal("IDActivite")), ActivityName = dataReader.GetString(dataReader.GetOrdinal("Nom")), TeacherName = dataReader.GetString(dataReader.GetOrdinal("NomProfesseur")), Text = dataReader.GetString(dataReader.GetOrdinal("Texte")), ProvidedWords = dataReader.GetString(dataReader.GetOrdinal("MotsFournis")).Split(';').ToList(), FoundWords = dataReader.GetString(dataReader.GetOrdinal("MotsTrouves")).Split(';').ToList(), RefusedWords = dataReader.GetString(dataReader.GetOrdinal("MotsRefuses")).Split(';').ToList() };
                }

                dataReader.Close();

                return activity;
            }
            catch (Exception e)
            {
                information = e.Message;

                return default(Activity);
            }
        }

        public int GetIdEtudiant(string firstName, string lastName)
        {
            int id = -1;

            if (!IsConnected())
                return id;

            try
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT IDEtudiant FROM Etudiant WHERE Nom LIKE @lastName AND Prenom LIKE @firstName";
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@firstName", firstName);

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    id = dataReader.GetInt32(0);

                dataReader.Close();
            }
            catch (Exception ex)
            {
                information = ex.Message;
            }

            return id;
        }

        public void CreateStudent(string firstName, string lastName, string className)
        {
            if (!IsConnected())
                return;

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandText = "INSERT INTO Etudiant (Nom, Prenom, IDClasse) VALUES (@lastName, @firstName, @idClass)";
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);

            int idClass = GetIdClass(className);

            if (idClass == -1)
            {
                CreateClass(className);

                idClass = GetIdClass(className);
            }

            command.Parameters.AddWithValue("@idClass", idClass);

            command.ExecuteNonQuery();
        }

        public List<Class> GetClasses()
        {
            if (!IsConnected())
                return default(List<Class>);

            try
            {
                List<Class> classes = new List<Class>();

                // Commande SQL a envoyer
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT * FROM Classe ORDER BY Nom ASC";

                // Lecture resultat
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    classes.Add(new Class() { ClassName = dataReader.GetString(dataReader.GetOrdinal("Nom")), IDClass = dataReader.GetInt32(dataReader.GetOrdinal("IDClasse")) });

                dataReader.Close();

                return classes;
            }
            catch (Exception ex)
            {
                information = ex.Message;
                messageBoxImage = MessageBoxImage.Error;

                return default(List<Class>);
            }
        }

        public int GetIdClass(string className)
        {
            int idClass = -1;

            if (!IsConnected())
                return idClass;


            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandText = "SELECT Classe.IDClasse FROM Classe WHERE Classe.Nom LIKE @className";
            command.Parameters.AddWithValue("@className", className);

            SqlDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
                idClass = dataReader.GetInt32(0);

            dataReader.Close();

            return idClass;
        }

        public void CreateClass(string className)
        {
            if (!IsConnected())
                return;

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandText = "INSERT INTO Classe (Nom) VALUES (@className)";
            command.Parameters.AddWithValue("@className", className);

            command.ExecuteNonQuery();
        }

        public void SaveActivities(Activity activity)
        {
            try
            {
                if (activity.IDActivity == -2)
                {
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = "INSERT INTO Activite (Nom, NomProfesseur, Texte, Visibilite, MotsFournis) VALUES (@ActivityName,@TeacherName,@Text,@Visibility,@ProvidedWords)";
                    sqlCommand.Parameters.AddWithValue("@ActivityName", activity.ActivityName);
                    sqlCommand.Parameters.AddWithValue("@TeacherName", activity.TeacherName);
                    sqlCommand.Parameters.AddWithValue("@Text", activity.Text);
                    sqlCommand.Parameters.AddWithValue("@Visibility", activity.ActivityVisibility);
                    sqlCommand.Parameters.AddWithValue("@ProvidedWords", string.Join(";", activity.ProvidedWords));

                    sqlCommand.ExecuteNonQuery();

                    activity.IDActivity = GetIDActivity(activity);

                    ClassAfffectation(activity);

                    information = "Activité mise en ligne !";
                    messageBoxImage = MessageBoxImage.Information;
                }
                else
                {
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();

                    // Supprimer l'ensemble des classes affectées à l'activité.
                    sqlCommand.CommandText = "DELETE FROM AffectationClasse WHERE IDActivite = @IDActivite";
                    sqlCommand.Parameters.AddWithValue("@IDActivite", activity.IDActivity);
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Parameters.Clear();

                    // Mettre à jour l'activité dans la base de données.
                    sqlCommand.CommandText = "UPDATE Activite SET Nom = @NomActivite, NomProfesseur = @NomProfesseur, MotsFournis = @MotsFournis, Texte = @Texte, Visibilite = @Visibilite WHERE IDActivite = @IDActivite";
                    sqlCommand.Parameters.AddWithValue("@IDActivite", activity.IDActivity);
                    sqlCommand.Parameters.AddWithValue("@NomActivite", activity.ActivityName);
                    sqlCommand.Parameters.AddWithValue("@NomProfesseur", activity.TeacherName);
                    sqlCommand.Parameters.AddWithValue("@MotsFournis", string.Join(";", activity.ProvidedWords));
                    sqlCommand.Parameters.AddWithValue("@Texte", activity.Text);
                    sqlCommand.Parameters.AddWithValue("@Visibilite", activity.ActivityVisibility);

                    sqlCommand.ExecuteNonQuery();

                    ClassAfffectation(activity);

                    information = "Activité mise à jour !";
                    messageBoxImage = MessageBoxImage.Information;
                }
            }
            catch (Exception e)
            {
                information = e.Message;
                messageBoxImage = MessageBoxImage.Error;

                return;
            }
        }

        public void SaveActivity(ActivityResult activityResult)
        {
            SaveStudentActivity(activityResult);
        }

        private int GetIDActivity(Activity activity)
        {
            int idActivity = -1;

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "SELECT IDActivite FROM Activite WHERE Nom LIKE @ActivityName AND NomProfesseur LIKE @TeacherName";
            sqlCommand.Parameters.AddWithValue("@ActivityName", activity.ActivityName);
            sqlCommand.Parameters.AddWithValue("@TeacherName", activity.TeacherName);

            SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
                idActivity = dataReader.GetInt32(0);

            dataReader.Close();

            return idActivity;
        }

        private void ClassAfffectation(Activity activity)
        {
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "INSERT INTO AffectationClasse (IDClasse,IDActivite) VALUES (@IDClasse,@IDActivite)";

            foreach (var classe in activity.Classes)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@IDClasse", classe.IDClass);
                sqlCommand.Parameters.AddWithValue("@IDActivite", activity.IDActivity);

                sqlCommand.ExecuteNonQuery();
            }
        }

        private void SaveStudentActivity(ActivityResult activityResult)
        {
            activityResult.Student.IDUser = GetIdEtudiant(activityResult.Student.FirstName, activityResult.Student.LastName);

            if (activityResult.Student.IDUser == -1)
            {
                CreateStudent(activityResult.Student.FirstName, activityResult.Student.LastName, activityResult.Student.Class.ClassName);

                activityResult.Student.IDUser = GetIdEtudiant(activityResult.Student.FirstName, activityResult.Student.LastName);
            }

            if (ActivityExists(activityResult))
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "UPDATE Resultat SET MotsTrouves = @MotsTrouves, MotsRefuses = @MotsRefuses FROM Etudiant, Activite WHERE Activite.IDActivite = Resultat.IDActivite AND Resultat.IDEtudiant = Etudiant.IDEtudiant AND Activite.IDActivite = @IDActivite AND Etudiant.IDEtudiant = @IDEtudiant";
                command.Parameters.AddWithValue("@MotsTrouves", string.Join(";", activityResult.FoundWords));
                command.Parameters.AddWithValue("@MotsRefuses", string.Join(";", activityResult.RefusedWords));
                command.Parameters.AddWithValue("@IDActivite", activityResult.IDActivity);
                command.Parameters.AddWithValue("@IDEtudiant", activityResult.Student.IDUser);

                command.ExecuteNonQuery();

                information = "La sauvegarde s'est correctement effectué.";

                messageBoxImage = MessageBoxImage.Information;
            }
            else
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "INSERT INTO Resultat (IDEtudiant, MotsTrouves, MotsRefuses, IDActivite) VALUES (@IDEtudiant, @MotsTrouves, @MotsRefuses, @IDActivite)";
                command.Parameters.AddWithValue("@IDEtudiant", activityResult.Student.IDUser);
                command.Parameters.AddWithValue("@MotsTrouves", string.Join(";", activityResult.FoundWords));
                command.Parameters.AddWithValue("@MotsRefuses", string.Join(";", activityResult.RefusedWords));
                command.Parameters.AddWithValue("@IDActivite", activityResult.IDActivity);

                command.ExecuteNonQuery();
            }
        }

        private bool ActivityExists(ActivityResult activityResult)
        {
            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Resultat, Activite, Etudiant WHERE Activite.IDActivite = Resultat.IDActivite AND Resultat.IDEtudiant = Etudiant.IDEtudiant AND Activite.IDActivite = @IdActivite AND Etudiant.Nom LIKE @NomEtudiant AND Etudiant.Prenom LIKE @PrenomEtudiant";
            command.Parameters.AddWithValue("@IdActivite", activityResult.IDActivity);
            command.Parameters.AddWithValue("@NomEtudiant", activityResult.Student.LastName);
            command.Parameters.AddWithValue("@PrenomEtudiant", activityResult.Student.FirstName);

            SqlDataReader dataReader = command.ExecuteReader();

            int nombreActivite = 0;

            while (dataReader.Read())
                nombreActivite = dataReader.GetInt32(0);

            dataReader.Close();

            if (nombreActivite != 1)
                return false;
            else
                return true;
        }

        public void UpdateVisibility(Activity activity)
        {
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "UPDATE Activite SET Visibilite = @Visibilite WHERE IDActivite = @IDActivite";
            sqlCommand.Parameters.AddWithValue("@IDActivite", activity.IDActivity);
            sqlCommand.Parameters.AddWithValue("@Visibilite", activity.ActivityVisibility);

            sqlCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Supprimer une liste d'activité de la base de données.
        /// </summary>
        /// <param name="activites"></param>
        /// <param name="message"></param>
        public void DeleteActivities(List<Activity> activities)
        {
            if (activities == null)
                throw new ArgumentNullException();

            if (!IsConnected())
                return;

            try
            {
                if (activities.Count <= 0)
                    return;

                SqlCommand command = sqlConnection.CreateCommand();

                for (int i = 0; i < activities.Count; i++)
                {
                    command.CommandText = "DELETE FROM AffectationClasse WHERE AffectationClasse.IDActivite = @IDActivite";
                    command.Parameters.AddWithValue("@IDActivite", activities[i].IDActivity);
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM Activite WHERE Nom LIKE @nomActivite";
                    command.Parameters.AddWithValue("@nomActivite", activities[i].IDActivity);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();

                    command.CommandText = "DELETE FROM Resultat WHERE Resultat.IDActivite = @IDActivite";
                    command.Parameters.AddWithValue("@IDActivite", activities[i].IDActivity);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                information = ex.Message;
                messageBoxImage = MessageBoxImage.Error;
            }
        }

        /// <summary>
        /// Recupère les résultats de l'élève
        /// </summary>
        /// <param name="nomActivite"></param>
        /// <returns></returns>
        public List<ActivityResult> GetStudentResults(Activity activity)
        {
            List<ActivityResult> activityResults = new List<ActivityResult>();
            
            if (!IsConnected())
                return default(List<ActivityResult>);

            try
            {
                string requete = "SELECT Etudiant.Nom, Etudiant.Prenom, MotsTrouves, MotsRefuses, Classe.Nom, Resultat.IDResultat, Activite.Texte, Activite.Nom FROM Resultat, Etudiant, Activite, AffectationClasse, Classe WHERE Etudiant.IDEtudiant = Resultat.IDEtudiant AND Activite.IDActivite = Resultat.IDActivite AND Activite.IDActivite = @IDActivity AND Activite.IDActivite = AffectationClasse.IDActivite AND AffectationClasse.IDClasse = Classe.IDClasse AND Classe.IDClasse = Etudiant.IDClasse";

                SqlCommand command = sqlConnection.CreateCommand();
                command.Parameters.AddWithValue("@IDActivity", activity.IDActivity);
                command.CommandText = requete;
                // Lecture resultat
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    ActivityResult activityResult = new ActivityResult();

                    activityResult.Student.LastName = dataReader.GetString(0);
                    activityResult.Student.FirstName = dataReader.GetString(1);

                    activityResult.FoundWords = new List<string>(dataReader.GetString(2).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    activityResult.RefusedWords = dataReader.GetString(3).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    activityResult.NumberOfWordsTotal = dataReader.GetString(6).Count(Activity.SpecialCharacters);

                    activityResult.Student.Class.ClassName = dataReader.GetString(4);

                    activityResult.IDResult = dataReader.GetInt32(5);
                    activityResult.Text = dataReader.GetString(6);
                    activityResult.ActivityName = dataReader.GetString(7);

                    activityResults.Add(activityResult);
                }

                dataReader.Close();

                return activityResults;
            }
            catch (Exception ex)
            {
                information = ex.Message;
                messageBoxImage = MessageBoxImage.Error;

                return default(List<ActivityResult>);
            }
        }

        /// <summary>
        /// Supprimer un résultat
        /// </summary>
        /// <param name="activityResult">Le résultat qui doit être supprimé</param>
        public void DeleteResult(ActivityResult activityResult)
        {
            if (!IsConnected())
                return;

            try
            {
                string requete = "DELETE FROM Resultat WHERE IDResultat = @IDResultat";

                SqlCommand command = sqlConnection.CreateCommand();
                command.Parameters.AddWithValue("@IDResultat", activityResult.IDResult);
                command.CommandText = requete;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                information = e.Message;
                messageBoxImage = MessageBoxImage.Error;
            }
        }

        /// <summary>
        /// Supprimer une liste de résultats
        /// </summary>
        /// <param name="activityResult">Les résultat qui doivent être supprimés</param>
        public void DeleteResults(List<ActivityResult> activityResults)
        {
            if (!IsConnected())
                return;

            try
            {
                foreach (ActivityResult activityResult in activityResults)
                {
                    string requete = "DELETE FROM Resultat WHERE IDResultat = @IDResultat";

                    SqlCommand command = sqlConnection.CreateCommand();
                    command.Parameters.AddWithValue("@IDResultat", activityResult.IDResult);
                    command.CommandText = requete;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                information = e.Message;
                messageBoxImage = MessageBoxImage.Error;
            }
        }
    }
}

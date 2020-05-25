using Dev2Be.ClassExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SDLL
{
    [Serializable]
    public class Activity : INotifyPropertyChanged
    {
        private static readonly char[] specialCharacters = new char[] { ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', '0', '1', '2', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~', '\r', '\n', '«', '»', '’' };

        private bool activityVisibility;

        private int numberOfWordFound;
        private int numberOfWordsFound;
        private int numberofWordsTotal;

        private string activityName;
        private string lastWordGrabbed;
        private string textToDisplay;
        private string text;
        private string wordStatus;

        [field:NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ActivityVisibility
        {
            get { return activityVisibility; }
            set
            {
                activityVisibility = value;
                NotifyPropertyChanged("ActivityVisibility");
            }
        }
        public bool DisplayProvidedWords { get; set; }

        public static char[] SpecialCharacters { get => specialCharacters; }

        public int IDActivity { get; set; }
        /// <summary>
        /// Obtenir le nombre de mots trouvé au total.
        /// </summary>
        public int NumberOfWordsFound { get; set; }

        public int NumberOfWordsTotal { get; set; }

        public List<Class> Classes { get; set; }

        public List<string> FoundWords { get; set; }
        public List<string> ProvidedWords { get; set; }
        public List<string> RefusedWords { get; set; }

        public string ActivityName {
            get { return activityName; }
            set
            {
                activityName = value;
                NotifyPropertyChanged("ActivtyName");
            }
        }
        public string LastWordGrabbed { get => lastWordGrabbed; }
        public string TeacherName { get; set; }
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                NotifyPropertyChanged("Text");
            }
        }
        public string TextToDisplay { get => textToDisplay; }
        public string WordStatus { get => wordStatus; }

        // public Student Student { get; set; }

        public Activity()
        {
            Classes = new List<Class>();

            FoundWords = new List<string>();
            ProvidedWords = new List<string>();
            RefusedWords = new List<string>();

            IDActivity = -2;
        }

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            if (name == "ActivityVisibility")
            {
                BDDAccess bddAccess = new BDDAccess();

                if (bddAccess.Connect())
                    bddAccess.UpdateVisibility(this);
            }
        }

        public void Save(string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream flux = null;

            try
            {
                flux = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(flux, this);

                flux.Flush();
            }
            finally
            {
                if (flux != null)
                    flux.Close();
            }
        }

        public static Activity Open(string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream flux = null;

            try
            {
                flux = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                return (Activity)formatter.Deserialize(flux);
            }
            catch
            {
                return default(Activity);
            }
            finally
            {
                if (flux != null)
                    flux.Close();
            }
        }

        public string CheckWordPresence(string word)
        {
            lastWordGrabbed = word;

            List<string> var = new List<string>(Text.Split(specialCharacters));

            if (var.Contains(word))
            {
                if (FoundWords.FindIndex(x => x.Equals(word, StringComparison.OrdinalIgnoreCase)) != -1)
                    wordStatus = "Déjà trouvé";
                else
                {
                    GetTextToDisplay(word);

                    wordStatus = "Trouvé " + numberOfWordFound + " fois";
                }
            }
            else
            {
                if (FoundWords.FindIndex(x => x.Equals(word, StringComparison.OrdinalIgnoreCase)) == -1)
                    RefusedWords.Add(word);

                wordStatus = "Introuvable";
            }

            return textToDisplay;
        }

        public string GetTextToDisplay() => GetTextToDisplay("");

        public string GetTextToDisplay(string word)
        {
            textToDisplay = "";
            string actualWord = String.Empty;
            numberOfWordsFound = 0;
            numberOfWordFound = 0;

            foreach (char c in Text)
            {
                if (specialCharacters.Contains(c))
                {
                    if (!string.IsNullOrEmpty(actualWord))
                    {
                        bool alreadyFound = false;

                        foreach (var providedWord in ProvidedWords)
                        {
                            if (actualWord == providedWord && DisplayProvidedWords)
                            {
                                alreadyFound = true;

                                break;
                            }
                        }

                        foreach (string foundWord in FoundWords)
                        {
                            if (actualWord == foundWord)
                            {
                                alreadyFound = true;

                                if (actualWord == word)
                                    numberOfWordFound++;

                                break;
                            }
                        }

                        if (alreadyFound)
                        {
                            textToDisplay += actualWord;
                            numberOfWordsFound++;
                        }
                        else
                        {
                            if (word == actualWord)
                            {
                                textToDisplay += actualWord;
                                FoundWords.Add(word);
                                numberOfWordsFound++;
                                numberOfWordFound++;
                            }
                            else
                            {
                                foreach (char c2 in actualWord)
                                    textToDisplay += "■";
                            }
                        }

                        actualWord = "";
                    }

                    textToDisplay += c;
                }
                else
                    actualWord += c;
            }

            return textToDisplay;
        }

        public int CountWords() => numberofWordsTotal = Text.Count(specialCharacters);
    }
}

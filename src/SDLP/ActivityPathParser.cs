using SDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLP
{
    public class ActivityPathParser
    {
        public static Activity Parser(string cheminActivite)
        {
            if (string.IsNullOrEmpty(cheminActivite))
                throw new ArgumentNullException();

            if (cheminActivite.Contains("type:bdd"))
            {
                int id = int.Parse(cheminActivite.Substring(cheminActivite.IndexOf("informations:")).Substring(13, cheminActivite.IndexOf(',') - (cheminActivite.LastIndexOf(':') + 1)));
                string nom = cheminActivite.Substring(cheminActivite.IndexOf(',') + 1);

                return new Activity() { IDActivity = id, ActivityName = nom };
            }
            else if (cheminActivite.Contains("type:local"))
            {
                string filePath = cheminActivite.Substring(cheminActivite.IndexOf("informations:")).Substring(13);

                return Activity.Open(filePath);
            }

            return default(Activity);
        }

        public static string ParserInvert(Activity activite) => "type:bdd;informations:" + activite.IDActivity + "," + activite.ActivityName;

        public static string ParserInvert(string path) => "type:local;informations:" + path;
    }
}

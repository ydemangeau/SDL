using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SDLL
{
    [Serializable]
    public class ActivityResult : Activity
    {
        public int IDResult { get; set; }
        public Student Student { get; set; }

        public ActivityResult()
        {
            Student = new Student();
        }

        public new void Save(string filePath)
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

        public new static ActivityResult Open(string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream flux = null;

            try
            {
                flux = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                return (ActivityResult)formatter.Deserialize(flux);
            }
            catch
            {
                return default(ActivityResult);
            }
            finally
            {
                if (flux != null)
                    flux.Close();
            }
        }
    }
}

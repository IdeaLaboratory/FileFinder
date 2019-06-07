using HashMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace FileFinder.ViewModel
{
    public class BinarySerializer
    {
        public static void Serialize(HashMap<string, List<string>> emps, String filename)
        {
            if (emps == null || emps.Count <1)
            {
                return;
            }
            if (!Directory.Exists(filename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            //Create the stream to add object into it.
            System.IO.Stream ms = File.OpenWrite(filename);
            //Format the object as Binary

            XmlSerializer formatter = new XmlSerializer(emps.GetType());
            //It serialize the employee object
            formatter.Serialize(ms, emps);
            ms.Flush();
            ms.Close();
            ms.Dispose();
        }

        public static T Deserialize<T>(String filename)
        {
            FileStream fs = null;
            T emps;
            try
            {
                //Format the object as Binary
                XmlSerializer formatter = new XmlSerializer(typeof(HashMap<string, List<string>>));

                //Reading the file from the server
                fs = File.Open(filename, FileMode.Open);
                object obj = formatter.Deserialize(fs);
                emps = (T)obj;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }

            return emps;
        }

        #region OtherMethods
        public static void DeserializeXML<T>(String filename)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(filename);
            XmlNodeReader xNodeReader = new XmlNodeReader(xDoc.DocumentElement);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            var employeeData = xmlSerializer.Deserialize(xNodeReader);
            T deserializedEmployee = (T)employeeData;
        }

        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static byte[] Serialize(object toSerialize)
        {
            using (var stream = new MemoryStream())
            {
                Formatter.Serialize(stream, toSerialize);
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] serialized)
        {
            using (var stream = new MemoryStream(serialized))
            {
                var result = (T)Formatter.Deserialize(stream);
                return result;
            }
        }
        #endregion
    }
}

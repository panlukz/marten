using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Sodev.Marten.Persitence.Helpers
{
    public static class XmlSerializeHelper
    {
        /// <summary>
        /// Serializes an object to xml string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        public static string SerializeObject<T>(T serializableObject)
        {
            var xmlDocument = new XmlDocument();

            try
            {
                var serializer = new XmlSerializer(serializableObject.GetType());

                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw;
            }

            return xmlDocument.InnerXml;
        }


        /// <summary>
        /// Deserializes an xml string into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeSerializeObject<T>(string xmlString)
        {
            T objectOut = default(T);
            try
            {
                string attributeXml = string.Empty;
                using (StringReader read = new StringReader(xmlString))
                {
                    var outType = typeof(T);

                    var serializer = new XmlSerializer(outType);
                    using (var reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw;
            }

            return objectOut;
        }
    }
}

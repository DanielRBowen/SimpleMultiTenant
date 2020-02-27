using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleMultiTenant.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// From: https://stackoverflow.com/questions/33022660/how-to-convert-byte-array-to-any-type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToByteArray<T>(this T obj)
        {
            if (obj == null)
                return null;
            var binaryFormatter = new BinaryFormatter();
            using var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, obj);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// From https://stackoverflow.com/questions/33022660/how-to-convert-byte-array-to-any-type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FromByteArray<T>(this byte[] data)
        {
            if (data == null)
                return default;
            var binaryFormater = new BinaryFormatter();
            using var memoryStream = new MemoryStream(data);
            object obj = binaryFormater.Deserialize(memoryStream);
            return (T)obj;
        }
    }
}

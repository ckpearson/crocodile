using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace xInvoke
{
    public static class Extensions
    {
        /// <summary>
        /// Flattens a collection of strings without adding a new line
        /// </summary>
        /// <param name="Collection">The IEnumerable(String) to flatten</param>
        /// <returns>The flattened string</returns>
        public static string Flatten(this IEnumerable<string> Collection)
        {
            return Flatten(Collection, false);
        }

        /// <summary>
        /// Flattens a collection of strings with the option of adding a new line after each
        /// </summary>
        /// <param name="Collection">The IEnumerable(String) to flatten</param>
        /// <param name="newLine">Whether to append a new line after each string</param>
        /// <returns>The flattened string</returns>
        public static string Flatten(this IEnumerable<string> Collection, bool newLine)
        {
            string r = string.Empty;
            foreach (string s in Collection)
            {
                r += s + (newLine ? "\n" : string.Empty);
            }
            return r;
        }

        /// <summary>
        /// Deserialises a BASE-64 encoded string
        /// </summary>
        /// <param name="str">Base-64 encoded string</param>
        /// <returns>Deserialised object (null if string = NULL)</returns>
        public static object Deserialise(this string str)
        {
            object r = null;

            if (str != "NULL")
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(str));
                ms.Position = 0;
                r = bf.Deserialize(ms);
            }
            return r;
        }

        /// <summary>
        /// Serilalises and object
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>The base-64 encoded string (NULL if obj is null)</returns>
        public static string Serialise(this object obj)
        {
            string r = string.Empty;

            if (obj != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                ms.Position = 0;
                bf.Serialize(ms, obj);
                ms.Position = 0;
                byte[] b = new byte[ms.Length];
                ms.Read(b, 0, b.Length);
                ms.Dispose();

                r = Convert.ToBase64String(b);
            }
            else { r = "NULL"; }
            return r;
        }
    }
}

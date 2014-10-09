using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Hl7.Fhir.Publication
{
    public delegate bool FileMatch(string name);

    public static class Disk
    {

        public static IEnumerable<string> FilterFiles(string directory, bool recursive, FileMatch isMatch)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (string filename in Directory.EnumerateFiles(directory, "*", option))
            {
                if (isMatch(filename)) yield return filename;
            }
        }

        public static IEnumerable<string> FilterFiles(string directory, string mask, bool recursive)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directory, mask, option);
        }

        public static string FileMaskToRegExPattern(string mask)
        {
            string pattern = mask
                .ToLower()
                .Replace("\\", "\\\\")
                .Replace(".", "\\.")
                .Replace("*", ".*")
                .Replace("?", ".?");

            return pattern;
        }

        public static string ParseMask(string name, string mask)
        {
            string[] parts = name.Split(new char[] { '-', '.' });
            string result = mask;
            for (int i = 0; i <= parts.Count()-1; i++)
            {
                string alias = string.Format("${0}", i+1);
                result = result.Replace(alias, parts[i]);
            }
            return result;
        }

        public static void EnsurePath(string path)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static String RelativePath(string basepath, string path)
        {
            
            if (path.StartsWith(basepath))
            {
                string s = path.Remove(0, basepath.Length).TrimStart('\\');
                return s;
            }
            else
            {
                return path;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Hash generator namespace
/// </summary>
namespace HashGenerator
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        private static List<string> GetFiles(IEnumerable<string> paths)
        {
            List<string> ret = new List<string>();
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    ret.AddRange(GetFiles(Directory.GetDirectories(path)));
                    ret.AddRange(GetFiles(Directory.GetFiles(path)));
                }
                else if (File.Exists(path))
                    ret.Add(path);
            }
            return ret;
        }

        /// <summary>
        /// Entry method
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            List<string> paths = new List<string>();
            bool delete_files = false;
            bool verbose = false;
            foreach (string arg in args)
            {
                if ((arg.Trim() == "-D") || (arg.Trim() == "--delete"))
                    delete_files = true;
                else if ((arg.Trim() == "-V") || (arg.Trim() == "--verbose"))
                    verbose = true;
                else
                    paths.Add(arg);
            }
            if (verbose && delete_files)
                Console.WriteLine("Deleting files after hashing");
            foreach (string path in GetFiles(args))
            {
                try
                {
                    Dictionary<string, string> results = new Dictionary<string, string>();
                    if (verbose)
                        Console.WriteLine("Generating hash for " + path);
                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        using (MD5 hash = MD5.Create())
                        {
                            results.Add("MD5", ComputeHash(stream, hash));
                        }
                        using (RIPEMD160 hash = RIPEMD160.Create())
                        {
                            results.Add("RIPEMD160", ComputeHash(stream, hash));
                        }
                        using (SHA1 hash = SHA1.Create())
                        {
                            results.Add("SHA1", ComputeHash(stream, hash));
                        }
                        using (SHA256 hash = SHA256.Create())
                        {
                            results.Add("SHA256", ComputeHash(stream, hash));
                        }
                        using (SHA384 hash = SHA384.Create())
                        {
                            results.Add("SHA384", ComputeHash(stream, hash));
                        }
                        using (SHA512 hash = SHA512.Create())
                        {
                            results.Add("SHA512", ComputeHash(stream, hash));
                        }
                    }
                    string output_path = path + ".hashes.txt";
                    if (File.Exists(output_path))
                        File.Delete(output_path);
                    using (StreamWriter writer = new StreamWriter(output_path))
                    {
                        foreach (KeyValuePair<string, string> kv in results)
                            writer.WriteLine(kv.Key + ":\t" + ((kv.Key.Length < 8) ? "\t" : "") + kv.Value);
                    }
                    if (delete_files)
                    {
                        if (verbose)
                            Console.WriteLine("Deleting file " + path);
                        File.Delete(path);
                    }
                    results.Clear();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Compute hash
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="hashAlgorithm">Hash algorithm</param>
        /// <returns>Base64 of hash</returns>
        private static string ComputeHash(Stream stream, HashAlgorithm hashAlgorithm)
        {
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            return Convert.ToBase64String(hashAlgorithm.ComputeHash(stream));
        }
    }
}

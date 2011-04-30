using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;

namespace fss_client
{
    class Sha1
    {

        // unique algorithm sha1 digest for fss
        public static string sha1_file_via_fname_fss(string rootpath, string fullname)
        {
            SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
            byte[] digest;
            string digest_str0, digest_str1;

            // if fullname is a dir, still calculate sha1 digest
            if (Directory.Exists(fullname))
            {
                digest = provider.ComputeHash(
                    System.Text.Encoding.Default.GetBytes(string.Empty));
            }
            else
            {

                FileStream fs = new FileStream(fullname, FileMode.Open, FileAccess.Read, FileShare.None);
                digest = provider.ComputeHash(fs);
                fs.Close();
            }

            digest_str0 = BitConverter.ToString(digest).Replace("-", "");

            string relaname = fullname.Substring(rootpath.Length);

            string[] words = relaname.Split('\\');


            foreach (string s in words)
            {
                if (s == "")
                    continue;
                digest = provider.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(s));

                digest_str1 = BitConverter.ToString(digest).Replace("-", "");

                digest = provider.ComputeHash(
                    System.Text.Encoding.Default.GetBytes(digest_str0 + digest_str1));

                digest_str0 = BitConverter.ToString(digest).Replace("-", "");


            }

            //return BitConverter.ToString(digest);
            return digest_str0;

        }

        // calculate normal sha1 digest
        public static string sha1_file_via_fname(string fullname)
        {
            SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
            byte[] digest;

            FileStream fs = new FileStream(fullname, FileMode.Open, FileAccess.Read);
            digest = provider.ComputeHash(fs);
            fs.Close();

            return BitConverter.ToString(digest).Replace("-", "");

        }




        public static void compute_hash(string fullname, string rootpath, ref string sha1_digest, ref string hash_digest)
        {
            SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
            byte[] digest;
            int flag = 0;
            string content_digest, path_digest;

            if (Directory.Exists(fullname))
            {
                flag = 1;
                digest = provider.ComputeHash(System.Text.Encoding.Default.GetBytes(string.Empty));
                content_digest = BitConverter.ToString(digest).Replace("-", "");

            }
            else
            {
                FileStream fs = new FileStream(fullname, FileMode.Open, FileAccess.Read, FileShare.None);
                digest = provider.ComputeHash(fs);
                fs.Close();
                content_digest = BitConverter.ToString(digest).Replace("-", "");

            }
            if (sha1_digest != null)
                sha1_digest = content_digest;

            if (hash_digest == null)
                return;


            string relaname = fullname.Substring(rootpath.Length);
            string[] words = relaname.Split('\\');


            foreach (string s in words)
            {
                if (s == "")
                    continue;
                digest = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));

                path_digest = BitConverter.ToString(digest).Replace("-", "");

                digest = provider.ComputeHash(
                    System.Text.Encoding.Default.GetBytes(content_digest + path_digest));

                content_digest = BitConverter.ToString(digest).Replace("-", "");
            }

            //return BitConverter.ToString(digest);
            hash_digest = content_digest;


        }


    }
}

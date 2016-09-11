using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace File_Manager_System
{
    class Crypt_Visitor : C_Visitor
    {
        byte[] my_key = new byte[24];
        byte[] my_vector = new byte[8];

        public Crypt_Visitor(string key, string vector)
        {
            try
            {
                string[] splited = key.Split();
                for (int i = 0; i < splited.Length; i++)
                {
                    my_key[i] = byte.Parse(splited[i]);
                }
                splited = vector.Split();
                for (int i = 0; i < splited.Length; i++)
                {
                    my_vector[i] = byte.Parse(splited[i]);
                }
            }
            catch (Exception)
            {
            }
        }

        public void Visit(My_Entry E)
        {
            if (E is My_File)
            {
                Encrypt(E as My_File);
            }
        }

        public bool IsDone
        {
            get { return false; }
        }

        public void Encrypt(My_File F)
        {
            try
            {
                string n_path = Path.Combine(Regex.Replace(F.FullName, Path.GetExtension(F.FullName), @"_crypted_" + Path.GetExtension(F.FullName)));

                My_File n_F = new My_File(n_path);
                string text = F.ReadAllText();

                byte[] encrypted;

                using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
                {
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = tdsAlg.CreateEncryptor(my_key, my_vector);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {

                                //Write all data to the stream.
                                swEncrypt.Write(text);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                    n_F.WritellBytes(encrypted);
                }
            }

            catch (Exception) { }
        }
    }
}

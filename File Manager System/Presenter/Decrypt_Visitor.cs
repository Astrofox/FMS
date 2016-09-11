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
    class Decrypt_Visitor:C_Visitor
    {
        byte[] my_key = new byte[24];
        byte[] my_vector = new byte[8];

        public Decrypt_Visitor(string key, string vector)
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
                Decrypt(E as My_File);
            }
        }

        public bool IsDone
        {
            get { return false; }
        }

        public void Decrypt(My_File F)
        {
            byte[] text = F.ReadAllBytes();

            string n_path = F.FullName;
            n_path = Regex.Replace(n_path, @"_crypted_", "");
            My_File n_F = new My_File(n_path);
            
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = "";

            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = tdsAlg.CreateDecryptor(my_key, my_vector);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(text))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
                My_File Used_file = new My_File();
                Used_file.WriteAllText(n_path,plaintext);
            }
        }

    }
}

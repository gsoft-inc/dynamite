using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class to encrypt and decrypt data using share secret
    /// </summary>
    public class CryptoHelper
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes("o6123642kbM7c5");

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <returns>The Encrypted version of the string</returns>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException("plainText");
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            string result = null;                       // Encrypted string to return
            RijndaelManaged aesAlgorithm = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlgorithm = new RijndaelManaged();
                aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                // Create the streams used for encryption.
                using (MemoryStream memoryStreamEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    memoryStreamEncrypt.Write(BitConverter.GetBytes(aesAlgorithm.IV.Length), 0, sizeof(int));
                    memoryStreamEncrypt.Write(aesAlgorithm.IV, 0, aesAlgorithm.IV.Length);
                    using (CryptoStream cryptoStreamEncrypt = new CryptoStream(memoryStreamEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriterEncrypt = new StreamWriter(cryptoStreamEncrypt))
                        {
                            // Write all data to the stream.
                            streamWriterEncrypt.Write(plainText);
                        }
                    }

                    result = Convert.ToBase64String(memoryStreamEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlgorithm != null)
                {
                    aesAlgorithm.Clear();
                }
            }

            // Return the encrypted bytes from the memory stream.
            return result;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <returns>The Decrypted version of the cipher</returns>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException("cipherText");
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlgorythm = null;

            // Declare the string used to hold
            // the decrypted text.
            string result = null;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream memoryStreamDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlgorythm = new RijndaelManaged();
                    aesAlgorythm.Key = key.GetBytes(aesAlgorythm.KeySize / 8);

                    // Get the initialization vector from the encrypted stream
                    aesAlgorythm.IV = ReadByteArray(memoryStreamDecrypt);

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlgorythm.CreateDecryptor(aesAlgorythm.Key, aesAlgorythm.IV);
                    using (CryptoStream cryptoStreamDecrypt = new CryptoStream(memoryStreamDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReaderDecrypt = new StreamReader(cryptoStreamDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            result = streamReaderDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlgorythm != null)
                {
                    aesAlgorythm.Clear();
                }
            }

            return result;
        }

        private static byte[] ReadByteArray(Stream stream)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}

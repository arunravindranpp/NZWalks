using ConflictAutomation.Services;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
#pragma warning disable CA1416

namespace ConflictAutomation.Utilities
{
    public class Cryptography
    {
        private static readonly string _encryptionKey;
        static Cryptography()
        {
            _encryptionKey = GetEncryptionKey();
        }
        /// <summary>
        /// Gets the Encryption Key value for the Encryption procedure.
        /// </summary>
        private static string GetEncryptionKey()
        {
            const string registryPath = "SOFTWARE\\Ernst & Young\\PACE-RegKey";
            const string registryValueName = "RegKeyValue";
            const string defaultKeyValue = "PACEEncryptionKey09282016";

            string eKey="";
            try
            {
                // Opening the registry key
                using (RegistryKey baseRegistryKey = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    // If the RegistrySubKey doesn't exist -> create it and set the default value
                    if (baseRegistryKey == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(registryPath))
                        {
                            newKey?.SetValue(registryValueName, defaultKeyValue);
                            eKey = defaultKeyValue;
                        }
                    }
                    else
                    {
                        // If the RegistryKey exists, get its value
                        eKey = baseRegistryKey.GetValue(registryValueName, defaultKeyValue) as string;
                        if (eKey == null)
                        {
                            baseRegistryKey.SetValue(registryValueName, defaultKeyValue);
                            eKey = defaultKeyValue;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LoggerInfo.LogException(ex);
            }
            return eKey;
        }
        /// <summary>
        /// Method to decrypt a particular string 
        /// </summary>
        /// <param name="stringToDecrypt"></param>
        /// <returns></returns>
        public static string Decrypt3DES(string stringToDecrypt)
        {
            byte[] key;
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
            byte[] inputByteArray = new byte[stringToDecrypt.Length];
            try
            {
                key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 24));
                TripleDES des = TripleDES.Create();
                inputByteArray = Convert.FromBase64String(stringToDecrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = Encoding.UTF8;
                des.Clear();
                return encoding.GetString(ms.ToArray());
            }
            catch (System.Exception ex)
            { return (ex.ToString()); }
        }

        /// <summary>
        /// Method to encrypt a particular string with the current 'Encryption Key'.
        /// </summary>
        /// <param name="stringToEncrypt">String to encrypt</param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt3DES(string stringToEncrypt)
        {
            byte[] key;
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
            byte[] inputByteArray;

            try
            {
                key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 24));
                TripleDES des = TripleDES.Create();
                inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                des.Clear();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (System.Exception ex)
            { return (ex.ToString()); }
        }
    }
}
#pragma warning restore CA1416
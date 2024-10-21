using System.Text;
using System.Security.Cryptography;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;

#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1825 // Avoid zero-length array allocations
#pragma warning disable CA1830 // Prefer strongly-typed Append and Insert method overloads on StringBuilder
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable SYSLIB0021 // Type or member is obsolete
#pragma warning disable SYSLIB0023 // Type or member is obsolete

namespace GEARAPI.Application.Helpers;

public class Cryptography
{

    private static string _encryptionKey;

    static Cryptography()
    {
        _encryptionKey = GetEncryptionKey();
    }

    public static void SetEncryptionKey(string encryptionKey)
    {
        _encryptionKey = encryptionKey;

    }

    public string DecryptString(string a_s)
    {
        string sRet = a_s;
        if (a_s.ToLower().IndexOf(";pwd=") == -1)
            sRet = Decrypt(a_s);

        return sRet;
    }

    /// <summary>
    /// Gets the Encryption Key value for the Encryption procedure.
    /// </summary>

    private static string GetEncryptionKey()
    {
        const string registryPath = "SOFTWARE\\Ernst & Young\\GEAR-RegKey";
        const string registryValueName = "RegKeyValue";
        const string defaultKeyValue = "GEAREncryptionKey12112019$";

        string eKey = "";
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
        catch (Exception ex)
        {
        }
        return eKey;
    }

    /// <summary>
    /// Method to Decrypt a particular string with the current Encryption Key.
    /// </summary>
    /// <param name="stringToDecrypt">String to decrypt (must have been encrypted with the current Encryption Key only).</param>
    /// <returns>Decrypted string</returns>
    public static string Decrypt(string stringToDecrypt)
    {
        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray = new byte[stringToDecrypt.Length];
        try
        {
            key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(stringToDecrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            Encoding encoding = Encoding.UTF8;
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
    public static string Encrypt(string stringToEncrypt)
    {
        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray;

        try
        {
            key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (System.Exception ex)
        { return (ex.ToString()); }
    }

    /// <summary>
    /// Method to generate a cryptographically strong random number.
    /// </summary>
    /// <returns>a random number</returns>
    public static string GenerateRandomNumber()
    {
        StringBuilder builder = new StringBuilder();
        byte[] random = new Byte[10];

        //RNGCryptoServiceProvider is an implementation of a random number 
        //generator.
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                              // The array is now filled with cryptographically strong random bytes.
        rng.GetBytes(random);
        foreach (byte b in random)
            builder.Append(b.ToString());

        return builder.ToString();
    }

    public static string Decrypt3DES(string stringToDecrypt)
    {
        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray = new byte[stringToDecrypt.Length];
        try
        {
            key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 24));
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
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
        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray;

        try
        {
            key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 24));
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
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


    public static string GetUnencryptedConnectionString(string connectionString)
    {
        if (connectionString.Contains("Initial Catalog", StringComparison.OrdinalIgnoreCase) 
            || connectionString.Contains("database", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }
        else
            return Cryptography.Decrypt3DES(connectionString);
    }
}

#pragma warning restore SYSLIB0023 // Type or member is obsolete
#pragma warning restore SYSLIB0021 // Type or member is obsolete
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0057 // Use range operator
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore CA1830 // Prefer strongly-typed Append and Insert method overloads on StringBuilder
#pragma warning restore CA1825 // Avoid zero-length array allocations
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore CA1416 // Validate platform compatibility

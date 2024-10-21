using System.Security.Cryptography;
using System.Text;

namespace GEARAPI.Application.Helpers;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable SYSLIB0021 // Type or member is obsolete
#pragma warning disable SYSLIB0022 // Type or member is obsolete

public class FileEncryption
{
    /// <summary>
    /// Encryption key.
    /// </summary>
    private static readonly byte[] Key = new byte[]
    {
     144,24,138,199,76,214,156,202,
     215,2,80,234,152,204,95,48,
     245,68,36,8,104,231,212,199
    };
    private static readonly string Vector = @"TJ$9x4(8&Aqih{1J";

    public byte[] Encrypt(byte[] attachment)
    {

        byte[] encryptedData = null;
        using (MemoryStream ms = new MemoryStream())
        {
            Rijndael alg = Rijndael.Create();
            var keyBytes = Key;
            byte[] vectorBytes = Encoding.UTF8.GetBytes(Vector);
            alg.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(keyBytes, vectorBytes), CryptoStreamMode.Write);

            cs.Write(attachment, 0, attachment.Length);

            cs.FlushFinalBlock();
            encryptedData = ms.ToArray();
            alg.Clear();
        }
        return encryptedData;
    }

    public string EncryptFileName(string input)
    {
        byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = Key;
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public void WriteToFile(byte[] data, string encryptedFileNamePath)
    {
        File.Delete(encryptedFileNamePath);
        System.IO.FileStream oFileStream = null;
        oFileStream = new System.IO.FileStream(encryptedFileNamePath, System.IO.FileMode.Create);
        oFileStream.Write(data, 0, data.Length);
        oFileStream.Close();
    }
    public string DecryptFileName(string withoutExtension)
    {
        byte[] inputArray = Convert.FromBase64String(withoutExtension);
        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = Key;
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        //Convert.ToBase64String(resultArray);
        return UTF8Encoding.UTF8.GetString(resultArray);
    }

    public byte[] Decrypt(string filePath, string docID, string sourceSystem = "")
    {
        FileStream fileStream = null;
        byte[] decryptedData = null;
        using (MemoryStream ms = new MemoryStream())
        {
            byte[] attachment = null;
            using (fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                //Tejal : EADR migrated docs will be in EADR directory and they do not need decryption. If this document gets modify by admins then new uploaded docs get 
                //        treated as GEAR document, no doubt we will not change SourceSystem = 'GEAR' but attachment/filepath will not contain \\EADR\\ directory and download will work.
                if (sourceSystem == "EADR" && filePath.Contains("\\EADR\\"))
                //if(sourceSystem == "EADR" && filePath.Contains("\\GEAR_US_ATT_UAT\\"))
                {
                    byte[] EADRbytes = System.IO.File.ReadAllBytes(filePath);
                    fileStream.Read(EADRbytes, 0, System.Convert.ToInt32(fileStream.Length));
                    fileStream.Close();

                    decryptedData = EADRbytes;
                }
                else
                {
                    int length = Convert.ToInt32(fileStream.Length);
                    attachment = new byte[length];

                    fileStream.Read(attachment, 0, length);
                    fileStream.Close();
                    string filename = Path.GetFileNameWithoutExtension(filePath);

                    Rijndael alg = Rijndael.Create();
                    var keyBytes = Key;
                    byte[] vectorBytes = Encoding.UTF8.GetBytes(Vector);
                    alg.Mode = CipherMode.CBC;

                    CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(keyBytes, vectorBytes), CryptoStreamMode.Write);

                    cs.Write(attachment, 0, attachment.Length);

                    cs.FlushFinalBlock();
                    decryptedData = ms.ToArray();
                    attachment = null;
                    alg.Clear();
                }
            }
        }
        return decryptedData;
    }

    //public string DecFile(string file)
    //{
    //    string decryptedData = null;
    //    using (MemoryStream ms = new MemoryStream())
    //    {
    //        byte[] attachment = null;
    //        Rijndael alg = Rijndael.Create();
    //        var keyBytes = Key;
    //        byte[] vectorBytes = Encoding.UTF8.GetBytes(Vector);
    //        alg.Mode = CipherMode.CBC;

    //        CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(keyBytes, vectorBytes), CryptoStreamMode.Write);

    //        cs.Write(attachment, 0, attachment.Length);
    //        decryptedData = System.Text.Encoding.UTF8.GetString(ms.ToArray()); ;
    //    }
    //    return decryptedData;
    //}
}

#pragma warning restore SYSLIB0022 // Type or member is obsolete
#pragma warning restore SYSLIB0021 // Type or member is obsolete
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CA1822 // Mark members as static

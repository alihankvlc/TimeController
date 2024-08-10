using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public interface ISaveable
{
}

public class SaveManager
{
    private readonly byte[] key = Encoding.UTF8.GetBytes("1234567890123456");
    private readonly byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

    public void Save<T>(T data) where T : ISaveable
    {
        string json = JsonUtility.ToJson(data);
        string encryptedJson = Encrypt(json);
        string filePath = GetFilePath<T>();

        File.WriteAllText(filePath, encryptedJson);
    }

    public T Load<T>() where T : class, ISaveable
    {
        string filePath = GetFilePath<T>();

        if (File.Exists(filePath))
        {
            string encryptedJson = File.ReadAllText(filePath);
            string json = Decrypt(encryptedJson);
            return JsonUtility.FromJson<T>(json);
        }

        return null;
    }

    public bool ContainsKey<T>() where T : ISaveable
    {
        string filePath = GetFilePath<T>();
        return File.Exists(filePath);
    }

    private string GetFilePath<T>() where T : ISaveable
    {
        string fileName = GenerateFileName<T>();
        return Path.Combine(Application.persistentDataPath, $"{fileName}.json");
    }

    private string GenerateFileName<T>() where T : ISaveable
    {
        string typeName = typeof(T).Name;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(typeName));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    private string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
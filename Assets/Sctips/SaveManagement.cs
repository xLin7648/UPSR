using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web; // 需要添加 System.Web 引用
using UnityEngine;
using System.Xml;

public class SaveManagement : MonoBehaviour
{
    private static readonly string encryptKey = "PGRS";
    public TextAsset encryptedXml; // 包含加密数据的 XML 文件

    private void Start()
    {
        var decryptedData = ParseAndDecryptXml(encryptedXml.text);

        decryptedData.Save("C:/a.xml");
        ;
    }

    // 解析 XML 并解密所有字符串
    public static XmlDocument ParseAndDecryptXml(string xmlContent)
    {
        // 加载 XML 文档
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        // 创建新的 XML 文档用于存储解密后的内容
        XmlDocument decryptedDoc = new XmlDocument();
        XmlElement root = decryptedDoc.CreateElement("map");
        decryptedDoc.AppendChild(root);

        // 获取所有 <string> 节点
        XmlNodeList stringNodes = xmlDoc.SelectNodes("//string");

        foreach (XmlNode node in stringNodes)
        {
            // 获取键和值
            string key = node.Attributes?["name"]?.Value;
            string encryptedValue = node.InnerText;

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(encryptedValue))
            {
                try
                {
                    // URL 解码
                    string urlDecodedKey = HttpUtility.UrlDecode(key);
                    string urlDecodedValue = HttpUtility.UrlDecode(encryptedValue);

                    // 解密值
                    string decryptedKey = Decrypt(urlDecodedKey);
                    string decryptedValue = Decrypt(urlDecodedValue);

                    // 创建新节点
                    XmlElement newElement = decryptedDoc.CreateElement("string");
                    newElement.SetAttribute("name", decryptedKey);
                    newElement.InnerText = decryptedValue;

                    // 添加到新文档
                    root.AppendChild(newElement);
                }
                catch
                {
                    try
                    {
                        // 解密值
                        string decryptedKey = Decrypt(key);
                        string decryptedValue = Decrypt(encryptedValue);

                        // 创建新节点
                        XmlElement newElement = decryptedDoc.CreateElement("string");
                        newElement.SetAttribute("name", decryptedKey);
                        newElement.InnerText = decryptedValue;

                        // 添加到新文档
                        root.AppendChild(newElement);
                    }
                    catch
                    {
                        // 创建新节点
                        XmlElement newElement = decryptedDoc.CreateElement("string");
                        newElement.SetAttribute("name", key);
                        newElement.InnerText = encryptedValue;

                        // 添加到新文档
                        root.AppendChild(newElement);
                    }
                }
                
            }
        }

        return decryptedDoc;
    }

    // 解密方法
    public static string Decrypt(string encryptedString)
    {
        // 1. 获取 Unicode 编码
        Encoding encoding = Encoding.Unicode;

        // 2. 获取加密密钥的字节数组
        byte[] keyBytes = encoding.GetBytes(encryptKey);

        // 3. 将 Base64 字符串转换为字节数组
        byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

        // 4. 创建 DES 加密服务提供者
        using DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();

        // 5. 创建内存流
        using MemoryStream memoryStream = new MemoryStream();

        // 6. 创建解密器
        // 注意：使用前8字节作为密钥（DES 需要8字节密钥）
        byte[] actualKey = new byte[8];
        Array.Copy(keyBytes, actualKey, Math.Min(keyBytes.Length, 8));

        ICryptoTransform decryptor = desProvider.CreateDecryptor(actualKey, actualKey);

        // 7. 创建加密流
        using CryptoStream cryptoStream = new CryptoStream(
            memoryStream,
            decryptor,
            CryptoStreamMode.Write
        );

        // 8. 写入加密数据并刷新
        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
        cryptoStream.FlushFinalBlock();

        // 9. 获取解密后的字节数组
        byte[] decryptedBytes = memoryStream.ToArray();

        // 10. 将字节数组转换为字符串
        return encoding.GetString(decryptedBytes);
    }
}

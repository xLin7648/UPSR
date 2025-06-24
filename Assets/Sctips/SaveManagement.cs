using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web; // ��Ҫ��� System.Web ����
using UnityEngine;
using System.Xml;

public class SaveManagement : MonoBehaviour
{
    private static readonly string encryptKey = "PGRS";
    public TextAsset encryptedXml; // �����������ݵ� XML �ļ�

    private void Start()
    {
        var decryptedData = ParseAndDecryptXml(encryptedXml.text);

        decryptedData.Save("C:/a.xml");
        ;
    }

    // ���� XML �����������ַ���
    public static XmlDocument ParseAndDecryptXml(string xmlContent)
    {
        // ���� XML �ĵ�
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        // �����µ� XML �ĵ����ڴ洢���ܺ������
        XmlDocument decryptedDoc = new XmlDocument();
        XmlElement root = decryptedDoc.CreateElement("map");
        decryptedDoc.AppendChild(root);

        // ��ȡ���� <string> �ڵ�
        XmlNodeList stringNodes = xmlDoc.SelectNodes("//string");

        foreach (XmlNode node in stringNodes)
        {
            // ��ȡ����ֵ
            string key = node.Attributes?["name"]?.Value;
            string encryptedValue = node.InnerText;

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(encryptedValue))
            {
                try
                {
                    // URL ����
                    string urlDecodedKey = HttpUtility.UrlDecode(key);
                    string urlDecodedValue = HttpUtility.UrlDecode(encryptedValue);

                    // ����ֵ
                    string decryptedKey = Decrypt(urlDecodedKey);
                    string decryptedValue = Decrypt(urlDecodedValue);

                    // �����½ڵ�
                    XmlElement newElement = decryptedDoc.CreateElement("string");
                    newElement.SetAttribute("name", decryptedKey);
                    newElement.InnerText = decryptedValue;

                    // ��ӵ����ĵ�
                    root.AppendChild(newElement);
                }
                catch
                {
                    try
                    {
                        // ����ֵ
                        string decryptedKey = Decrypt(key);
                        string decryptedValue = Decrypt(encryptedValue);

                        // �����½ڵ�
                        XmlElement newElement = decryptedDoc.CreateElement("string");
                        newElement.SetAttribute("name", decryptedKey);
                        newElement.InnerText = decryptedValue;

                        // ��ӵ����ĵ�
                        root.AppendChild(newElement);
                    }
                    catch
                    {
                        // �����½ڵ�
                        XmlElement newElement = decryptedDoc.CreateElement("string");
                        newElement.SetAttribute("name", key);
                        newElement.InnerText = encryptedValue;

                        // ��ӵ����ĵ�
                        root.AppendChild(newElement);
                    }
                }
                
            }
        }

        return decryptedDoc;
    }

    // ���ܷ���
    public static string Decrypt(string encryptedString)
    {
        // 1. ��ȡ Unicode ����
        Encoding encoding = Encoding.Unicode;

        // 2. ��ȡ������Կ���ֽ�����
        byte[] keyBytes = encoding.GetBytes(encryptKey);

        // 3. �� Base64 �ַ���ת��Ϊ�ֽ�����
        byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

        // 4. ���� DES ���ܷ����ṩ��
        using DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();

        // 5. �����ڴ���
        using MemoryStream memoryStream = new MemoryStream();

        // 6. ����������
        // ע�⣺ʹ��ǰ8�ֽ���Ϊ��Կ��DES ��Ҫ8�ֽ���Կ��
        byte[] actualKey = new byte[8];
        Array.Copy(keyBytes, actualKey, Math.Min(keyBytes.Length, 8));

        ICryptoTransform decryptor = desProvider.CreateDecryptor(actualKey, actualKey);

        // 7. ����������
        using CryptoStream cryptoStream = new CryptoStream(
            memoryStream,
            decryptor,
            CryptoStreamMode.Write
        );

        // 8. д��������ݲ�ˢ��
        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
        cryptoStream.FlushFinalBlock();

        // 9. ��ȡ���ܺ���ֽ�����
        byte[] decryptedBytes = memoryStream.ToArray();

        // 10. ���ֽ�����ת��Ϊ�ַ���
        return encoding.GetString(decryptedBytes);
    }
}

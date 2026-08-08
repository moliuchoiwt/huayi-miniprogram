using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace YW.Common
{
    public class XmlHelper
    {

        #region [xml文件读取/写入]

        /// <summary>
        /// xml文件读取
        /// </summary>
        /// <param name="filePath">文件绝对路径</param>
        /// <returns></returns>
        public static T ReadXml<T>(string filePath)
        {
            var fs = File.ReadAllText(filePath);
            T res = GetT<T>(fs);
            return res;
        }

        /// <summary>
        /// xml文件写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static void SaveXml<T>(T xml, string filename)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(xml.GetType());
                serializer.Serialize(fs, xml);
            }
            catch (Exception ex)
            {
                LogHelper.Error("", ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

        }
        #endregion

        #region [xml序列化/反序列化]

        /// <summary>
        /// 创建xml序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXml<T>(T obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                serializer.Serialize(writer, obj);

                string xml = writer.ToString();
                writer.Close();
                writer.Dispose();
                return xml;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T GetT<T>(string xml)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader reader = new StringReader(xml);

                T res = (T)serializer.Deserialize(reader);
                reader.Close();
                reader.Dispose();
                return res;
            }
            catch (Exception ex)
            {
                LogHelper.Error("", ex);
            }
            return default(T);

        }
        #endregion


        #region [xml/json配置文件读取]
        /// <summary>
        /// xml配置文件读取
        /// </summary>
        /// <param name="configFileName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static IConfigurationRoot GetXmlConfig(
                    string configFileName = "appsettings.xml",
                    string basePath = "")
        {
            basePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : basePath;

            var builder = new ConfigurationBuilder().
               //SetBasePath(basePath).
               AddXmlFile(b =>
               {
                   b.Path = configFileName;
                   b.FileProvider = new PhysicalFileProvider(basePath);
               });
            return builder.Build();
        }
        /// <summary>
        /// json配置文件读取
        /// </summary>
        /// <param name="configFileName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static IConfigurationRoot GetJsonConfig(
                    string configFileName = "appsettings.json",
                    string basePath = "")
        {
            basePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : basePath;

            var builder = new ConfigurationBuilder().
                    SetBasePath(basePath).
                    AddJsonFile(configFileName);
            return builder.Build();
        }
        #endregion

    }
}

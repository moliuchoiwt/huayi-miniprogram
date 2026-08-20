using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YW.Common
{
    public class JsonOperateService
    {
        /// <summary>
        /// 将Json转换回列表
        /// </summary>
        public static List<T> ReadJsonFileToList<T>(string fileName)
        {
            //将Json转换回列表
            var directorypath = Directory.GetCurrentDirectory();
            string strFileName = directorypath + $"\\Config\\{fileName}";//  "\\NewsData.json";
            string jsonData = GetJsonFile(strFileName);
            //反序列化Json字符串内容为对象
            List<T> jsondata = JsonConvert.DeserializeObject<List<T>>(jsonData);
            return jsondata;

        }
        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        public static string GetJsonFile(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }
        /// <summary>
        /// 把对象写入到json文件中
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static void WriteJson<T>(List<T> jsonData,string fileName)
        {
            var directorypath = Directory.GetCurrentDirectory();
            string strFileName = directorypath + $"\\Config\\{fileName}";// "\\NewsData.json";
            string ListJson = JsonConvert.SerializeObject(jsonData);

            writeJsonFile(strFileName, ListJson);

            //将序列化的json字符串内容写入Json文件，并且保存
            void writeJsonFile(string path, string jsonConents)
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //如果json文件中有中文数据，可能会出现乱码的现象，那么需要加上如下代码
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(jsonConents);
                    }
                }
            }
        }
    }
}

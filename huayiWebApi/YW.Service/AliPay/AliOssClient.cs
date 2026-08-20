using Aliyun.OSS;
using System.IO;
using System.Text;

namespace YW.Service.AliPay
{
    public class AliOssClient
    {

        #region /初始变量
        // 填写Bucket所在地域对应的Endpoint。以华东1（杭州）为例，Endpoint填写为https://oss-cn-hangzhou.aliyuncs.com。
        private static readonly string endpoint = "https://oss-cn-shenzhen-internal.aliyuncs.com";// "https://oss-cn-guangzhou.aliyuncs.com";
        private static readonly string accessKeyId = PubConstant.Config.Ayl_accessKeyId;
        private static readonly string accessKeySecret = PubConstant.Config.Ayl_accessKeySecret;
        private static readonly string bucketName = "benshiyixin";//存储空间名称
        #endregion


        private static OssClient GetClient()
        {
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            return client;
        }


        /// <summary>
        /// 判断存储空间是否存在
        /// </summary>
        /// <param name="bucketName">存储空间名称</param>
        /// <returns></returns>
        public static bool DoesBucketExist(string bucketName)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                exist = client.DoesBucketExist(bucketName);

            }
            catch (Exception ex)
            {
                LogHelper.Error("判断存储空间是否存在", ex);
            }
            return exist;
        }

        /// <summary>
        /// 上传本地文件
        /// </summary>
        /// <param name="objectName">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <param name="localFilename">本地文件的完整路径。如果未指定本地路径，则默认从示例程序所属项目对应本地路径中上传文件。</param>
        /// <returns></returns>
        public static bool UploadLocalFiles(string objectName, string localFilename)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                // 上传文件。
                var res = client.PutObject(bucketName, objectName, localFilename);
                exist = (res.HttpStatusCode == System.Net.HttpStatusCode.OK);


            }
            catch (Exception ex)
            {
                LogHelper.Error("上传本地文件", ex);
            }
            return exist;
        }




        /// <summary>
        ///分片上传本地文件
        /// </summary>
        /// <param name="objectName">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <param name="localFilename">本地文件的完整路径。如果未指定本地路径，则默认从示例程序所属项目对应本地路径中上传文件。</param>
        /// <returns></returns>
        public static bool UploadLocalFiles2(string objectName, string localFilename)
        {
            var exist = false;
            try
            {
                var client = GetClient();

                // 初始化分片上传，返回uploadId。
                var uploadId = "";
                try
                {
                    // 定义上传的文件及所属Bucket的名称。您可以在InitiateMultipartUploadRequest中设置ObjectMeta，但不必指定其中的ContentLength。
                    var request = new InitiateMultipartUploadRequest(bucketName, objectName);
                    var result = client.InitiateMultipartUpload(request);
                    uploadId = result.UploadId;
                }
                catch (Exception)
                {
                    //Console.WriteLine("Init multi part upload failed, {0}", ex.Message);
                    return false;
                }
                // 计算分片总数。
                var partSize = 10 * 1024 * 1024;
                var fi = new FileInfo(localFilename);
                var fileSize = fi.Length;
                var partCount = fileSize / partSize;
                if (fileSize % partSize != 0)
                {
                    partCount++;
                }
                // 开始分片上传。PartETags是保存PartETag的列表，OSS收到用户提交的分片列表后，会逐一验证每个分片数据的有效性。当所有的数据分片通过验证后，OSS会将这些分片组合成一个完整的文件。
                var partETags = new List<PartETag>();
                try
                {
                    using (var fs = File.Open(localFilename, FileMode.Open))
                    {
                        for (var i = 0; i < partCount; i++)
                        {
                            var skipBytes = (long)partSize * i;
                            // 定位到本次上传的起始位置。
                            fs.Seek(skipBytes, 0);
                            // 计算本次上传的分片大小，最后一片为剩余的数据大小。
                            var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);
                            var request = new UploadPartRequest(bucketName, objectName, uploadId)
                            {
                                InputStream = fs,
                                PartSize = size,
                                PartNumber = i + 1
                            };
                            // 调用UploadPart接口执行上传功能，返回结果中包含了这个数据片的ETag值。
                            var result = client.UploadPart(request);
                            partETags.Add(result.PartETag);
                            //Console.WriteLine("finish {0}/{1}", partETags.Count, partCount);
                        }
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine("Put multi part upload failed, {0}", ex.Message);
                    return false;
                }
                // 完成分片上传。
                try
                {
                    var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(bucketName, objectName, uploadId);
                    foreach (var partETag in partETags)
                    {
                        completeMultipartUploadRequest.PartETags.Add(partETag);
                    }
                    var result = client.CompleteMultipartUpload(completeMultipartUploadRequest);
                    //Console.WriteLine("complete multi part succeeded");
                    exist = (result.HttpStatusCode == System.Net.HttpStatusCode.OK);
                }
                catch (Exception)
                {
                    //Console.WriteLine("complete multi part failed, {0}", ex.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("分片上传本地文件", ex);
            }
            return exist;
        }


        /// <summary>
        /// 上传文字
        /// </summary>
        /// <param name="objectName">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <param name="objectContent ">上传字符串。</param>
        /// <returns></returns>
        public static bool UploadText(string objectName, string objectContent)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                // 上传文件。
                byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);
                MemoryStream requestContent = new MemoryStream(binaryData);
                var res = client.PutObject(bucketName, objectName, requestContent);
                exist = (res.HttpStatusCode == System.Net.HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                LogHelper.Error("上传文字", ex);
            }
            return exist;
        }



        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="objectName">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <returns></returns>
        public static bool DoesObjectExist(string objectName)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                exist = client.DoesObjectExist(bucketName, objectName);

            }
            catch (Exception ex)
            {
                LogHelper.Error("上传本地文件", ex);
            }
            return exist;
        }

        /// <summary>
        /// 简单列举指定Bucket下的个文件
        /// </summary>
        /// <param name="maxKeys">MaxKeys默认值为100，最大值为1000</param>
        /// <param name="prefix">指定前缀的文件</param>
        /// <param name="nextMarker">指定marker文件之后的文件</param>
        /// <returns></returns>
        public static List<string> ListObjects(int maxKeys = 100, string prefix = "", string nextMarker = "")
        {
            var list = new List<string>();
            try
            {
                var client = GetClient();
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = maxKeys,
                    Prefix = prefix,
                };
                var res = client.ListObjects(listObjectsRequest);
                foreach (var summary in res.ObjectSummaries)
                {
                    list.Add(summary.Key);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("列举指定Bucket下的文件", ex);
            }
            return list;
        }


        /// <summary>
        /// 删除单个文件
        /// </summary>
        /// <param name="objectName">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <returns></returns>
        public static bool DeleteObject(string objectName)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                var res = client.DeleteObject(bucketName, objectName);
                exist = (res.HttpStatusCode == System.Net.HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                LogHelper.Error("删除单个文件", ex);
            }
            return exist;
        }

        /// <summary>
        /// 批量删除文件 每次最多删除1000个文件。
        /// </summary>
        /// <param name="keys">Object完整路径。Object完整路径中不能包含Bucket名称。</param>
        /// <param name="quietMode">详细模式（verbose）：未设置quietMode或者设置quietMode为false，表示返回所有删除的文件列表。简单模式（quiet）：设置quietMode为true，表示只返回删除失败的文件列表。</param>
        /// <returns></returns>
        public static bool DeleteObjects(List<string> keys, bool quietMode = true)
        {
            var exist = false;
            try
            {
                var client = GetClient();
                var request = new DeleteObjectsRequest(bucketName, keys, quietMode);
                var res = client.DeleteObjects(request);
                exist = (res.HttpStatusCode == System.Net.HttpStatusCode.OK);
                if (quietMode && (res.Keys != null && res.Keys.Length > 0))
                {
                    exist = false;
                    //foreach (var obj in res.Keys)
                    //{
                    //    Console.WriteLine("Delete successfully : {0} ", obj.Key);
                    //}
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("批量删除文件", ex);
            }
            return exist;
        }
    }
}

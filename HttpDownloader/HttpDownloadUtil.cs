using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpDownloader
{
    class HttpDownloadUtil
    {
        LogUtil _logUtil = null;
        public HttpDownloadUtil(LogUtil logUtil)
        {
            _logUtil = logUtil;
        }
        internal async void StartDownloadAsync(string url, string headers, string filePath)
        {
            _logUtil.AddLog("Started Download");
            HttpClient client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage();
                Dictionary<string, string> headersParsed = ParseHeaders(headers);
                requestMessage.Method = HttpMethod.Get;
                requestMessage.RequestUri = new Uri(url);
                foreach (var kvp in headersParsed)
                {
                    requestMessage.Headers.Add(kvp.Key, kvp.Value);
                }
                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
            catch(Exception ex)
            {
                _logUtil.AddLog(ex.ToString());
            }

            _logUtil.AddLog("Finished Download");
        }


        internal async void FollowStreamAsync(string url, string headers, string filePath, bool useNewParseHeader)
        {
            _logUtil.AddLog("Started Following");
            HttpClient client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage();

                Dictionary<string, string> headersParsed = useNewParseHeader ? ParseHeaders2(headers) : ParseHeaders(headers);
                requestMessage.Method = HttpMethod.Get;
                requestMessage.RequestUri = new Uri(url);
                foreach (var kvp in headersParsed)
                {
                    requestMessage.Headers.Add(kvp.Key, kvp.Value);
                }
                //var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                var response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string m3u8Contents = await response.Content.ReadAsStringAsync();
                var byteArr = await response.Content.ReadAsByteArrayAsync();
                using (var fs = new FileStream(filePath+"m3u8", FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArr, 0, byteArr.Length);
                }
                FollowM3U8(m3u8Contents, url, filePath, headersParsed);
            }
            catch (Exception ex)
            {
                _logUtil.AddLog(ex.ToString());
            }

            _logUtil.AddLog("Finished Download");
        }

        private async void FollowM3U8(string m3u8Contents, string url, string filePath, Dictionary<string, string> headersParsed)
        {
            // [1] Determine the base url
            //string m3u8Keyword = "index.m3u8";
            string m3u8Keyword = "v.m3u8";
            int baseUrlEnd = url.IndexOf(m3u8Keyword);
            if (baseUrlEnd < 0)
            {
                throw new Exception("Cannot determine base url");
            }

            string baseUrl = url.Substring(0, baseUrlEnd);

            // [2] Parse each segment
            string[] lines = m3u8Contents.Split("\n");
            foreach(string line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                _logUtil.AddLog($"Following {line}");

                try
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    HttpRequestMessage requestMessage = new HttpRequestMessage();
                    foreach (var kvp in headersParsed)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                    requestMessage.RequestUri = new Uri(baseUrl+ line);

                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create | FileMode.Append))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logUtil.AddLog(ex.ToString());
                }
            }
        }

        private Dictionary<string, string> ParseHeaders(string headers)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            string[] lines = headers.Split("\r\n");
            foreach(string line in lines)
            {
                string[] keyVal = line.Split(':');
                if (keyVal.Length >= 2)
                {
                    //retDict.Add(keyVal[0], keyVal[1].Trim()); // may contain multiple colons like "referer: https://www..."
                    int firstColon = line.IndexOf(':');
                    retDict.Add(keyVal[0], line.Substring(firstColon + 1).Trim());
                }
            }

            return retDict;
        }

        // Note: Remove accept-encoding gzip, deflate, br, zstd
        // Should return text format!
        private Dictionary<string, string> ParseHeaders2(string headers)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            string[] lines = headers.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string line in lines)
            {
                string[] keyVal = line.Split('♦');
                retDict.Add(keyVal[0], keyVal[1]);
            }

            return retDict;
        }
    }
}

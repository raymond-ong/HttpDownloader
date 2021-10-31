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
    }
}

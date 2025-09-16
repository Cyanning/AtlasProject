using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;


namespace Plugins.C_
{
    public class DownLoader : MonoBehaviour
    {
        private string cacheFolder;
        private int completedDownloads = 0;


        public void StartDownload(List<string> fileUrls, string saveFolder, Action onResult)
        {
            this.cacheFolder = saveFolder;
            Debug.Log($"save Paht = {cacheFolder}");
            StartCoroutine(DownloadAndCacheImages(fileUrls, onResult));
        }

        private IEnumerator DownloadAndCacheImages(List<string> fileUrls, Action onResult)
        {
            foreach (var url in fileUrls)
            {
                // 启动下载协程，并传递一个用于更新完成下载计数的回调函数
                StartCoroutine(DownloadImage(url, UpdateDownloadCount));
            }

            // 等待所有下载完成
            while (completedDownloads < fileUrls.Count)
            {
                yield return null; // 等待下一帧
            }

            Debug.Log("1111"); // 所有图片下载完成
            onResult();
        }

        private IEnumerator DownloadImage(string url, System.Action<bool, string> onComplete)
        {
            string localPath = GetLocalFilePath(url);

            // 检查文件是否已存在
            if (File.Exists(localPath))
            {
                Debug.Log($"Skipping {url} because it's already cached.");
                onComplete(true, url); // 假设缓存读取也算作“完成”
                yield break;
            }

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading {url}: {request.error}");
                onComplete(false, url); // 下载失败
                yield break;
            }

            // 获取Texture2D并保存到文件（如果需要Texture2D，否则可以直接保存bytes）
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            byte[] fileBytes = texture.EncodeToPNG();

            // 确保缓存文件夹存在
            Directory.CreateDirectory(cacheFolder);

            // 将图片保存到文件
            File.WriteAllBytes(localPath, fileBytes);

            Debug.Log($"Downloaded and cached {url} to {localPath}");

            // 清理资源
            request.Dispose();
            Destroy(texture);
            onComplete(true, url);
        }


        private void UpdateDownloadCount(bool success, string url)
        {
            Debug.Log($"download {success},url = {url}");
            completedDownloads++;
        }

        public string GetLocalFilePath(string url)
        {
            string fileName = Path.GetFileName(new Uri(url).LocalPath);
            return Path.Combine(cacheFolder, fileName);
        }
    }
}

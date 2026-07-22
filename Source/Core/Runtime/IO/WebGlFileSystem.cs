#if !UNITY_EDITOR && UNITY_WEBGL
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    public class WebGlFileSystem : DefaultFileSystem
    {
        public WebGlFileSystem(string streamingAssetsPath, string persistentDataPath) : base(streamingAssetsPath, persistentDataPath)
        {
        }

        public override Task<IProcessAssetManifest> FetchManifest(string processName, string manifestPath, IProcessSerializer serializer)
        {
            IProcessAssetManifest manifest = new ProcessAssetManifest()
            {
                AssetStrategyTypeName = typeof(SingleFileProcessAssetStrategy).FullName,
                ProcessFileName = processName,
                AdditionalFileNames = Array.Empty<string>(),
            };
            return Task.FromResult(manifest);
        }

        protected override async Task<bool> FileExistsInStreamingAssets(string filePath)
        {
            string absolutePath = Path.Combine(StreamingAssetsPath, filePath);

            var webRequest = UnityWebRequest.Head(absolutePath);
            await webRequest.SendWebRequest();
            return webRequest.responseCode != 404;
        }

        protected override async Task<byte[]> ReadFromStreamingAssets(string filePath)
        {
            string absolutePath = Path.Combine(StreamingAssetsPath, filePath);

            var webRequest = UnityWebRequest.Get(absolutePath);
            await webRequest.SendWebRequest();
            return webRequest.downloadHandler.data;
        }
    }
}

public static class ExtensionMethods
{
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        if (asyncOp.isDone)
        {
            tcs.SetResult(null);
        }
        else
        {
            asyncOp.completed += obj => { tcs.SetResult(null); };
        }

        return ((Task)tcs.Task).GetAwaiter();
    }
}
#endif

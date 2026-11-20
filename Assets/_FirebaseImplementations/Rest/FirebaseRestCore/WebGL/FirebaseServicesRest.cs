#if UNITY_WEBGL && !UNITY_EDITOR
// REST implementation of Firebase.Storage / Firebase.Functions and a no-op Firebase.Analytics.
// Only compiled into WebGL players, where the Firebase Unity SDK is not available.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yaguar.FirebaseRest;

namespace Firebase.Storage
{
    public class StorageException : Exception
    {
        public StorageException(string message) : base(message) { }
    }

    public class StorageMetadata
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
    }

    public class FirebaseStorage
    {
        public static FirebaseStorage DefaultInstance { get; } = new FirebaseStorage();

        public StorageReference GetReference(string path)
        {
            return new StorageReference(path);
        }
    }

    // NOTE: browsers can only download/upload if the bucket has a CORS configuration allowing the game's origin.
    public class StorageReference
    {
        const string BaseUrl = "https://firebasestorage.googleapis.com/v0/b/" + FirebaseRestConfig.StorageBucket + "/o";

        readonly string path;

        internal StorageReference(string path)
        {
            this.path = (path ?? "").Trim('/');
        }

        public string Path { get { return path; } }

        static async Task<Dictionary<string, string>> AuthHeaders()
        {
            string token = await FirebaseRestSession.GetIdTokenAsync();
            var h = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(token))
                h["Authorization"] = "Firebase " + token;
            return h;
        }

        public async Task<StorageMetadata> PutBytesAsync(byte[] bytes)
        {
            var headers = await AuthHeaders();
            string contentType = path.EndsWith(".jpg") || path.EndsWith(".jpeg") ? "image/jpeg"
                : path.EndsWith(".png") ? "image/png" : "application/octet-stream";

            var res = await FirebaseRestHttp.SendAsync("POST", BaseUrl + "?name=" + Uri.EscapeDataString(path), bytes, contentType, headers);
            if (!res.Ok)
                throw new StorageException("Upload " + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
            return new StorageMetadata { Name = System.IO.Path.GetFileName(path), Path = path };
        }

        // The home screen requests every thumbnail at once; cap the parallel downloads so the single WebGL thread
        // and the browser connection pool are not flooded.
        const int MaxParallelDownloads = 6;
        static int activeDownloads;
        static readonly Queue<TaskCompletionSource<bool>> waitingDownloads = new Queue<TaskCompletionSource<bool>>();

        static async Task AcquireDownloadSlot()
        {
            if (activeDownloads < MaxParallelDownloads)
            {
                activeDownloads++;
                return;
            }
            var tcs = new TaskCompletionSource<bool>();
            waitingDownloads.Enqueue(tcs);
            await tcs.Task; // the slot is handed over by ReleaseDownloadSlot
        }

        static void ReleaseDownloadSlot()
        {
            if (waitingDownloads.Count > 0)
                waitingDownloads.Dequeue().SetResult(true);
            else
                activeDownloads--;
        }

        public async Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
        {
            await AcquireDownloadSlot();
            try
            {
                return await DownloadAsync(maxDownloadSizeBytes);
            }
            finally
            {
                ReleaseDownloadSlot();
            }
        }

        async Task<byte[]> DownloadAsync(long maxDownloadSizeBytes)
        {
            var headers = await AuthHeaders();
            var res = await FirebaseRestHttp.SendAsync("GET", BaseUrl + "/" + Uri.EscapeDataString(path) + "?alt=media", null, null, headers, true);
            if (!res.Ok)
                throw new StorageException("Download " + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
            if (res.Data != null && res.Data.LongLength > maxDownloadSizeBytes)
                throw new StorageException("Download " + path + " exceeds the max allowed size");
            return res.Data;
        }

        public async Task DeleteAsync()
        {
            var headers = await AuthHeaders();
            var res = await FirebaseRestHttp.SendAsync("DELETE", BaseUrl + "/" + Uri.EscapeDataString(path), null, null, headers);
            if (!res.Ok)
                throw new StorageException("Delete " + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
        }
    }
}

namespace Firebase.Functions
{
    public class FunctionsException : Exception
    {
        public FunctionsException(string message) : base(message) { }
    }

    public class HttpsCallableResult
    {
        public object Data { get; internal set; }
    }

    public class FirebaseFunctions
    {
        public static FirebaseFunctions DefaultInstance { get; } = new FirebaseFunctions();

        string baseUrl = "https://" + FirebaseRestConfig.FunctionsRegion + "-" + FirebaseRestConfig.ProjectId + ".cloudfunctions.net";

        public void UseFunctionsEmulator(string origin)
        {
            baseUrl = origin.TrimEnd('/') + "/" + FirebaseRestConfig.ProjectId + "/" + FirebaseRestConfig.FunctionsRegion;
        }

        public HttpsCallableReference GetHttpsCallable(string name)
        {
            return new HttpsCallableReference(baseUrl + "/" + name);
        }
    }

    public class HttpsCallableReference
    {
        readonly string url;

        internal HttpsCallableReference(string url)
        {
            this.url = url;
        }

        public Task<HttpsCallableResult> CallAsync()
        {
            return CallAsync(null);
        }

        // Callable protocol: POST {"data": ...} -> {"result": ...} | {"error": {"message": ...}}
        public async Task<HttpsCallableResult> CallAsync(object data)
        {
            string token = await FirebaseRestSession.GetIdTokenAsync();
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(token))
                headers["Authorization"] = "Bearer " + token;

            string body = "{\"data\":" + (data == null ? "null" : JsonConvert.SerializeObject(data)) + "}";
            var res = await FirebaseRestHttp.SendAsync("POST", url, Encoding.UTF8.GetBytes(body), "application/json", headers);

            if (!res.Ok)
            {
                string message = res.IsNetworkError ? "network error" : res.Status.ToString();
                try
                {
                    var err = JObject.Parse(res.Text)["error"];
                    if (err != null)
                        message += " " + err["status"] + ": " + err["message"];
                }
                catch (Exception) { }
                throw new FunctionsException("Call " + url + " failed: " + message);
            }

            var json = JObject.Parse(res.Text);
            return new HttpsCallableResult { Data = json["result"]?.ToObject<object>() };
        }
    }
}

namespace Firebase.Analytics
{
    public class Parameter
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public Parameter(string parameterName, string parameterValue) { Name = parameterName; Value = parameterValue; }
        public Parameter(string parameterName, long parameterValue) { Name = parameterName; Value = parameterValue; }
        public Parameter(string parameterName, int parameterValue) { Name = parameterName; Value = parameterValue; }
        public Parameter(string parameterName, double parameterValue) { Name = parameterName; Value = parameterValue; }
    }

    // Firebase Analytics has no REST equivalent that works from a browser without extra credentials: events are dropped in WebGL.
    public static class FirebaseAnalytics
    {
        public static void LogEvent(string name) { }
        public static void LogEvent(string name, string parameterName, string parameterValue) { }
        public static void LogEvent(string name, string parameterName, long parameterValue) { }
        public static void LogEvent(string name, string parameterName, double parameterValue) { }
        public static void LogEvent(string name, params Parameter[] parameters) { }
        public static void SetUserProperty(string name, string property) { }
        public static void SetUserId(string userId) { }
    }
}
#endif

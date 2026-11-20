using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Yaguar.FirebaseRest
{
    public sealed class RestResponse
    {
        public long Status;
        public bool IsNetworkError;
        public byte[] Data;
        public string Text;
        public string ETag;

        public bool Ok { get { return !IsNetworkError && Status >= 200 && Status < 300; } }
    }

    public class FirebaseRestException : Exception
    {
        public long Status { get; private set; }
        public string Body { get; private set; }

        public FirebaseRestException(string message, long status = 0, string body = null) : base(message)
        {
            Status = status;
            Body = body;
        }

        public static FirebaseRestException From(string what, RestResponse r)
        {
            if (r.IsNetworkError)
                return new FirebaseRestException(what + ": network error", 0, r.Text);
            return new FirebaseRestException(what + ": HTTP " + r.Status + " " + r.Text, r.Status, r.Text);
        }
    }

    /// <summary>
    /// Minimal UnityWebRequest wrapper that returns a Task. Must be called from the main thread
    /// (it is completed from the UnityWebRequest callback, so it also works in WebGL, which has no threads).
    /// </summary>
    public static class FirebaseRestHttp
    {
        public static Task<RestResponse> SendAsync(string method, string url, byte[] body = null, string contentType = null,
            Dictionary<string, string> headers = null, bool binaryResponse = false)
        {
            var tcs = new TaskCompletionSource<RestResponse>();

            var req = new UnityWebRequest(url, method);
            req.downloadHandler = new DownloadHandlerBuffer();
            if (body != null)
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                if (contentType != null)
                    req.SetRequestHeader("Content-Type", contentType);
            }
            if (headers != null)
            {
                foreach (var h in headers)
                    req.SetRequestHeader(h.Key, h.Value);
            }

            var op = req.SendWebRequest();
            op.completed += _ =>
            {
                var res = new RestResponse();
                res.Status = req.responseCode;
                res.IsNetworkError = req.result == UnityWebRequest.Result.ConnectionError;
                res.ETag = req.GetResponseHeader("ETag");
                if (req.downloadHandler != null)
                {
                    res.Data = req.downloadHandler.data;
                    if (!binaryResponse || !res.Ok)
                        res.Text = req.downloadHandler.text;
                }
                req.Dispose();
                tcs.TrySetResult(res);
            };

            return tcs.Task;
        }
    }
}

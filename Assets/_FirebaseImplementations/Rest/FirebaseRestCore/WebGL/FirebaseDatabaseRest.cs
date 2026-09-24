#if UNITY_WEBGL && !UNITY_EDITOR
// REST implementation of the subset of Firebase.Database used by the app. Only compiled into WebGL players,
// where the Firebase Unity SDK is not available. Everywhere else the real SDK is used.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yaguar.FirebaseRest;

namespace Firebase.Database
{
    public class DatabaseError
    {
        public string Message { get; internal set; }
        public int Code { get; internal set; }
        public string Details { get; internal set; }
    }

    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message) { }
    }

    public class ChildChangedEventArgs : EventArgs
    {
        public DatabaseError DatabaseError { get; internal set; }
        public DataSnapshot Snapshot { get; internal set; }
        public string PreviousChildName { get; internal set; }
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public DatabaseError DatabaseError { get; internal set; }
        public DataSnapshot Snapshot { get; internal set; }
    }

    internal static class RestJson
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

        // Dates are kept as plain strings: the app stores ISO timestamps as strings and reads them back with "as string".
        public static JToken Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return JValue.CreateNull();
            return JsonConvert.DeserializeObject<JToken>(text, Settings) ?? JValue.CreateNull();
        }

        // Same value types the Firebase SDK exposes through DataSnapshot.Value.
        public static object ToValue(JToken t)
        {
            if (t == null)
                return null;
            switch (t.Type)
            {
                case JTokenType.Integer: return t.Value<long>();
                case JTokenType.Float: return t.Value<double>();
                case JTokenType.String: return t.Value<string>();
                case JTokenType.Boolean: return t.Value<bool>();
                case JTokenType.Object:
                    var d = new Dictionary<string, object>();
                    foreach (var p in ((JObject)t).Properties())
                        d[p.Name] = ToValue(p.Value);
                    return d;
                case JTokenType.Array:
                    var l = new List<object>();
                    foreach (var item in (JArray)t)
                        l.Add(ToValue(item));
                    return l;
                default: return null;
            }
        }
    }

    public class DataSnapshot
    {
        readonly JToken token;
        readonly string rawJson;

        public string Key { get; private set; }

        internal DataSnapshot(string key, JToken token, string rawJson = null)
        {
            Key = key;
            this.token = token;
            this.rawJson = rawJson;
        }

        internal static DataSnapshot FromJson(string key, string json)
        {
            return new DataSnapshot(key, RestJson.Parse(json), json);
        }

        public bool Exists { get { return token != null && token.Type != JTokenType.Null; } }
        public bool HasChildren { get { return ChildrenCount > 0; } }

        public object Value { get { return Exists ? RestJson.ToValue(token) : null; } }

        public long ChildrenCount
        {
            get
            {
                if (!Exists) return 0;
                if (token is JObject o) return o.Count;
                if (token is JArray a) return a.Count(x => x.Type != JTokenType.Null);
                return 0;
            }
        }

        public IEnumerable<DataSnapshot> Children
        {
            get
            {
                if (token is JObject o)
                {
                    foreach (var p in o.Properties())
                        yield return new DataSnapshot(p.Name, p.Value);
                }
                else if (token is JArray a)
                {
                    // Firebase turns objects with sequential numeric keys into arrays; gaps come back as null.
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (a[i].Type != JTokenType.Null)
                            yield return new DataSnapshot(i.ToString(), a[i]);
                    }
                }
            }
        }

        public DataSnapshot Child(string path)
        {
            JToken current = token;
            string key = Key;
            foreach (string part in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                key = part;
                if (current is JObject o)
                    current = o[part];
                else if (current is JArray a && int.TryParse(part, out int idx) && idx >= 0 && idx < a.Count)
                    current = a[idx];
                else
                    current = null;
                if (current == null)
                    break;
            }
            return new DataSnapshot(key, current ?? JValue.CreateNull());
        }

        public bool HasChild(string path)
        {
            return Child(path).Exists;
        }

        public string GetRawJsonValue()
        {
            if (!Exists)
                return null;
            return rawJson ?? token.ToString(Formatting.None);
        }

        public string GetJson()
        {
            return GetRawJsonValue();
        }
    }

    public class MutableData
    {
        public object Value { get; set; }
        internal MutableData(object value) { Value = value; }
    }

    public class TransactionResult
    {
        internal bool IsSuccess;
        internal MutableData Data;

        public static TransactionResult Success(MutableData data)
        {
            return new TransactionResult { IsSuccess = true, Data = data };
        }

        public static TransactionResult Abort()
        {
            return new TransactionResult { IsSuccess = false };
        }
    }

    public class FirebaseDatabase
    {
        public static FirebaseDatabase DefaultInstance { get; } = new FirebaseDatabase();

        public void SetPersistenceEnabled(bool enabled) { }

        public DatabaseReference RootReference { get { return new DatabaseReference(""); } }

        public DatabaseReference GetReference(string path)
        {
            return new DatabaseReference(path);
        }
    }

    public class Query
    {
        protected readonly string path;
        string orderByChild;
        string equalToJson;

        internal Query(string path)
        {
            this.path = (path ?? "").Trim('/');
        }

        Query(string path, string orderByChild, string equalToJson) : this(path)
        {
            this.orderByChild = orderByChild;
            this.equalToJson = equalToJson;
        }

        public Query OrderByChild(string childKey)
        {
            return new Query(path, childKey, equalToJson);
        }

        public Query EqualTo(string value) { return new Query(path, orderByChild, JsonConvert.SerializeObject(value)); }
        public Query EqualTo(bool value) { return new Query(path, orderByChild, value ? "true" : "false"); }
        public Query EqualTo(long value) { return new Query(path, orderByChild, value.ToString()); }

        internal string Url(string token, string extraQuery = null)
        {
            var sb = new StringBuilder(FirebaseRestConfig.DatabaseUrl);
            foreach (string seg in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                sb.Append('/').Append(Uri.EscapeDataString(seg));
            sb.Append(".json");

            var q = new List<string>();
            if (!string.IsNullOrEmpty(token))
                q.Add("auth=" + Uri.EscapeDataString(token));
            if (orderByChild != null)
                q.Add("orderBy=" + Uri.EscapeDataString(JsonConvert.SerializeObject(orderByChild)));
            if (equalToJson != null)
                q.Add("equalTo=" + Uri.EscapeDataString(equalToJson));
            if (!string.IsNullOrEmpty(extraQuery))
                q.Add(extraQuery);
            if (q.Count > 0)
                sb.Append('?').Append(string.Join("&", q));
            return sb.ToString();
        }

        public async Task<DataSnapshot> GetValueAsync()
        {
            string token = await FirebaseRestSession.GetIdTokenAsync();
            var res = await FirebaseRestHttp.SendAsync("GET", Url(token));

            if (!res.Ok && orderByChild != null && res.Status == 400 && res.Text != null && res.Text.Contains("Index not defined"))
                return await GetFilteredClientSide(token);

            if (!res.Ok)
                throw new DatabaseException("GET /" + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));

            return DataSnapshot.FromJson(LastSegment(path), res.Text);
        }

        // The SDK falls back to client side filtering when the rules have no ".indexOn"; the REST API returns an error instead.
        async Task<DataSnapshot> GetFilteredClientSide(string token)
        {
            var res = await FirebaseRestHttp.SendAsync("GET", new Query(path).Url(token));
            if (!res.Ok)
                throw new DatabaseException("GET /" + path + " failed: " + res.Status + " " + res.Text);

            var all = RestJson.Parse(res.Text) as JObject;
            var filtered = new JObject();
            if (all != null)
            {
                JToken wanted = JToken.Parse(equalToJson);
                foreach (var p in all.Properties())
                {
                    if (p.Value is JObject child && child[orderByChild] != null && JToken.DeepEquals(child[orderByChild], wanted))
                        filtered.Add(p.Name, p.Value);
                }
            }
            return new DataSnapshot(LastSegment(path), filtered.Count > 0 ? (JToken)filtered : JValue.CreateNull());
        }

        internal static string LastSegment(string p)
        {
            int i = p.LastIndexOf('/');
            return i < 0 ? p : p.Substring(i + 1);
        }
    }

    public class DatabaseReference : Query
    {
        // Realtime listeners are not available over plain REST in WebGL: subscriptions are accepted but never fire.
        public event EventHandler<ChildChangedEventArgs> ChildAdded { add { } remove { } }
        public event EventHandler<ChildChangedEventArgs> ChildChanged { add { } remove { } }
        public event EventHandler<ChildChangedEventArgs> ChildRemoved { add { } remove { } }
        public event EventHandler<ValueChangedEventArgs> ValueChanged { add { } remove { } }

        internal DatabaseReference(string path) : base(path) { }

        public string Key { get { return LastSegment(path); } }

        public DatabaseReference Child(string childPath)
        {
            return new DatabaseReference(path.Length == 0 ? childPath : path + "/" + childPath.Trim('/'));
        }

        public DatabaseReference Push()
        {
            return Child(PushId.Next());
        }

        public Task SetRawJsonValueAsync(string json)
        {
            return WriteAsync("PUT", json);
        }

        public Task SetValueAsync(object value)
        {
            return WriteAsync("PUT", JsonConvert.SerializeObject(value));
        }

        public Task UpdateChildrenAsync(IDictionary<string, object> update)
        {
            return WriteAsync("PATCH", JsonConvert.SerializeObject(update));
        }

        public Task RemoveValueAsync()
        {
            return WriteAsync("DELETE", null);
        }

        async Task WriteAsync(string method, string json)
        {
            string token = await FirebaseRestSession.GetIdTokenAsync();
            string httpMethod = method;
            string extra = "print=silent";
            if (method == "PATCH")
            {
                // UnityWebRequest in WebGL does not reliably send PATCH; RTDB accepts a method override on a POST.
                httpMethod = "POST";
                extra += "&x-http-method-override=PATCH";
            }

            var res = await FirebaseRestHttp.SendAsync(httpMethod, Url(token, extra),
                json != null ? Encoding.UTF8.GetBytes(json) : null, json != null ? "application/json" : null);
            if (!res.Ok)
                throw new DatabaseException(method + " /" + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
        }

        /// <summary>
        /// Optimistic transaction: read the value with its ETag, apply the update function and write it back
        /// only if the ETag still matches (retrying on conflict).
        /// </summary>
        public async Task<DataSnapshot> RunTransaction(Func<MutableData, TransactionResult> transactionUpdate)
        {
            const int maxAttempts = 5;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string token = await FirebaseRestSession.GetIdTokenAsync();
                var get = await FirebaseRestHttp.SendAsync("GET", Url(token), null, null,
                    new Dictionary<string, string> { { "X-Firebase-ETag", "true" } });
                if (!get.Ok)
                    throw new DatabaseException("Transaction read /" + path + " failed: " + get.Status + " " + get.Text);

                var current = DataSnapshot.FromJson(Key, get.Text);
                var data = new MutableData(current.Value);
                var result = transactionUpdate(data);
                if (result == null || !result.IsSuccess)
                    return current;

                string newJson = JsonConvert.SerializeObject(result.Data.Value);
                var headers = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(get.ETag))
                    headers["if-match"] = get.ETag;

                var put = await FirebaseRestHttp.SendAsync("PUT", Url(token), Encoding.UTF8.GetBytes(newJson), "application/json", headers);
                if (put.Ok)
                    return DataSnapshot.FromJson(Key, newJson);
                if (put.Status != 412)
                    throw new DatabaseException("Transaction write /" + path + " failed: " + put.Status + " " + put.Text);
            }
            throw new DatabaseException("Transaction /" + path + " aborted: too many conflicts");
        }
    }

    // Same algorithm as the Firebase SDKs: 8 chars of timestamp + 12 random chars, so keys stay chronologically ordered.
    internal static class PushId
    {
        const string Chars = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        static readonly System.Random Rng = new System.Random();
        static long lastTime;
        static readonly int[] LastRandom = new int[12];

        public static string Next()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool duplicate = now == lastTime;
            lastTime = now;

            var id = new char[20];
            for (int i = 7; i >= 0; i--)
            {
                id[i] = Chars[(int)(now % 64)];
                now /= 64;
            }

            if (!duplicate)
            {
                for (int i = 0; i < 12; i++)
                    LastRandom[i] = Rng.Next(64);
            }
            else
            {
                int j = 11;
                while (j >= 0 && LastRandom[j] == 63)
                {
                    LastRandom[j] = 0;
                    j--;
                }
                if (j >= 0)
                    LastRandom[j]++;
            }

            for (int i = 0; i < 12; i++)
                id[8 + i] = Chars[LastRandom[i]];
            return new string(id);
        }
    }
}

namespace Firebase.Extensions
{
    public static class TaskExtension
    {
        // Continuations run on the Unity main thread (WebGL has no other thread anyway).
        public static Task ContinueWithOnMainThread(this Task task, Action<Task> continuation)
        {
            return task.ContinueWith(continuation, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task ContinueWithOnMainThread<T>(this Task<T> task, Action<Task<T>> continuation)
        {
            return task.ContinueWith(continuation, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
#endif

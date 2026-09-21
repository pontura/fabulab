namespace Yaguar.FirebaseRest
{
    /// <summary>
    /// Public client identifiers of the Firebase project (same values as google-services.json).
    /// They are not secrets: access is enforced by the Database / Storage security rules.
    /// </summary>
    public static class FirebaseRestConfig
    {
        public const string ApiKey = "AIzaSyDoW39-IFpYWfilGC6oC3HcgZBERIHho0Q";
        public const string DatabaseUrl = "https://fabu-lab-default-rtdb.firebaseio.com";
        public const string StorageBucket = "fabu-lab.firebasestorage.app";
        public const string ProjectId = "fabu-lab";

        /// <summary>Region where the callable functions are deployed (change if they are not in us-central1).</summary>
        public const string FunctionsRegion = "us-central1";
    }
}

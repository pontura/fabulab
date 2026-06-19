using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
#elif UNITY_IOS
using Apple.GameKit;
using System.Threading.Tasks;
#endif

public class SocialAuth : MonoBehaviour
{

    [SerializeField] int _maxAuthAttempts = 2;
    int _authTriesCount;
    System.Action<string> onSocialSigned;

    async public void Init(System.Action<string> onSocialSigned)
    {

        this.onSocialSigned = onSocialSigned;

#if UNITY_EDITOR || UNITY_STANDALONE
            onSocialSigned("");
#elif UNITY_ANDROID
            Debug.Log("#ACA2");
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);            
#elif UNITY_IOS
            Debug.Log("iOs");
            await AppleGameCenterLogin();
#endif
    }

#if UNITY_IOS
        string Signature;
        string TeamPlayerID;
        string Salt;
        string PublicKeyUrl;
        string Timestamp;
        public async Task AppleGameCenterLogin()
        {
            Debug.Log("AppleGameCenterLogin");
            if (!GKLocalPlayer.Local.IsAuthenticated)
            {
                // Perform the authentication.
                var player = await GKLocalPlayer.Authenticate();
                Debug.Log($"GameKit Authentication: player {player}");

                // Grab the display name.
                var localPlayer = GKLocalPlayer.Local;
                Debug.Log($"Local Player: {localPlayer.DisplayName}");

                // Fetch the items.
                var fetchItemsResponse = await GKLocalPlayer.Local.FetchItems();

                Signature = Convert.ToBase64String(fetchItemsResponse.GetSignature());
                TeamPlayerID = localPlayer.TeamPlayerId;
                Debug.Log($"Team Player ID: {TeamPlayerID}");

                Salt = Convert.ToBase64String(fetchItemsResponse.GetSalt());
                PublicKeyUrl = fetchItemsResponse.PublicKeyUrl;
                Timestamp = fetchItemsResponse.Timestamp.ToString();

                Debug.Log($"GameKit Authentication: signature => {Signature}");
                Debug.Log($"GameKit Authentication: publickeyurl => {PublicKeyUrl}");
                Debug.Log($"GameKit Authentication: salt => {Salt}");
                Debug.Log($"GameKit Authentication: Timestamp => {Timestamp}");
            }
            else
            {
                Debug.Log("AppleGameCenter player already logged in.");
            }
            LoadUserData();
        }
#endif

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (_authTriesCount > 0)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
#endif
            }
        }
    }
    
#if UNITY_ANDROID
    void ProcessAuthentication(SignInStatus status)
    {
        Debug.Log("ProcessAuthentication for userName:" + Social.localUser.userName + " (" + Social.localUser.id + ")");
        if (status == SignInStatus.Success)
        {
            Debug.Log("#ProcessAuthentication Success");
            _authTriesCount = 0;
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, onSocialSigned);
        }
        else
        {
            if (_authTriesCount < _maxAuthAttempts)
            {
                PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
            }
            else
            {
                onSocialSigned("");
            }
            _authTriesCount++;
        }
    }
#endif   
   
}

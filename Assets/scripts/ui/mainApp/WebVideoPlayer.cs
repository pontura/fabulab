using System;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.Auth;

public class WebVideoPlayer : MonoBehaviour
{
    [SerializeField] AllStoriesScreen allStoriesScreen;
    [SerializeField] string id = "-P1LdwgrQPyJIHeUbXox";
    string url;
    
    void Start()
    {
        url = Application.absoluteURL;
        if(url != "")
        {
            var uri = new System.Uri(url);
            id = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("id");           
        }
    }

    public void Init()
    {        
        print("WebVideoPlayer open work from url:" + url);
        print("open work id:" + id);
        allStoriesScreen.OpenWork(id);
    }
}

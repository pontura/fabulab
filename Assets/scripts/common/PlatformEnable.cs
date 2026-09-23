using UnityEngine;

public class PlatformEnable : MonoBehaviour
{
    [SerializeField] bool editor, ios, android, webgl, win, osx, linux;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if UNITY_EDITOR
        gameObject.SetActive(editor);
#elif UNITY_ANDROID
    gameObject.SetActive(android);
#elif UNITY_IOS
    gameObject.SetActive(ios);
#elif UNITY_WEBGL
    gameObject.SetActive(ios);
#elif UNITY_STANDALONE_WIN
    gameObject.SetActive(win);
#elif UNITY_STANDALONE_OSX
    gameObject.SetActive(osx);
#elif UNITY_STANDALONE_LINUX
    gameObject.SetActive(linux);
#else
    gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

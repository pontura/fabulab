using BoardItems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Data : MonoBehaviour
{
    static Data mInstance = null;
    public bool DEBUG;
    public string lastScene;
    public string newScene;
    private float time_ViewingMap = 7.5f;
    public UserData userData;
    public AdminData adminData;
    public PalettesManager palettesManager;
    public GaleriasData galeriasData;
    public CharactersData charactersData;
    public AnimationsManager animationsManager;
    public Settings settings;

    public static Data Instance {  get  {  return mInstance;  }  }

    public void LoadLevel(string aLevelName, bool showMap)
    {
        this.newScene = aLevelName;
        Invoke("LoadDelayed", 0.75f);
    }
    void LoadDelayed()
    {
        SceneManager.LoadScene(newScene);
    }
    void Awake()
    {
        if (!mInstance)
            mInstance = this;

        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this);

        animationsManager = GetComponent<AnimationsManager>();
    }
}
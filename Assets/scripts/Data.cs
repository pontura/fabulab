using BoardItems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Data : MonoBehaviour
{
    const string PREFAB_PATH = "Data";
    static Data mInstance = null;
    public bool DEBUG;
    public string lastScene;
    public string newScene;
    private float time_ViewingMap = 7.5f;
    public PalettesManager palettesManager;
    public GaleriasData galeriasData;
    public AlbumData albumData;
    public AnimationsManager animationsManager;
    public Settings settings;

    public static Data Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<Data>();

                if (mInstance == null)
                {
                    GameObject go = Instantiate(Resources.Load<GameObject>(PREFAB_PATH)) as GameObject;
                    mInstance = go.GetComponent<Data>();
                    go.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            return mInstance;
        }
    }
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
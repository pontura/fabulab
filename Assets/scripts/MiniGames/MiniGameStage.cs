using BoardItems;
using BoardItems.Characters;
using BoardItems.SceneObjects;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace MiniGames
{
    // World space where minigames are played. It lives far from the rest of the app and renders
    // through its own camera into a square RenderTexture that the minigame screen shows in a RawImage
    public class MiniGameStage : MonoBehaviour
    {
        [SerializeField] CharacterManager characterPrefab;
        [SerializeField] SceneObjectManager objectPrefab;
        [SerializeField] Vector3 worldPosition = new Vector3(0, -5000, 0);
        [SerializeField] float cameraSize = 5;
        [SerializeField] Color backgroundColor = new Color(0.75f, 0.9f, 1f);
        [SerializeField] int textureSize = 1024;
        [SerializeField] float characterScale = 1;

        Camera cam;
        RenderTexture renderTexture;

        public Transform Content { get; private set; }
        public Camera Cam => cam;
        public float CameraSize => cameraSize;

        void Build()
        {
            if (cam != null) return;

            transform.position = worldPosition;

            Content = new GameObject("Content").transform;
            Content.SetParent(transform, false);

            cam = new GameObject("MiniGameCamera").AddComponent<Camera>();
            cam.transform.SetParent(transform, false);
            cam.transform.localPosition = new Vector3(0, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = cameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100;
            cam.aspect = 1; // square view
            cam.enabled = false;
        }

        public void Open(RawImage view)
        {
            Build();
            gameObject.SetActive(true);

            if (renderTexture == null) {
                renderTexture = new RenderTexture(textureSize, textureSize, 16) { name = "MiniGameRT" };
                renderTexture.Create();
            }
            cam.targetTexture = renderTexture;
            cam.aspect = 1;
            cam.enabled = true;
            view.texture = renderTexture;
        }

        // Turns off the camera while the stage is not visible
        public void SetRendering(bool isOn)
        {
            if (cam != null)
                cam.enabled = isOn;
        }

        public void SetBackgroundColor(Color color)
        {
            if (cam != null)
                cam.backgroundColor = color;
        }

        public void ResetBackgroundColor()
        {
            SetBackgroundColor(backgroundColor);
        }

        public void Close()
        {
            Clear();
            if (cam != null) {
                cam.enabled = false;
                cam.targetTexture = null;
            }
            if (renderTexture != null) {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            if (Content == null) return;
            foreach (Transform child in Content)
                Destroy(child.gameObject);
        }

        // Instantiates a character and loads its parts (same flow used by the story avatars)
        public CharacterManager SpawnCharacter(string characterId, Vector3 localPosition)
        {
            CharacterManager character = Instantiate(characterPrefab, Content);
            character.name = "MiniGameCharacter_" + characterId;
            character.transform.localPosition = localPosition;
            character.transform.localEulerAngles = Vector3.zero;
            character.transform.localScale = Vector3.one * characterScale;
            character.gameObject.SetActive(true);
            character.characterID = characterId;
            character.Init();
            Events.LoadBoardItemForStory(character, characterId);

            DisableColliders(character.gameObject);
            return character;
        }

        // Instantiates an object and loads its items (same flow used by the story props).
        // Init() is not called on purpose: it activates the drag & drop UI of the object editor
        public SceneObjectManager SpawnObject(string objectId, Vector3 localPosition)
        {
            SceneObjectManager obj = Instantiate(objectPrefab, Content);
            obj.name = "MiniGameObject_" + objectId;
            obj.transform.localPosition = localPosition;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.gameObject.SetActive(true);
            obj.id = objectId;

            PlayState(obj, "generic");
            Events.LoadBoardItemForStory(obj, objectId);
            SetStatic(obj);
            return obj;
        }

        // Scale the stories give to their background container before loading a background
        const float StoryBgContainerScale = 2.4f;

        // Instantiates a background with the objects placed inside it (same flow used by the story backgrounds).
        // Returns the root: its position is the origin of the background.
        // The loading places the items of the inner objects using world positions, so it only works in the
        // place of the stories: the background is loaded there, in a copy of the story background container,
        // and then moved to the stage (everything inside keeps its layout)
        public Transform SpawnBackground(string backgroundId, Vector3 localPosition, float scale)
        {
            Transform storyContainer = Scenario.Instance != null ? Scenario.Instance.sceneObjectsManager.BgContainer : null;
            Vector3 storyLocalPos = storyContainer != null ? storyContainer.localPosition : new Vector3(0, 19, 0);
            Quaternion storyLocalRot = storyContainer != null ? storyContainer.localRotation : Quaternion.identity;

            // Space of the stories: same world transform as the parent of the story background container
            Transform storySpace = new GameObject("StoryBgSpace").transform;
            if (storyContainer != null && storyContainer.parent != null) {
                Transform storyParent = storyContainer.parent;
                storySpace.SetPositionAndRotation(storyParent.position, storyParent.rotation);
                storySpace.localScale = storyParent.lossyScale;
            }

            // Copy of the story background container (the loading reads its local scale and Y)
            Transform container = new GameObject("StoryBgContainer").transform;
            container.SetParent(storySpace, false);
            container.localPosition = storyLocalPos;
            container.localRotation = storyLocalRot;
            container.localScale = Vector3.one * StoryBgContainerScale;

            SceneObjectManager bg = Instantiate(objectPrefab, container);
            bg.name = "MiniGameBackground_" + backgroundId;
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localEulerAngles = Vector3.zero;
            bg.transform.localScale = objectPrefab.transform.localScale;
            bg.gameObject.SetActive(true);
            bg.id = backgroundId;

            PlayState(bg, "background");
            Events.LoadBgForStory(bg, backgroundId, container, bg.GetContainer());
            SetStatic(bg);

            // Moves it to the stage. The offset cancels the position of the container inside the stories,
            // so the root is the origin of the background
            Transform root = new GameObject("Background_" + backgroundId).transform;
            root.SetParent(Content, false);
            root.localPosition = localPosition;
            root.localScale = Vector3.one * scale;

            Transform offset = new GameObject("StoryOffset").transform;
            offset.SetParent(root, false);
            offset.localPosition = -storyLocalPos;

            Vector3 containerScale = container.localScale; // the loading halves it
            container.SetParent(offset, false);
            container.localPosition = storyLocalPos;
            container.localRotation = Quaternion.identity;
            container.localScale = containerScale;
            Destroy(storySpace.gameObject);

            return root;
        }

        static void PlayState(SceneObjectManager obj, string stateName)
        {
            Animator anim = obj.GetComponent<Animator>();
            int state = Animator.StringToHash(stateName);
            if (anim != null && anim.HasState(0, state)) {
                anim.Play(state);
                anim.Update(0); // applies the pose now, so the bounds are right from the first frame
            }
        }

        // Hides the editor selection and turns off the colliders
        static void SetStatic(SceneObjectManager obj)
        {
            foreach (BodyPart bp in obj.GetComponentsInChildren<BodyPart>(true))
                bp.SetSelection(false);
            DisableColliders(obj.gameObject);
        }

        // Simple colored square, used when there is no asset to show
        public SpriteRenderer CreatePlaceholder(string name, Color color, Vector3 localPosition, Vector3 localScale)
        {
            SpriteRenderer sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(Content, false);
            sr.transform.localPosition = localPosition;
            sr.transform.localScale = localScale;
            sr.sprite = WhiteSprite;
            sr.color = color;
            return sr;
        }

        static Sprite whiteSprite;
        public static Sprite WhiteSprite {
            get {
                if (whiteSprite == null) {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
                }
                return whiteSprite;
            }
        }

        // The minigames handle their own collisions: the editor colliders are not needed
        static void DisableColliders(GameObject go)
        {
            foreach (Collider2D coll in go.GetComponentsInChildren<Collider2D>(true))
                coll.enabled = false;
        }

        void OnDestroy()
        {
            if (renderTexture != null) {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}

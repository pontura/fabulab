using BoardItems;
using System.Collections;
using UnityEngine;

public class ItemPhotoCreator : MonoBehaviour
{
    [SerializeField] Camera _camera;
    System.Action<ItemData, Sprite> OnSpriteDone;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] Transform container;

    public void Add(ItemData itemData, System.Action<ItemData, Sprite> OnSpriteDone)
    {
        this.OnSpriteDone = OnSpriteDone;
        StartCoroutine(CaptureAllPrefabs(itemData));
    }

    IEnumerator CaptureAllPrefabs(ItemData itemData)
    {
        Utils.RemoveAllChildsIn(container);
        GameObject instance = Instantiate(itemData.gameObject, container);
        instance.transform.localPosition = Vector3.zero;
        instance.layer = LayerMask.NameToLayer("PrefabShot");
        SetLayerRecursively(instance.transform, LayerMask.NameToLayer("PrefabShot"));

        yield return new WaitForEndOfFrame(); 

        Texture2D photo = CaptureRenderTexture();
        Sprite sprite = Sprite.Create(photo, new Rect(0, 0, photo.width, photo.height), new Vector2(0.5f, 0.5f));

        OnSpriteDone(itemData, sprite);
      //  Destroy(instance);
    }

    Texture2D CaptureRenderTexture()
    {
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }
}

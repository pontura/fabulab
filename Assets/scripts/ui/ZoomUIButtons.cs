using UnityEngine;

public class ZoomUIButtons : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float speed = 0.1f;
    int zoomType = 0;
    private void Awake()
    {
        Events.DragAndDropActive += OnDragAndDropActive;
    }
    private void OnDestroy()
    {
        Events.DragAndDropActive -= OnDragAndDropActive;
    }
    void OnDragAndDropActive(bool active)
    {
        gameObject.SetActive(active);
    }
    private void Update()
    {
        if (zoomType == 0) return;
        else if (zoomType == 1) cam.orthographicSize += -speed;
        else if (zoomType == -1) cam.orthographicSize += speed;
    }
    public void OnPointerZoomIn(bool isOn)
    {
        if (isOn) zoomType = 1;
        else zoomType = 0;
    }
    public void OnPointerZoomOut(bool isOn)
    {
        if (isOn) zoomType = -1;
        else zoomType = 0;
    }
}

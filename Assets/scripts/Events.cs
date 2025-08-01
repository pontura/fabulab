using UnityEngine;
using BoardItems;

public static class Events
{
    public static System.Action<ItemInScene, Vector3> OnStartDrag = delegate { };
    public static System.Action<ItemInScene, Vector3> OnStopDrag = delegate { };
    public static System.Action<ItemInScene> ItemBackToMenu = delegate { };
    public static System.Action<ItemInScene> OnNewItem = delegate { };
    public static System.Action<GaleriasData.GalleryData> InitGallery = delegate { };
    public static System.Action ResetItems = delegate { };
    public static System.Action<PalettesManager.colorNames> Colorize = delegate { };
    public static System.Action<AnimationsManager.AnimData> AnimateItem = delegate { };
    public static System.Action<bool> ActivateUIButtons = delegate { };
    public static System.Action<bool> OnLoading = delegate { };
    public static System.Action<string, System.Action> CaptureGif = delegate { };
    public static System.Action<bool> OnCanvasDragger = delegate { };
    public static System.Action<System.Action<bool>> OnParentalGate = delegate { };

}
   

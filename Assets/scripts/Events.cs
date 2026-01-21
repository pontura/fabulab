using BoardItems;
using BoardItems.UI;
using UnityEngine;

public static class Events
{
    public static System.Action<BodyPart> OnNewBodyPartSelected = delegate { };
    public static System.Action<ItemInScene, Vector3> OnStartDrag = delegate { };
    public static System.Action<ItemInScene, Vector3> OnStopDrag = delegate { };
    public static System.Action<ItemInScene> ItemBackToMenu = delegate { };
    public static System.Action<ItemInScene> OnNewItem = delegate { };
    public static System.Action<GaleriasData.GalleryData, bool, System.Action> InitGallery = delegate { };
    public static System.Action GalleryDone = delegate { };
    public static System.Action ResetItems = delegate { };
    public static System.Action<PalettesManager.colorNames> Colorize = delegate { };
    public static System.Action<AnimationsManager.AnimData> AnimateItem = delegate { };
    public static System.Action<bool> ActivateUIButtons = delegate { };
    public static System.Action<bool> OnLoading = delegate { };
    public static System.Action<string, System.Action> CaptureGif = delegate { };
    public static System.Action<bool> OnCanvasDragger = delegate { };
    public static System.Action<System.Action<bool>> OnParentalGate = delegate { };
    public static System.Action<int, CharacterAnims.anims> OnCharacterAnim = delegate { };
    public static System.Action<int, CharacterExpressions.expressions> OnCharacterExpression = delegate { };
    public static System.Action<bool> EditMode = delegate { };
    public static System.Action<ZoomsManager.zoomTypes> Zoom = delegate { };

}
   

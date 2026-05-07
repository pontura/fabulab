using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using UI;
using UI.MainApp;
using UnityEngine;

public static class Events
{
    public static System.Action<bool> ShowUndo = delegate { };
    public static System.Action OnUndoAdded = delegate { };
    public static System.Action<bool> OnBGColorizerOpen = delegate { };
    public static System.Action<bool> SetGroupToolsOn = delegate { };
    public static System.Action<BodyPart> OnNewBodyPartSelected = delegate { };
    public static System.Action<ItemInScene, Vector3> OnStartDrag = delegate { };
    public static System.Action<ItemInScene, Vector3> OnStopDrag = delegate { };
    public static System.Action<ItemInScene> ItemBackToMenu = delegate { };
    public static System.Action<GaleriasData.GalleryData, bool, System.Action> InitGallery = delegate { };
    public static System.Action GalleryDone = delegate { };
    public static System.Action ResetItems = delegate { };
    public static System.Action<string, string, string, System.Action<bool>> OnConfirm = delegate { };
    public static System.Action<CharacterPartsHelper.parts> EmptyCharacterItemsButExlude = delegate { };
    public static System.Action EmptySceneItems = delegate { };
    public static System.Action<BoardItemManager,string> LoadBoardItemForStory = delegate { };
    public static System.Action<BoardItemManager, string, Transform, Transform> LoadBgForStory = delegate { };

    public static System.Action<PalettesManager.colorNames> Colorize = delegate { };
    public static System.Action<PalettesManager.colorNames> ColorizeBG = delegate { };
    public static System.Action<string,PalettesManager.colorNames> ColorizeArms = delegate { };
    public static System.Action<string, PalettesManager.colorNames> ColorizeLegs = delegate { };
    public static System.Action<string, PalettesManager.colorNames> ColorizeEyebrows = delegate { };
    public static System.Action<string> ChangeName = delegate { };

    public static System.Action<AnimationsManager.AnimData> AnimateItem = delegate { };
    public static System.Action<string> OnPopupTopSignalText = delegate { };
    public static System.Action<bool> ActivateUIButtons = delegate { };
    public static System.Action<bool> SetChangesMade = delegate { };

    public static System.Action<bool> OnLoading = delegate { };
    public static System.Action<Transform, System.Action> OnLoadingParent = delegate { };

    public static System.Action<string, System.Action> CaptureGif = delegate { };
    public static System.Action<bool> OnCanvasDragger = delegate { };
    public static System.Action<bool> DragAndDropActive = delegate { };
    public static System.Action<System.Action<bool>> OnParentalGate = delegate { };
    public static System.Action<string, string> OnCharacterAnim = delegate { };
    public static System.Action<string> SetCharacterIdle = delegate { };
    public static System.Action<string, CharacterPartsHelper.parts> OnCharacterPartAnim = delegate { };
    public static System.Action<string, string> OnCharacterExpression = delegate { };
    public static System.Action<bool> EditMode = delegate { };
    public static System.Action<CharacterPartsHelper.parts> OnBodyPartActive = delegate { };
    public static System.Action<ZoomStates, bool> Zoom = delegate { };

    public static System.Action OnCharacterReset = delegate { }; // si hay un preset abierto lo resetea:
    public static System.Action OnPresetReset = delegate { }; // si hay un preset abierto lo resetea:
    public static System.Action<string> OnPresetLoaded = delegate { };

    public static System.Action<UIManager.screenType> ShowScreen = delegate { };
    public static System.Action<string> DuplicateCharacter = delegate { };
    public static System.Action<string> DuplicateSO = delegate { };

    public static System.Action OnAllFilmMetadataLoadDone = delegate { };

    public static System.Action<FilmDataFabulab> OnFilmMetadataAdded = delegate { };
    public static System.Action<FilmDataFabulab> OnFilmMetadataUpdated = delegate { };
    public static System.Action<string> OnFilmMetadataRemoved = delegate { };

    public static System.Action<CharacterMetaData> OnCharacterMetadataAdded = delegate { };
    public static System.Action<CharacterMetaData> OnCharacterMetadataUpdated = delegate { };
    public static System.Action<string> OnCharacterMetadataRemoved = delegate { };

    public static System.Action<PropMetaData> OnPropMetadataAdded = delegate { };
    public static System.Action<PropMetaData> OnPropMetadataUpdated = delegate { };
    public static System.Action<PropMetaData> OnPropMetadataRemoved = delegate { };
}
   

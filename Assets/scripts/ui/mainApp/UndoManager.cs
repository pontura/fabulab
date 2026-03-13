using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public List<UndoStep> undoSteps; 
    public CharacterPartsHelper.parts part;

    [Serializable]
    public class UndoStep
    {
        public SOPartData soPartData;
    }
    private void Start()
    { 
        undoSteps = new List<UndoStep>();
    }
    public void OnNewStep()
    {
        if (part != UIManager.Instance.part)
        {
            part = UIManager.Instance.part;
            undoSteps.Clear();
        }
        UndoStep undoStep = new UndoStep { };
        SOPartData sPartData = SaveStep();
        undoStep.soPartData = sPartData;
        undoSteps.Add( undoStep );
        Events.OnUndoAdded();
    }
    public void Reset()
    {
        undoSteps = new List<UndoStep>();
    }
    public void OnClicked()
    {
        Data.Instance.charactersData.OnPresetReset();
        if (undoSteps.Count > 0)
        {
            UndoStep undoStep = undoSteps[undoSteps.Count - 1];
            UIManager.Instance.boardUI.items.DeleteInPart((int)part);
            UIManager.Instance.boardUI.items.OpenWork(UIManager.Instance.boardUI.activeBoardItem, undoStep.soPartData, false);
            Invoke("ActvePart", 0.1f);
            undoStep.Equals(null);
            undoSteps.Remove(undoStep);
        }
    }
    void ActvePart()
    {
        UIManager.Instance.boardUI.items.SetColliders(UIManager.Instance.boardUI.activeBoardItem, true);
    }
    SOPartData SaveStep()
    {
        SOPartData wd  = new SOPartData();
    
        wd.items = new List<SavedIData>();
        foreach (ItemInScene iInScene in UIManager.Instance.boardUI.items.all)
        {
            if (iInScene.data.part == part)
            {
                int newPartID = (int)iInScene.data.part;

                SavedIData sd = new SavedIData();
                sd.part = newPartID;
                sd.id = iInScene.data.id;
                sd.position = iInScene.data.position;
                sd.rotation = iInScene.data.rotation;
                sd.scale = iInScene.data.scale;
                sd.anim = iInScene.data.anim;
                sd.color = iInScene.data.colorName;
                sd.galleryID = iInScene.data.galleryID;
                wd.items.Add(sd);
            }
        }
        return wd;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

public class SOButtons : MonoBehaviour {

    public GameObject container;
    public GameObject customizer;
    public GameObject edit;
    public GameObject actions;
    public GameObject expresions;

    public float buttonsTimeout = 2f;

    void Start() {
        container.SetActive(false);
        StoryMakerEvents.ShowSoButtons += ShowSoButtons;
        StoryMakerEvents.HideSoButtons += HideSoButtons;
    }

    void OnDestroy() {
        StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
        StoryMakerEvents.HideSoButtons -= HideSoButtons;
    }

    void ShowSoButtons(Vector3 pos, SOData data) {
        CancelInvoke();
        transform.position = pos;

        edit.SetActive(false);
        customizer.SetActive(false);
        actions.SetActive(false);
        expresions.SetActive(false);

        if (data is SOAvatarData) {
            customizer.SetActive(true);
            actions.SetActive(true);
            expresions.SetActive(true);
        } else  if (data is SOInputData)
            edit.SetActive(true);

        container.SetActive(true);
        Invoke("Reset", buttonsTimeout);
    }

    void HideSoButtons() {
        container.SetActive(false);
    }

    void Reset()
    {
        container.SetActive(false);
    }

    public void Remove() {
        StoryMakerEvents.RemoveSceneObject();
    }

    public void EditCustomize() {
        StoryMakerEvents.EditCustomize();
        HideSoButtons();
        //Scenario.Instance.sceneObejctsManager.selected.StopDrag();
    }

    public void EditActions() {
        StoryMakerEvents.EditActions();
        HideSoButtons();
        //Scenario.Instance.sceneObejctsManager.selected.StopDrag();
    }
    public void EditExpressions()
    {
        StoryMakerEvents.EditExpressions();
        HideSoButtons();
        //Scenario.Instance.sceneObejctsManager.selected.StopDrag();
    }

    public void EditInput()
    {
        StoryMakerEvents.OnInputField((ObjectSignal)Scenario.Instance.sceneObejctsManager.selected);
    }
}

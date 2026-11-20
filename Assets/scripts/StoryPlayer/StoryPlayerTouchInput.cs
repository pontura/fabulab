using UnityEngine;

namespace StoryPlayer
{
    // Touch (and mouse) controls of the story player:
    //  - tap anywhere: pauses the story, another tap continues it
    //  - tap and hold on the right half: shows the next keyframe every stepInterval, until the last one
    //  - tap and hold on the left half: the same backwards, until the first one
    // When the hold ends, the story plays again if it was playing before
    [RequireComponent(typeof(StoryPlayerController))]
    public class StoryPlayerTouchInput : MonoBehaviour
    {
        [Tooltip("Seconds pressed before a press counts as a hold instead of a tap")]
        [SerializeField] float holdDelay = 0.3f;
        [Tooltip("Seconds between keyframes while holding")]
        [SerializeField] float stepInterval = 0.2f;

        StoryPlayerController controller;
        bool pressing;
        bool holding;
        float pressTime;
        float nextStepTime;
        int direction;

        void Awake() {
            controller = GetComponent<StoryPlayerController>();
        }

        void OnDisable() {
            Release(cancel: true);
        }

        void Update() {
            // The mouse also gets the first touch (Input.simulateMouseWithTouches)
            if (Input.GetMouseButtonDown(0))
                Press(Input.mousePosition);

            if (!pressing)
                return;

            if (!Input.GetMouseButton(0)) {
                Release(cancel: false);
                return;
            }

            if (!holding && Time.unscaledTime - pressTime >= holdDelay)
                BeginHold();

            if (holding && Time.unscaledTime >= nextStepTime) {
                controller.StepKeyframe(direction);
                nextStepTime = Time.unscaledTime + stepInterval;
            }
        }

        void Press(Vector3 screenPosition) {
            if (!controller.IsLoaded)
                return;
            pressing = true;
            holding = false;
            pressTime = Time.unscaledTime;
            direction = screenPosition.x >= Screen.width * 0.5f ? 1 : -1;
        }

        void BeginHold() {
            holding = true;
            controller.BeginScrub();
            nextStepTime = Time.unscaledTime;
        }

        void Release(bool cancel) {
            if (!pressing)
                return;
            pressing = false;
            if (holding) {
                holding = false;
                controller.EndScrub();
            } else if (!cancel) {
                controller.TogglePause();
            }
        }
    }
}

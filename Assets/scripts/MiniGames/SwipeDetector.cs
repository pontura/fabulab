using System;
using UnityEngine;

namespace MiniGames
{
    public enum SwipeDirection { None, Up, Down, Left, Right }

    // Detects swipes with the legacy Input Manager (mouse input also covers touch on mobile).
    // The swipe fires while dragging, as soon as it passes the threshold, so it feels responsive.
    // Arrow keys also work, to test in the Editor
    [Serializable]
    public class SwipeDetector
    {
        [Tooltip("Minimum drag distance, as a fraction of the screen height")]
        [SerializeField] float threshold = 0.05f;

        Vector2 startPos;
        bool dragging;
        bool fired;

        public SwipeDirection Tick()
        {
            SwipeDirection keyDirection = GetKeyDirection();
            if (keyDirection != SwipeDirection.None)
                return keyDirection;

            if (Input.GetMouseButtonDown(0)) {
                startPos = Input.mousePosition;
                dragging = true;
                fired = false;
            }
            if (Input.GetMouseButtonUp(0))
                dragging = false;

            if (!dragging || fired)
                return SwipeDirection.None;

            Vector2 delta = (Vector2)Input.mousePosition - startPos;
            if (delta.magnitude < threshold * Screen.height)
                return SwipeDirection.None;

            fired = true; // one swipe per drag
            if (Mathf.Abs(delta.y) >= Mathf.Abs(delta.x))
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }

        public void Reset()
        {
            dragging = false;
            fired = false;
        }

        SwipeDirection GetKeyDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) return SwipeDirection.Up;
            if (Input.GetKeyDown(KeyCode.DownArrow)) return SwipeDirection.Down;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) return SwipeDirection.Left;
            if (Input.GetKeyDown(KeyCode.RightArrow)) return SwipeDirection.Right;
            return SwipeDirection.None;
        }
    }
}

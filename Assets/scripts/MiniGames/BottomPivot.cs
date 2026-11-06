using UnityEngine;

namespace MiniGames
{
    // Puts an object inside a parent used as pivot, at the lowest point of its sprites,
    // so placing the pivot on a line leaves the object standing on it
    public static class BottomPivot
    {
        public static Transform Wrap(Transform content, string name)
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(content.parent, false);
            content.SetParent(pivot, false);
            content.localPosition = Vector3.zero;
            return pivot;
        }

        // Moves the content so its lowest point is on the pivot. Call it once the object shows its
        // final pose (ex: one frame after activating it, so the animator has been applied)
        public static void Align(Transform pivot, Transform content)
        {
            bool found = false;
            float minY = float.MaxValue;
            foreach (SpriteRenderer sr in content.GetComponentsInChildren<SpriteRenderer>()) {
                if (!sr.enabled || sr.sprite == null) continue;
                minY = Mathf.Min(minY, sr.bounds.min.y);
                found = true;
            }
            if (!found)
                return;

            float bottom = pivot.InverseTransformPoint(new Vector3(pivot.position.x, minY, pivot.position.z)).y;
            Vector3 pos = content.localPosition;
            pos.y -= bottom;
            content.localPosition = pos;
        }
    }
}

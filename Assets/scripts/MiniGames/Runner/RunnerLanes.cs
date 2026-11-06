using System;
using UnityEngine;

namespace MiniGames.Runner
{
    // Layout of the runner lanes. Lane 0 is the bottom one (closest to the camera):
    // lower lanes are drawn bigger and in front, to give a sense of depth
    [Serializable]
    public class RunnerLanes
    {
        [SerializeField] int count = 4;
        [SerializeField] float bottomY = -3.5f;
        [SerializeField] float topY = 1.5f;
        [Tooltip("Scale of the character in the bottom lane")]
        [SerializeField] float nearScale = 1f;
        [Tooltip("Scale of the character in the top lane")]
        [SerializeField] float farScale = 0.8f;
        [SerializeField] int baseSortingOrder = 100;
        [SerializeField] int sortingOrderStep = 10;

        public int Count => count;

        public int Clamp(int lane)
        {
            return Mathf.Clamp(lane, 0, count - 1);
        }

        float Normalized(int lane)
        {
            return count > 1 ? (float)lane / (count - 1) : 0;
        }

        public float GetY(int lane)
        {
            return Mathf.Lerp(bottomY, topY, Normalized(lane));
        }

        public float GetScale(int lane)
        {
            return Mathf.Lerp(nearScale, farScale, Normalized(lane));
        }

        // Lanes further away are drawn behind
        public int GetSortingOrder(int lane)
        {
            return baseSortingOrder + (count - 1 - lane) * sortingOrderStep;
        }

        public float LaneHeight => count > 1 ? (topY - bottomY) / (count - 1) : 1;
    }
}

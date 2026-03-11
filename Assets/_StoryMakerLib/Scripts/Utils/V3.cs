using System;
using UnityEngine;

[Serializable]
public struct V3
{

    public float x;
    public float y;
    public float z;

    public V3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public V3(Vector3 v) {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }

    public override bool Equals(object obj) {
        if (obj is V3 other) {
            return other.x == x
                && other.y == y
                && other.z == z;
        }
        return false;
    }
}

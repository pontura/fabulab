using System;
using UnityEngine;

[Serializable]
public class SOData
{
    public string itemName;
    public Vector3 pos;
    public bool goLeft;
    public float size, rot;
    public int id;
    public string customization;

    public override bool Equals(object obj) {
        if (obj is SOData other) {
            return string.Equals(this.itemName, other.itemName)
                && this.pos.Equals(other.pos)
                && this.goLeft == other.goLeft
                && this.size == other.size
                && this.rot == other.rot
                && this.id == other.id
                && string.Equals(this.customization, other.customization);
        }
        return false;
    }

    public override int GetHashCode() {
        int hash = 17;
        hash = hash * 31 + (itemName?.GetHashCode() ?? 0);
        hash = hash * 31 + pos.GetHashCode();
        hash = hash * 31 + goLeft.GetHashCode();
        hash = hash * 31 + size.GetHashCode();
        hash = hash * 31 + rot.GetHashCode();
        hash = hash * 31 + id.GetHashCode();
        hash = hash * 31 + (customization?.GetHashCode() ?? 0);
        return hash;
    }

}

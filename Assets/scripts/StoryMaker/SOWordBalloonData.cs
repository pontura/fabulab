using System;

[Serializable]
public class SOWordBalloonData : SOData
{
    public string inputValue;
    public int direction;
    public int fontId;

    public override bool Equals(object obj) {
        if (obj is SOWordBalloonData other) {
            return string.Equals(this.itemName, other.itemName)
                && this.pos.Equals(other.pos)
                && this.goLeft == other.goLeft
                && this.size == other.size
                && this.rot == other.rot
                && this.id == other.id
                && string.Equals(this.customization, other.customization)
                && string.Equals(this.inputValue, other.inputValue);
        }
        return false;
    }

    public override int GetHashCode() {
        int hash = base.GetHashCode();
        hash = hash * 31 + (inputValue?.GetHashCode() ?? 0);
        hash = hash * 31 + direction.GetHashCode();
        hash = hash * 31 + fontId.GetHashCode();
        return hash;
    }
}

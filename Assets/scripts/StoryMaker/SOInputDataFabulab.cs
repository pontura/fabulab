using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Yaguar.StoryMaker.Editor;

[Serializable]
public class SOInputDataFabulab : SOInputData
{
    public int direction;
    public int fontId;

    public override bool Equals(object obj) {        
        if (obj is SOWordBalloonData other) {
            Debug.Log(this.inputValue + " == " + other.inputValue);
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
        int hash = 17;
        hash = hash * 31 + (itemName?.GetHashCode() ?? 0);
        hash = hash * 31 + pos.GetHashCode();
        hash = hash * 31 + goLeft.GetHashCode();
        hash = hash * 31 + size.GetHashCode();
        hash = hash * 31 + rot.GetHashCode();
        hash = hash * 31 + id.GetHashCode();
        hash = hash * 31 + (customization?.GetHashCode() ?? 0);
        hash = hash * 31 + (inputValue?.GetHashCode() ?? 0);
        hash = hash * 31 + (direction.ToString()?.GetHashCode() ?? 0);
        hash = hash * 31 + (fontId.ToString()?.GetHashCode() ?? 0);
        return hash;
    }
}

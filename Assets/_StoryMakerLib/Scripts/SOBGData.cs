using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class SOBGData : SOData
{
    public Sprite icon;
    // public CataloguesData.ItemData itemData; ##

    public void SetIcon(){
        /* ##
        if (itemData.bundleName != "") {
            AssetBundle bundle = Data.Instance.bundlesManager.GetBundle(itemData.bundleName);
            string url = "assets/update_assets/" + itemData.bundleName + "/" + itemName + ".png";
            Debug.Log(url);
            icon = bundle.LoadAsset<Sprite>(url);
            Debug.Log(icon.name);
        } else
            icon = Resources.Load<Sprite>(itemName) as Sprite;

        ## */
    }
}

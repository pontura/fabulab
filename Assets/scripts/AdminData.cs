using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminData : MonoBehaviour
{
    public List<string> adminIds;

    // Start is called before the first frame update
    void Start() {
    }
    public bool IsAdmin(string id) {
        return adminIds.Contains(id);
    }
}

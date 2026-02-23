using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

public class SpeedButton : MonoBehaviour
{
    public GameObject signal;
    public int speed;
    private void Start()
    {
        StoryMakerEvents.ChangeSpeed += ChangeSpeed;
    }
    private void OnDestroy()
    {
        StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
    }
    public void Clicked(int qty)
    {
        speed += qty;
        if (speed > 2)
            speed = -2;
        if (speed < -2)
            speed = 2;
        StoryMakerEvents.ChangeSpeed(speed);
    }
    void ChangeSpeed(int speed)
    {
        this.speed = speed;
        signal.transform.localEulerAngles = new Vector3(0, 0, -speed*30);
    }
}

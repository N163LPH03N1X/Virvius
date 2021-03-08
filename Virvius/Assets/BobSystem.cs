using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobSystem : MonoBehaviour
{
    public PlayerSystem playerSystem;
    public Vector3[] bobVectors = new Vector3[3];
    Vector3 lastPosition;
    public float bobSpeed;
    public float MovementSpeedY = 3;
    public float AboveOrBelowZeroY = 0;
    public float UpAndDownAmount = 5;
    int bobIndex = 0;
    bool switchDir = false;
    bool idle = true;
    Vector3 newPosition;
    void Start()
    {
        lastPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerSystem.inputX != 0 && playerSystem.inputY != 0 && !playerSystem.isJumping && !playerSystem.isFalling)
        {
            if (idle) idle = false;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, bobVectors[bobIndex], Time.deltaTime * bobSpeed);
            if (transform.localPosition == bobVectors[bobIndex])
            {
                if (!switchDir)
                {
                    bobIndex++;
                    if (bobIndex > 2) { bobIndex = 1; switchDir = true; }
                }
                else
                {
                    bobIndex--;
                    if (bobIndex < 0) { bobIndex = 1; switchDir = false; }
                }
            }
        }
        else
        {
            if (bobIndex != 0) { bobIndex = 0; switchDir = false; }
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, lastPosition, Time.deltaTime * bobSpeed);
            if(transform.localPosition == lastPosition)
            {
                if (!idle) idle = true;
            }
            if (idle)
            {
                newPosition.z = lastPosition.z;
                newPosition.x = lastPosition.x;
                newPosition.y = lastPosition.y + ((Mathf.Sin(Time.time * MovementSpeedY) + AboveOrBelowZeroY) / UpAndDownAmount);
                transform.localPosition = newPosition;
            }
        }
    }
}

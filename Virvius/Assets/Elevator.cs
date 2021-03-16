using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    private float maxY;
    [SerializeField]
    private float minY;
    [SerializeField]
    private float moveSpeed;
    public bool activated = false;
    private Vector3 position = Vector3.zero;
    private Transform elevator;
    void Start()
    {
        elevator = transform.GetChild(0);
        position.y = minY;
        elevator.localPosition = position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        position.y = activated ? maxY : minY;
        float distance = Mathf.Clamp(position.y, minY, maxY);
        Vector3 SetPos = new Vector3(0, distance, 0);
        elevator.localPosition = Vector3.MoveTowards(elevator.localPosition, SetPos, Time.deltaTime * moveSpeed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorSwitch : MonoBehaviour
{
    public bool isLower = true;
    public Elevator elevator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            elevator.activated = !isLower;
        }
    }
}

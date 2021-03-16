using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public Elevator elevator;
    public Transform playerHolder;
    private Transform holderReturn;
    private void Start()
    {
        holderReturn = GameObject.Find("GameSystem/Game").transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            elevator.activated = !elevator.activated;
            other.gameObject.transform.SetParent(playerHolder);
            other.gameObject.transform.parent = transform;
            InputSystem inputSystem = other.gameObject.GetComponent<InputSystem>();
            inputSystem.removeGravity = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.SetParent(holderReturn);
            other.gameObject.transform.parent = holderReturn;
            InputSystem inputSystem = other.gameObject.GetComponent<InputSystem>();
            inputSystem.removeGravity = false;
        }
    }
}

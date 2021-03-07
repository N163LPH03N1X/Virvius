using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : MonoBehaviour
{
    OptSystem optSystem = new OptSystem();
    [Header("Player Movement")]
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float strafeSpeed = 5f;
    [SerializeField]
    private float lookSpeed = 10f;
    [SerializeField]
    private float jumpSpeed = 10f;
    [SerializeField]
    private float antiJumpFactor;
    [SerializeField]
    private float gravity;
    [Space]
    private float health;
    private int jumpTimer;
    Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private float inputX;
    private float inputY;
    private bool isGrounded = true;
    private bool isJumping = true;
    

    void Start()
    {
        controller = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {
        Move(moveSpeed);
        Look(lookSpeed);
    }
    public void Move(float speed)
    {
        inputX = optSystem.Input.GetAxis("LSH");
        inputY = optSystem.Input.GetAxis("LSV");
        moveDirection = new Vector3(inputX, 0, inputY);
        moveDirection = transform.TransformDirection(moveDirection) * moveSpeed;
        if (!optSystem.Input.GetButton("A"))
        {
            jumpTimer++;
        }
        else if (jumpTimer >= antiJumpFactor && isGrounded)
        {
            isJumping = true;
            moveDirection.y += jumpSpeed;
            jumpTimer = 0;
        }
        moveDirection.y -= gravity * Time.unscaledDeltaTime;
        isGrounded = (controller.Move(moveDirection * Time.unscaledDeltaTime) & CollisionFlags.Below) != 0;
       
    }
    public void Damage(int amount)
    {
        health -= amount;
    }
    public void Look(float speed)
    {

    }
}

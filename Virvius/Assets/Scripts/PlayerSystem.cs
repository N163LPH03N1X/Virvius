using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerSystem : MonoBehaviour
{

    private Player inputPlayer;
    private Transform head;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 contactPoint;
    private RaycastHit hit;
    private enum RotationAxes { XY, X, Y };
    private RotationAxes axis = RotationAxes.XY;
    public float moveSpeed = 10;
    public float jumpSpeed = 20f;
    public float antiBumpFactor = .75f;
    public  float gravityPull = 50;
    private float gravity;
    public float inputX;
    public float inputY;
    private float health = 100;
    private int antiJumpFactor = 1;
    private int jumpTimer;
    private float fallingDamageThreshold = 10.0f;
    private float fallStartLevel;
    private bool fallDamage = false;
    private float sensitivity = 5f;
    private bool smoothRotation = false;
    private bool invertY = false;
    private float ClampY = 55f;
    private float[] lookRotation = new float[2];

    [Header("Player can move in air")]
    public bool airControl = false;
    [Header("Limit slope speed")]
    public bool limitDiagonalSpeed = true;
    [Header("Player Slide Angle")]
    public float slideAngle;
    [Header("Player Slide Speed")]
    public float slideSpeed = 12.0f;
    [Header("Player Slides on angle")]
    public bool slideOnAngle = false;
    [Header("Player Slides on Tag (Slide)")]
    public bool slideOnTag = false;
    [Header("Player Moving attributes")]
    public bool isGrounded = false;
    public bool isJumping = true;
    public bool isFalling = false;
    public bool isSliding = false;
    public bool isMoving = false;
    [Header("Player Sounds")]
    public AudioClip jumpSfx;

    private void Awake()
    {
      
    }
    private void Start()
    {
        // get the player input system from rewired
        inputPlayer = ReInput.players.GetPlayer(0);
        // grab the head gameObject [for look rotation]
        head = transform.GetChild(0);
        // grab character controller component
        controller = GetComponent<CharacterController>();
        // set the jump timer
        jumpTimer = antiJumpFactor;
        // set the gravity
        gravity = gravityPull;
    }
    private void Update()
    {
        Move();
        Look();
    }

    // Player Movement =========================
    private void Move()
    {
      
        // player input of left stick or Arrow Keys
        inputX = inputPlayer.GetAxis("LSH");
        inputY = inputPlayer.GetAxis("LSV");
       
        // if no player input and angle limit true, slow down input factor [For player air control when falling]
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;

        if (isGrounded)
        {
            // [PLAYER SLIDING] -----------------------------------------------------------------------------
            isSliding = false;
            // when player transform collides with slide angle, sliding = true
            if (Physics.Raycast(transform.position, -Vector3.up, out hit))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > slideAngle)
                    isSliding = true;
                else if (Vector3.Angle(hit.normal, Vector3.up) <= slideAngle)
                    isMoving = true;
            }
            // when player collision contact point collides with slide angle, sliding = true
            else
            {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
                if (Vector3.Angle(hit.normal, Vector3.up) > slideAngle)
                    isSliding = true;
                else if (Vector3.Angle(hit.normal, Vector3.up) <= slideAngle)
                    isMoving = true;
            }
            // start sliding the player based on angle or tag in direction of the angle
            if ((isSliding && slideOnAngle) || (isSliding && slideOnTag && hit.collider.tag == "Slide"))
            {
                Vector3 hitNormal = hit.normal;
                moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                moveDirection *= slideSpeed;
            }
            // Start moving the player based on player input
            else
            {
                moveDirection = new Vector3(inputX, -antiBumpFactor, inputY);
                if (moveDirection.x != 0 || moveDirection.z != 0)
                    isMoving = true;
                else if (moveDirection.x != 0 && moveDirection.z != 0)
                    isMoving = true;
                else isMoving = false;
                moveDirection = transform.TransformDirection(moveDirection) * moveSpeed;
            }
            // [PLAYER FALLING] -------------------------------------------------------------------------------------
            fallDamage = false;
            if (isFalling)
            {
                isFalling = false;
                // set the player falling damage threshold when grounded
                if (transform.position.y < fallStartLevel - fallingDamageThreshold)
                    FallingDamageAlert(fallStartLevel - transform.position.y);
            }
            // [PLAYER JUMPING] -------------------------------------------------------------------------------------
            if (!inputPlayer.GetButton("A") && !isSliding)
            {
                // add index to timer value to increase over anti Jump Factor
                jumpTimer++;
            }
            else if (jumpTimer >= antiJumpFactor)
            {
                // player is now jumping
                isJumping = true;
                // jump upwards
                moveDirection.y = jumpSpeed;
                // reset jump timer so plasyer doesnt continue to jump
                jumpTimer = 0;
                AudioSystem.PlayAudioSource(jumpSfx, 0.7f, 1);
            }
        }
        else
        {
            if (!isFalling)
            {
                //start falling if not grounded
                isFalling = true;
                // reset the gravity 
                gravity = gravityPull;
                // set last grounded position
                fallStartLevel = transform.position.y;
            }
            // Move the player in the air based on control
            if (airControl)
            {
                // Move horizontal
                moveDirection.x = inputX * moveSpeed * inputModifyFactor;
                // Move Forward/back
                moveDirection.z = inputY * moveSpeed * inputModifyFactor;
                // set current movement
                moveDirection = transform.TransformDirection(moveDirection);
            }
        }
        // Always force the player downwards
        moveDirection.y -= gravity * Time.unscaledDeltaTime;
        // Set the isGrounded collision flags if player has landed 
        isGrounded = (controller.Move(moveDirection * Time.unscaledDeltaTime) & CollisionFlags.Below) != 0;

    }
    private void Look()
    {
        // activate rotation smoothing value
        float smoothing = smoothRotation ? Time.deltaTime : 1;
        // switch the look rotation
        switch (axis)
        {
            // XY rotation
            case RotationAxes.XY: XLook(smoothing); YLook(smoothing); break;
            // X rotation
            case RotationAxes.X: XLook(smoothing); break;
            // Y rootation
            case RotationAxes.Y: YLook(smoothing); break;
        }
    }
    private void XLook(float smoothing)
    { // rotation input times smoothing & sensitivity
        lookRotation[0] = inputPlayer.GetAxis("RSH") * smoothing * sensitivity;
        // rotate only player transform
        transform.Rotate(0, lookRotation[0], 0);
    }
    private void YLook(float smoothing)
    {
        // rotation input times smoothing, sensitivity and inversion
        lookRotation[1] += inputPlayer.GetAxis("RSV") * smoothing * sensitivity * (invertY ? -1 : 1);
        // clamp rotation of the Y between 55/-55
        lookRotation[1] = Mathf.Clamp(lookRotation[1], -ClampY, ClampY);
        // rotate the head up or down
        head.localEulerAngles = new Vector3(-lookRotation[1], 0, 0);
    }
    // Player Collision ========================
    public void Damage(int amount)
    {
        // reduce the players health by amount
        health -= amount;
    }
    private void FallingDamageAlert(float fallDistance)
    {
        fallDamage = true;
        if (fallDistance > 55) Debug.Log("Distance: " + fallDistance + "Kill Player");
        else if (fallDistance > 35 && fallDistance <= 55) Debug.Log("Distance: " + fallDistance + "Massive Damage");
        else if (fallDistance > 25 && fallDistance <= 35) Debug.Log("Distance: " + fallDistance + "Less Damage");
        else if (fallDistance <= 25) Debug.Log("Distance: " + fallDistance + "No Damage");
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isJumping = false;
        if (!fallDamage)
        {
            //if ( !isGrounded || isJumping)
            // play landing sound
        }
        contactPoint = hit.point;
    }
    public void ClearOutRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, UnityEngine.Color.clear);
        RenderTexture.active = rt;
    }
    public void GameMouseActive(bool active, CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
        Cursor.visible = active;
    }
}

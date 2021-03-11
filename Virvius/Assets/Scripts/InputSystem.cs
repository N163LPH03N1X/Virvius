using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
public class InputSystem : MonoBehaviour
{
    public static InputSystem inputSystem;
    private EnvironmentSystem environmentSystem;
    private PlayerSystem playerSystem;
    [HideInInspector]
    public Player inputPlayer;
    private CharacterController controller;
    private Transform head;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    private enum RotationAxes { XY, X, Y };
    private RotationAxes axis = RotationAxes.XY;
    private Vector3 contactPoint;
    private RaycastHit playerhit;
    private RaycastHit wallHit;
    // Player Move & Look
    private float moveSpeed = 50;
    private float strafeSpeed = 50;
    [HideInInspector]
    public float inputX;
    [HideInInspector]
    public float inputY;
    private float sensitivity = 5f;
    [SerializeField]
    private bool smoothRotation = false;
    private bool invertY = false;
    private float clampLookAngle = 45f;
    private float[] lookRotation = new float[2];
    // Player Jumping
    private float jumpSpeed = 40f;
    private int antiJumpFactor = 1;
    private int jumpTimer;
    private float antiBumpFactor = .75f;
    // Player Gravity
    public float gravityPull = 80;
    [HideInInspector]
    public float gravity;
    private bool airControl = true;
    private bool limitDiagonalSpeed = false;
    // Player Sliding
    private float slideAngle = 35f;
    private float slideSpeed = 12.0f;
    private bool slideOnAngle = true;
    private bool slideOnTag = false;
    // Player Tilting
    private float tiltAngle;
    private float angle = 2f;
    private float tiltSpeed = 1.5f;
    // Player Falling
    private float fallingDamageThreshold = 10.0f;
    private float fallStartLevel;
    private bool fallDamage = false;
    // Player Attributes
    public bool isGrounded = false;
    public bool isJumping = true;
    public bool isLedge = false;
    public bool isSliding = false;
    public bool isMoving = false;
    public bool isFalling = false;
    public bool isSwimming = false;

    // Player Sound
    public AudioClip playerJSound;
    public AudioClip[] playerSWSound = new AudioClip[2];

    private float swimTime = 0.4f;
    private float swimTimer;
    private void Awake()
    {
        inputSystem = this;
    }
    void Start()
    {
        environmentSystem = EnvironmentSystem.environmentSystem;
        playerSystem = PlayerSystem.playerSystem;
        // Grabe the head object transform for look
        head = transform.GetChild(0);
        // get the player input system from rewired
        inputPlayer = ReInput.players.GetPlayer(0);
        // grab character controller component
        controller = GetComponent<CharacterController>();
        // set the jump timer
        jumpTimer = antiJumpFactor;
        // set the gravity
        gravity = gravityPull;
        swimTimer = swimTime;
    }
    void Update()
    {
        Look();
        if (playerSystem.isDead)
            return;
        Move();

       
    }
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
            if (Physics.Raycast(transform.position, -Vector3.up, out playerhit))
            {
                if (Vector3.Angle(playerhit.normal, Vector3.up) > slideAngle)
                    isSliding = true;
                else if (Vector3.Angle(playerhit.normal, Vector3.up) <= slideAngle)
                    isMoving = true;
            }
            // when player collision contact point collides with slide angle, sliding = true
            else
            {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out playerhit);
                if (Vector3.Angle(playerhit.normal, Vector3.up) > slideAngle)
                    isSliding = true;
                else if (Vector3.Angle(playerhit.normal, Vector3.up) <= slideAngle)
                    isMoving = true;
            }
            // start sliding the player based on angle or tag in direction of the angle
            if ((isSliding && slideOnAngle) || (isSliding && slideOnTag && playerhit.collider.tag == "Slide"))
            {
                Vector3 hitNormal = playerhit.normal;
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
            if (!inputPlayer.GetButton("A") && !isSliding) jumpTimer++;
            else if (inputPlayer.GetButton("A") && jumpTimer >= antiJumpFactor)
            {
                isJumping = true;
                moveDirection.y = jumpSpeed;
                jumpTimer = 0;
                AudioSystem.PlayAudioSource(playerJSound, 0.7f, 1);
            }
            if (inputX > 0.2f) tiltAngle = -angle;
            else if (inputX < -0.2f) tiltAngle = angle;
            else tiltAngle = 0;
            Vector3 headRot = new Vector3(0, 0, tiltAngle);
            Quaternion rot = Quaternion.Euler(headRot);
            // Tilt Camera angle to angle specified
            if(inputX != 0)
                head.GetChild(0).localRotation = Quaternion.RotateTowards(head.GetChild(0).localRotation, rot, Time.deltaTime * (tiltSpeed * 10));
            // Return Camera rotation back to 0 (Caution**  Might interfere with other Camera rotations)
            else head.GetChild(0).localRotation = Quaternion.RotateTowards(head.GetChild(0).localRotation, Quaternion.identity, Time.deltaTime * (tiltSpeed * 10));
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
                moveDirection.x = inputX * strafeSpeed * inputModifyFactor;
                // Move Forward/back
                moveDirection.z = inputY * moveSpeed * inputModifyFactor;
                // set current movement
                moveDirection = transform.TransformDirection(moveDirection);
            }
            
        }
        if (isSwimming)
        {
            // shut everything off.
            isGrounded = false;
            isFalling = false;
            isJumping = false;
            // set the jump timer
            if (!inputPlayer.GetButton("A") && !isSliding) jumpTimer++;
            // keep jumping up to swim
            else if (inputPlayer.GetButton("A"))
            {
                moveDirection.y = 12;
                jumpTimer = 0;
                swimTimer -= Time.deltaTime;
                swimTimer = Mathf.Clamp(swimTimer, 0, swimTime);
                if (swimTimer == 0)
                {
                    AudioSystem.PlayAudioSource(playerSWSound[0], 1f, 1);
                    swimTimer = swimTime;
                }
            }
            moveDirection.y -= 15 * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime * 0.5f);
        }
        else
        {
            // Only look for ledge if player is close to wall
            if (Physics.Raycast(transform.position, transform.forward, out playerhit, 5))
            {
                // Set the position of the second raycast to the head
                Vector3 aboveLedge = head.position + transform.forward * 2f;
                // Add hieght adjustment a bit above the head
                aboveLedge.y += 5;
                // Draw second ray in the editor from the head
                Debug.DrawRay(aboveLedge, transform.forward * 10, Color.yellow);
                // Check if second raycast collides with a ledge or not
                if (Physics.Raycast(aboveLedge, transform.forward, out wallHit, 10) && isLedge)
                {
                   //Dont Jump up if something is there
                }
                else if(!Physics.Raycast(aboveLedge, transform.forward, out wallHit, 10) && isLedge)
                {
                    // Time to jump up nothing is there
                    moveDirection.y = 40;
                    controller.Move(moveDirection * Time.deltaTime);
                    isLedge = false;
                    environmentSystem.SetEnvironment(0, 0);

                    AudioSystem.PlayAudioSource(playerSWSound[1], 1f, 1);
                }
            }
            // Always force the player downwards
            moveDirection.y -= gravity * Time.deltaTime;
            // Set the isGrounded collision flags if player has landed 
            isGrounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }
        Debug.DrawRay(transform.position, transform.forward * 10, Color.green);

    }
    private void Look()
    {
        // activate rotation smoothing value
        float smoothing = smoothRotation ? Time.deltaTime : 1;
        // switch the look rotation
        switch (axis)
        {
            // XY rotation
            case RotationAxes.XY: XLook(1); YLook(1); break;
            // X rotation
            case RotationAxes.X: XLook(1); break;
            // Y rootation
            case RotationAxes.Y: YLook(1); break;
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
        lookRotation[1] = inputPlayer.GetAxis("RSV") * smoothing * sensitivity * (invertY ? -1 : 1);
        // clamp rotation of the Y between 55/-55
        lookRotation[1] = Mathf.Clamp(lookRotation[1], -clampLookAngle, clampLookAngle);
        // rotate the head up or down
        head.transform.Rotate(-lookRotation[1], 0, 0);
    }
    private void FallingDamageAlert(float fallDistance)
    {
        fallDamage = true;
        if (fallDistance > 55) Debug.Log("Distance: " + fallDistance + "Kill Player");
        else if (fallDistance > 35 && fallDistance <= 55) Debug.Log("Distance: " + fallDistance + "Massive Damage");
        else if (fallDistance > 25 && fallDistance <= 35) Debug.Log("Distance: " + fallDistance + "Less Damage");
        else if (fallDistance <= 25) Debug.Log("Distance: " + fallDistance + "No Damage");
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isJumping = false;
        if (!fallDamage)
        {
            //if ( !isGrounded || isJumping)
            // play landing sound
        }
        contactPoint = hit.point;
    }
}

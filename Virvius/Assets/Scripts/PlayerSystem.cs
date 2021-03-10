using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;

public class PlayerSystem : MonoBehaviour
{
  
    private Player inputPlayer;
    private Transform head;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 contactPoint;
    private RaycastHit playerhit;
    private enum RotationAxes { XY, X, Y };
    public enum WeaponType { ShotGun };
    private RotationAxes axis = RotationAxes.XY;
    private WeaponType wType;
    public float moveSpeed = 10;
    public float jumpSpeed = 20f;
    private float antiBumpFactor = .75f;
    public  float gravityPull = 50;
    private float gravity;
    [HideInInspector]
    public float inputX;
    [HideInInspector]
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
    private float ammo;
    public float[] returnRate = new float[2];
    public float[] blastBackRate = new float[2];
    private int reloadCounter = 0;
    private float gunflashTime = 0.05f;
    private float gunflashTimer;
    private bool gunFlash = false;
    private bool[] lerpAhead = new bool[2] { false, false };
    private bool[] lerpFinished = new bool[2] { false, false };
    private Transform bulletPool;
    private GameObject bulletPrefab;
    private GameObject weapon;
    [SerializeField]
    private Image ammoUI;
    [SerializeField]
    private Image armorUI;
    [SerializeField]
    private Image flashUI;
    [SerializeField]
    public Sprite[] flashSprites = new Sprite[2];   
    [SerializeField]
    private Image[] environmentUI = new Image[3];
    [SerializeField]
    private Text ammoText;
    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text armorText;
    private AudioClip weaponSound;
    public AudioClip[] playerDSound = new AudioClip[3];
    public AudioClip[] playerHSound = new AudioClip[3];
    public AudioClip[] shotgunReloadSounds = new AudioClip[2];

    [HideInInspector]
    public Vector3 weaponPosition;
    [HideInInspector]
    public Vector3 recoilPosition;  
    [HideInInspector]
    public Vector3 reloadStartPosition;    
    [HideInInspector]
    public Vector3 reloadEndPosition;
    public  Transform[] weaponEmitter;
    private MeshRenderer weaponMuzzle;
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
    public bool isShooting = false;
    public bool isRecoiling = false;
    public bool isDrowning = false;
    public bool isDamaged = false;
    public bool isHealed = false;
    private int environmentIndex = 0;
    private float environmentTime = 1f;
    private float environmentTimer;
    private float flashTime = 0.05f;
    private float flashTimer;
    [Header("Player Sounds")]
    public AudioClip jumpSfx;
    [Header("Player Tilt")]
    float tiltAngle;
    public float angle = 4f;
    public float tiltSpeed = 0f;
    [Header("PlayerWeapons")]
    public Transform[] weaponSEmitters = new Transform[1];
    public float[] weaponFireRates = new float[1];
    public GameObject[] weapons = new GameObject[1];
    public MeshRenderer[] weaponMuzzles = new MeshRenderer[1];
    public ParticleSystem weaponSmoke;
    public AudioClip[] weaponSounds = new AudioClip[1];
    public Vector3[] weaponRecoilPositions = new Vector3[1];
    public Quaternion[] weaponRecoilRotations = new Quaternion[1];
    public bool[] weaponEquipped = new bool[1];
    public bool[] weaponObtained = new bool[1];
    public int[] weaponAmmo = new int[1];
    public Transform[] bulletPools = new Transform[1];
    public GameObject[] bulletPrefabs = new GameObject[1];
    public List<int> emitterW0List = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    public Transform[] weaponReloadObj = new Transform[1];
    public Vector3[] weaponReloadStartPositions = new Vector3[1];
    public Vector3[] weaponReloadEndPositions = new Vector3[1];

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
        flashTimer = flashTime;
        gunflashTimer = gunflashTime;
        healthText.text = health.ToString();
        WeaponSetup(WeaponType.ShotGun);
    }
    private void Update()
    {
        Move();
        Look();
        GunFlash();
        ShootWeapon();
        RecoilWeapon();
        Environment(environmentIndex);
        DamageFlash();
        HealFlash();
    }
    private void DamageFlash()
    {
        if (!isDamaged)
            return;
        FlashTimer(1);
    }
    private void HealFlash()
    {
        if (!isHealed)
            return;
        FlashTimer(0);

    }
    private void FlashTimer(int spriteIndex)
    {
        if (flashUI.sprite != flashSprites[spriteIndex])
            flashUI.sprite = flashSprites[spriteIndex];
        flashUI.enabled = true;
        flashTimer -= Time.deltaTime;
        flashTimer = Mathf.Clamp(flashTimer, 0.0f, flashTime);
        if (flashTimer == 0)
        {
            flashTimer = flashTime;
            flashUI.enabled = false;
           if(isHealed) isHealed = false;
           else if(isDamaged) isDamaged = false;
        }
    }
    private void Environment(int index)
    {
        switch (index)
        {
            case 0: return;
            case 1: 
                {
                    EnvironmentDamage(environmentTime, 20);
                    break; 
                }
            case 2:
                {
                    EnvironmentDamage(environmentTime, 5);
                    break;
                }
            case 3:
                {
                    if (!isDrowning)
                    {
                        float speedAbsolute = 1.0f / environmentTimer;
                        environmentTimer -= Time.deltaTime * speedAbsolute;
                        environmentTimer = Mathf.Clamp(environmentTimer, 0.0f, environmentTime);
                        if (environmentTimer == 0)
                            isDrowning = true;
                    }
                    else
                    {
                        EnvironmentDamage(1, 2);
                    }
                    break;
                }
        }

    }
    private void EnvironmentDamage(float time, int damageAmt)
    {
        float speedAbsolute = 1.0f / environmentTimer;
        environmentTimer -= Time.deltaTime * speedAbsolute;
        environmentTimer = Mathf.Clamp(environmentTimer, 0.0f, time);
        if (environmentTimer == 0)
        {
            Damage(damageAmt);
            environmentTimer = time;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            environmentIndex = 1;
            environmentTime = 0.5f;
            environmentTimer = environmentTime;
        }
        else if (other.gameObject.CompareTag("Acid"))
        {
            environmentIndex = 2;
            environmentTime = 2;
            environmentTimer = environmentTime;
        }
        else if (other.gameObject.CompareTag("Water"))
        {
            environmentIndex = 3;
            environmentTime = 30;
            environmentTimer = environmentTime;
        }
        //else if (other.gameObject.CompareTag("Shotgun"))
        //    environmentIndex = 1;
        //else if (other.gameObject.CompareTag("ShotgunAmmo"))
        //    environmentIndex = 1;
        else if (other.gameObject.CompareTag("Health100"))
        {
            AudioSystem.PlayAudioSource(playerHSound[2], 1, 1);
            Recover(100, true);
            other.gameObject.SetActive(false);
        }
        else if (other.gameObject.CompareTag("Health25"))
        {
            if (health < 100)
            {
                AudioSystem.PlayAudioSource(playerHSound[1], 1, 1);
                Recover(25, false);
                other.gameObject.SetActive(false);
            }
        }
        else if (other.gameObject.CompareTag("Health15"))
        {
            if (health < 100)
            {
                AudioSystem.PlayAudioSource(playerHSound[0], 1, 1);
                Recover(15, false);
                other.gameObject.SetActive(false);
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Lava"))
            environmentIndex = 0;
        else if (other.gameObject.CompareTag("Acid"))
            environmentIndex = 0;
        else if (other.gameObject.CompareTag("Water"))
        {
            environmentIndex = 0;
            isDrowning = false;
        }
    }
    private void RecoilWeapon()
    {
        if (!isRecoiling)
            return;
        Vector3 lerpPosition = !lerpAhead[0] ? recoilPosition : weaponPosition;
        Vector3 lerpReload = !lerpAhead[1] ? reloadEndPosition : reloadStartPosition;
        float[] rate = new float[2]
        { 
            !lerpAhead[0] ? blastBackRate[0] : returnRate[0], 
            !lerpAhead[1] ? blastBackRate[1] : returnRate[1] 
        };
        // Move the gun back to forward
        weapon.transform.localPosition = Vector3.MoveTowards(weapon.transform.localPosition, lerpPosition, Time.deltaTime * rate[0]);
        if (weapon.transform.localPosition == recoilPosition && !lerpAhead[0])
            lerpAhead[0] = true;
        else if (weapon.transform.localPosition == weaponPosition && lerpAhead[0] && !lerpFinished[0])
        {
            lerpAhead[0] = false; 
            lerpFinished[0] = true;
        }
        // Move animated Weapon part back and forward
        weaponReloadObj[0].localPosition = Vector3.MoveTowards(weaponReloadObj[0].localPosition, lerpReload, Time.deltaTime * rate[1]);
        if (weaponReloadObj[0].localPosition == reloadEndPosition && !lerpAhead[1])
            lerpAhead[1] = true;
        else if (weaponReloadObj[0].localPosition == reloadStartPosition && lerpAhead[1] && !lerpFinished[1])
        {
            AudioSystem.PlayAudioSource(shotgunReloadSounds[reloadCounter], 1, 1);
            reloadCounter++;
            lerpAhead[1] = false; 
            if(reloadCounter > 1)
                lerpFinished[1] = true;
        }
        if (lerpFinished[0] && lerpFinished[1])
        {
            isRecoiling = false;
            weapon.transform.localPosition = weaponPosition;
            weaponReloadObj[0].localPosition = reloadStartPosition;
            for (int l = 0; l < lerpFinished.Length; l++)
                lerpFinished[l] = false;
            reloadCounter = 0;
        }
     
    }
    private void GunFlash()
    {
        if (!gunFlash) return;
        weaponMuzzle.enabled = true;
        gunflashTimer -= Time.deltaTime;
        gunflashTimer = Mathf.Clamp(gunflashTimer, 0.0f, 0.05f);
        if (gunflashTimer == 0.0f) 
        {
            weaponMuzzle.transform.parent.Rotate(0, 0, 30);
            weaponMuzzle.enabled = false;
            gunFlash = false;
            gunflashTimer = gunflashTime;
        }

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
            if (!inputPlayer.GetButton("A") && !isSliding)
            {
                // add index to timer value to increase over anti Jump Factor
                jumpTimer++;
            }
            else if (inputPlayer.GetButtonDown("A") && jumpTimer >= antiJumpFactor)
            {
                // player is now jumping
                isJumping = true;
                // jump upwards
                moveDirection.y = jumpSpeed;
                // reset jump timer so plasyer doesnt continue to jump
                jumpTimer = 0;
                AudioSystem.PlayAudioSource(jumpSfx, 0.7f, 1);
            }
            if (inputX > 0.2f) tiltAngle = -angle;
            else if (inputX < -0.2f) tiltAngle = angle;
            else tiltAngle = 0;
            Vector3 headRot = new Vector3(head.localRotation.x, 0, tiltAngle);
            Quaternion rot = Quaternion.Euler(headRot);
            head.localRotation = Quaternion.RotateTowards(head.localRotation, rot, Time.deltaTime * (tiltSpeed * 10));
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
        head.localEulerAngles = new Vector3(-lookRotation[1], 0, head.localEulerAngles.z);
    }
    // Player Collision ========================
    public void Damage(int amount)
    {
        AudioSystem.PlayAudioSource(playerDSound[Random.Range(0, playerDSound.Length)], 1, 1);
        isDamaged = true;
        health -= amount;
        health = Mathf.Clamp(health, 0, 200);
        if(health == 0)
        {
            Debug.Log("Player Has Died");
        }
        healthText.text = health.ToString();
    }
    private void Recover(int amt, bool overhealth)
    {
        isHealed = true;
        health += amt;
        int limit = overhealth ? 200 : 100;
        health = Mathf.Clamp(health, 0, limit);
        if (health == limit)
        {
            health = limit;
            Debug.Log("Player Has Recovered");
        }
        healthText.text = health.ToString();
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
    public void WeaponSetup(WeaponType type)
    {
        int weaponIndex = 0;
        switch (type)
        {
            case WeaponType.ShotGun: weaponIndex = 0; break;
        }
        for (int w = 0; w < weapons.Length; w++)
        {
            if(w == weaponIndex) weapons[w].SetActive(true);
            else weapons[w].SetActive(false);
        }
        for (int w = 0; w < weaponMuzzles.Length; w++)
            weaponMuzzles[w].enabled = false;
        if (weaponSmoke.isPlaying)
            weaponSmoke.Stop();
        weapon = weapons[weaponIndex];
        weaponMuzzle = weaponMuzzles[weaponIndex];
        weaponSound = weaponSounds[weaponIndex];
        recoilPosition = weaponRecoilPositions[weaponIndex];
        weaponPosition = weapon.transform.localPosition;
        reloadStartPosition = weaponReloadStartPositions[weaponIndex];
        reloadEndPosition = weaponReloadEndPositions[weaponIndex];
        ammo = weaponAmmo[weaponIndex];
        weaponEmitter = weaponSEmitters;
        bulletPrefab = bulletPrefabs[weaponIndex];
        bulletPool = bulletPools[weaponIndex];
        for (int w = 0; w < weaponEquipped.Length; w++)
        {
            if (w == weaponIndex) weaponEquipped[weaponIndex] = true;
            else weaponEquipped[weaponIndex] = false;
        }
        wType = type;
    }

    public void ShootWeapon()
    {
        if (inputPlayer.GetButton("RT") && !isRecoiling)
        {
            isRecoiling = true;
            isShooting = true;
            FireWeaponType(wType);
            AudioSystem.PlayAudioSource(weaponSound, 1, 1);
            gunFlash = true;
        }
        else if (inputPlayer.GetButtonUp("RT"))
            isShooting = false;
    }
    private void FireWeaponType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.ShotGun:
                {
                    for (int e = 0; e < 5; e++)
                    {
                        int rnd = Random.Range(0, 7);
                        if (emitterW0List.Contains(rnd))
                            emitterW0List.Remove(rnd);
                        else
                        {
                            rnd = emitterW0List[Random.Range(0, emitterW0List.Count)];
                            emitterW0List.Remove(rnd);
                        }
                        SetupBullet(weaponEmitter[rnd], 50000);
                    }
                    emitterW0List.Clear();
                    for (int l = 0; l < 7; l++)
                        emitterW0List.Add(l);
                    weaponSmoke.transform.localPosition = weaponEmitter[0].localPosition;
                    weaponSmoke.Play();
                    break;
                }
        }
    }
    private void SetupBullet(Transform emitter, float bulletForce)
    {
        GameObject bullet = AccessWeaponBullet();
        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        bullet.transform.position = emitter.position;
        bullet.transform.rotation = emitter.rotation;
        bullet.SetActive(true);
        bulletSystem.SetupBullet(bulletForce, 5);
        rb.AddForce(emitter.transform.forward * bulletForce);
    }
    public GameObject AccessWeaponBullet()
    {
        for (int b = 0; b < bulletPool.childCount; b++)
        {
            if (!bulletPool.GetChild(b).gameObject.activeInHierarchy)
                return bulletPool.GetChild(b).gameObject;
        }
        if (GameSystem.expandBulletPool)
        {
            GameObject newBullet = Instantiate(bulletPrefab, bulletPool);
            return newBullet;
        }
        else
            return null;
    }
    private float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
}
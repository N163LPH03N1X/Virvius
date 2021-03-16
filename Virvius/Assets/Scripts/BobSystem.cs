using UnityEngine;

public class BobSystem : MonoBehaviour
{
    private WeaponSystem weaponSystem;
    private InputSystem inputSystem;
    private PlayerSystem playerSystem;
    public enum weaponAnimType { up, down, left, right, jump, idle, origin };

    [Header("Weapon Object")]
    public float weaponBlastBackRate = 6;
    public float weaponReturnRate = 1;
    private Vector3 weaponStartPosition;
    public Vector3 weaponEndPosition;
    [Header("Animated Object")]
    public float animatedBlastBackRate = 3.5f;
    public float animatedReturnRate = 3.5f;
    public Vector3 animatedStartPosition;
    public Vector3 animatedEndPosition;
    public AudioClip[] weaponAnimatedSounds = new AudioClip[1];
    public Transform[] weaponAnimatedObj = new Transform[1];
    [Header("Weapon Bobbing")]
    public float bobSpeed = 0.65f;
    [Header("Vertical Amount")]
    public float upAmount = -2.9f;
    public float downAmount = -3.2f;
    [Header("Horizontal Amount")]
    public float leftRightAmount = 0.05f;
    [Header("Jump Amount")]
    public float heightAmount = -2.5f;
    private bool[] lerpAhead = new bool[2] { false, false };
    private bool[] lerpFinished = new bool[2] { false, false };
    private Vector3[] bobVectors = new Vector3[3];
    private float MovementSpeedY = 1;
    private float AboveOrBelowZeroY = 0;
    private float UpAndDownAmount = 15;
    private int bobIndex = 0;
    private bool switchDir = false;
    private bool idle = true;
    private bool fall = false;
    [HideInInspector]
    public bool isRecoiling = false;
    void Start()
    {
        inputSystem = InputSystem.inputSystem;
        playerSystem = PlayerSystem.playerSystem;
        weaponSystem = transform.parent.GetComponent<WeaponSystem>();
        bobVectors[0] = WeaponAnimation(weaponAnimType.right);
        bobVectors[1] = WeaponAnimation(weaponAnimType.up);
        bobVectors[2] = WeaponAnimation(weaponAnimType.left);
        weaponStartPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerSystem.isDead)
            return;
        AnimatePlayer();
        RecoilWeapon();
    }
    private void AnimatePlayer()
    {
        if (isRecoiling)
            return;
        if (!inputSystem.isJumping && !weaponSystem.isShooting)
        {
            if (inputSystem.inputX == 0 && inputSystem.inputY == 0)
            {
                fall = false;
                weaponAnimType type = idle ? weaponAnimType.idle : weaponAnimType.origin;
                if (bobIndex != 0) { bobIndex = 0; switchDir = false; }
                transform.localPosition = MoveTowards(WeaponAnimation(type), bobSpeed);
                if (transform.localPosition == WeaponAnimation(weaponAnimType.origin) && !idle) idle = true;
            }
            else
            {
                if (!inputSystem.isFalling )
                {
                    if (fall) fall = false; if (idle) idle = false;
                    transform.localPosition = MoveTowards(bobVectors[bobIndex], bobSpeed);
                    if (transform.localPosition == bobVectors[bobIndex])
                    {
                        bobIndex += switchDir ? -1 : +1;
                        if (bobIndex > 2 || bobIndex < 0) { bobIndex = 1; switchDir = !switchDir; }
                    }
                }
            }
        }
        else
        {
            idle = false;
            float speed = fall ? bobSpeed / 1.5f : bobSpeed + 0.75f;
            weaponAnimType type = fall ? weaponAnimType.origin : weaponAnimType.jump;
            transform.localPosition = MoveTowards(WeaponAnimation(type), speed);
            if (transform.localPosition == WeaponAnimation(weaponAnimType.jump) && !fall) fall = true;
        }
    }
    public Vector3 WeaponAnimation(weaponAnimType type)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        Vector3 animVector;
        switch (type)
        {
            case weaponAnimType.up: x = 0f; y = upAmount; z = transform.localPosition.z; break;
            case weaponAnimType.down: x = 0f; y = downAmount; z = transform.localPosition.z; break;
            case weaponAnimType.left: x = -leftRightAmount; y = -3; z = transform.localPosition.z; break;
            case weaponAnimType.right: x = leftRightAmount; y = -3; z = transform.localPosition.z; break;
            case weaponAnimType.jump: x = 0f; y = heightAmount; z = transform.localPosition.z; break;
            case weaponAnimType.idle: 
                {
                    float newY = ((Mathf.Sin(Time.time * MovementSpeedY) + AboveOrBelowZeroY) / UpAndDownAmount);
                    x = weaponStartPosition.x; y = weaponStartPosition.y + newY; z = weaponStartPosition.z; break; 
                }
            case weaponAnimType.origin: x = weaponStartPosition.x; y = weaponStartPosition.y; z = weaponStartPosition.z; break;
        }
        animVector = new Vector3(x, y, z);
        return animVector;
    }
    public Vector3 MoveTowards(Vector3 destPosition, float transitionSpeed)
    {
        Vector3 move = Vector3.MoveTowards(transform.localPosition, destPosition, Time.deltaTime * transitionSpeed);
        return move;
    }    
    public Quaternion RotateTowards(Quaternion destRotation, float transitionSpeed)
    {
        Quaternion move = Quaternion.RotateTowards(transform.localRotation, destRotation, Time.deltaTime * transitionSpeed);
        return move;
    }
    private void RecoilWeapon()
    {
        if (!isRecoiling)
            return;
        Vector3 lerpPosition = !lerpAhead[0] ? weaponEndPosition : weaponStartPosition;
        Vector3 lerpReload = !lerpAhead[1] ? animatedEndPosition : animatedStartPosition;
        float[] rate = new float[2]
        {
            !lerpAhead[0] ? weaponBlastBackRate : weaponReturnRate,
            !lerpAhead[1] ? animatedBlastBackRate : animatedReturnRate
        };
        if (!lerpFinished[0])
        {
            // Move the gun forward to end
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, lerpPosition, Time.deltaTime * rate[0]);

            if (transform.localPosition == weaponEndPosition && !lerpAhead[0])
                lerpAhead[0] = true;
            // Move the gun back to start
            else if (transform.localPosition == weaponStartPosition && lerpAhead[0])
            {
                AudioSystem.PlayAudioSource(weaponAnimatedSounds[0], 1, 1);
                lerpAhead[0] = false;
                lerpFinished[0] = true;
            }
        }
        else if (lerpFinished[0] && !lerpFinished[1])
        {
            // Move the animated object forward to end
            weaponAnimatedObj[0].localPosition = Vector3.MoveTowards(weaponAnimatedObj[0].localPosition, lerpReload, Time.deltaTime * rate[1]);
            if (weaponAnimatedObj[0].localPosition == animatedEndPosition && !lerpAhead[1])
            {
                AudioSystem.PlayAudioSource(weaponAnimatedSounds[0], 1, 1);
                lerpAhead[1] = true;
            }
            // Move the animated object back to start
            else if (weaponAnimatedObj[0].localPosition == animatedStartPosition && lerpAhead[1] && lerpFinished[0])
            {
               
                lerpAhead[1] = false;
                lerpFinished[1] = true;
            }
        }
        // when both are finished animating, ready to fire the gun
        else if (lerpFinished[0] && lerpFinished[1])
        {
            isRecoiling = false;
            transform.localPosition = weaponStartPosition;
            weaponAnimatedObj[0].localPosition = animatedStartPosition;
            for (int l = 0; l < lerpFinished.Length; l++)
                lerpFinished[l] = false;
        }
    }
}

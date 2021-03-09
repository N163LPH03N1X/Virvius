using UnityEngine;

public class BobSystem : MonoBehaviour
{
    public PlayerSystem playerSystem;
    public enum weaponAnimType { up, down, left, right, jump, idle, origin };
    Vector3 lastPosition;
    Vector3[] bobVectors = new Vector3[3];
    public float bobSpeed;
    public float MovementSpeedY = 3;
    public float AboveOrBelowZeroY = 0;
    public float UpAndDownAmount = 5;
    int bobIndex = 0;
    bool switchDir = false;
    bool idle = true;
    bool fall = false;
    Vector3 newPosition;
    void Start()
    {
        bobVectors[0] = WeaponAnimation(weaponAnimType.right);
        bobVectors[1] = WeaponAnimation(weaponAnimType.up);
        bobVectors[2] = WeaponAnimation(weaponAnimType.left);
        lastPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        AnimatePlayer();
    }
    private void AnimatePlayer()
    {
        if (playerSystem.isRecoiling)
            return;
        if (!playerSystem.isJumping && !playerSystem.isShooting)
        {
            if (playerSystem.inputX == 0 && playerSystem.inputY == 0)
            {
                fall = false;
                weaponAnimType type = idle ? weaponAnimType.idle : weaponAnimType.origin;
                if (bobIndex != 0) { bobIndex = 0; switchDir = false; }
                transform.localPosition = MoveTowards(WeaponAnimation(type), bobSpeed);
                if (transform.localPosition == WeaponAnimation(weaponAnimType.origin) && !idle) idle = true;
            }
            else
            {
                if (!playerSystem.isFalling )
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
            float speed = fall ? bobSpeed / 2.5f : bobSpeed + 0.75f;
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
            case weaponAnimType.up: x = 0f; y = -2.8f; z = transform.localPosition.z; break;
            case weaponAnimType.down: x = 0f; y = -3.2f; z = transform.localPosition.z; break;
            case weaponAnimType.left: x = -0.025f; y = -3f; z = transform.localPosition.z; break;
            case weaponAnimType.right: x = 0.025f; y = -3f; z = transform.localPosition.z; break;
            case weaponAnimType.jump: x = 0f; y = -2.5f; z = transform.localPosition.z; break;
            case weaponAnimType.idle: 
                {
                    float newY = ((Mathf.Sin(Time.time * MovementSpeedY) + AboveOrBelowZeroY) / UpAndDownAmount);
                    x = lastPosition.x; y = lastPosition.y + newY; z = lastPosition.z; break; 
                }
            case weaponAnimType.origin: x = lastPosition.x; y = lastPosition.y; z = lastPosition.z; break;
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
}

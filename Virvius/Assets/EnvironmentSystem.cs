using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentSystem : MonoBehaviour
{
    public static EnvironmentSystem environmentSystem;
    private PlayerSystem playerSystem;
    private InputSystem inputSystem;
    private GameObject waveEffect;
    private RaycastHit hit;
    private Vector3 contactPoint;
    [SerializeField]
    private Image environmentUI;
    [SerializeField]
    private Sprite[] environmentSprites = new Sprite[3];
    public bool isDrowning = false;
    [HideInInspector]
    public int environmentIndex = 0;
    private float environmentTime = 1f;
    private float environmentTimer;
    private SphereCollider col;
    public AudioClip playerDrSound;
    [HideInInspector]
    public  string[] environmentTag = new string[3] { "Lava", "Acid", "Water"};
    public bool headUnderWater = false;
    private void Awake()
    {
        environmentSystem = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        playerSystem = PlayerSystem.playerSystem;
        inputSystem = InputSystem.inputSystem;
        waveEffect = Camera.main.transform.GetChild(0).gameObject;
        col = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
      
        Environment(environmentIndex);
    }
    private void Environment(int index)
    {
        if (playerSystem.isDead)
            return;
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
                        environmentTimer -= Time.deltaTime;
                        environmentTimer = Mathf.Clamp(environmentTimer, 0.0f, environmentTime);
                        if (environmentTimer == 0)
                            isDrowning = true;
                        //Debug.Log(environmentTimer);
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
            if (isDrowning)
                AudioSystem.PlayAudioSource(playerDrSound, Random.Range(0.7f, 1), 1);
            playerSystem.Damage(damageAmt);
            environmentTimer = time;
        }
    }
    public void SetEnvironment(float time, int index)
    {

        environmentIndex = index;
        environmentTime = time;
        environmentTimer = environmentTime;
        if (isDrowning) isDrowning = false;
    }
    private void OnTriggerStay(Collider other)
    {
        for (int en = 0; en < environmentTag.Length; en++)
        {
            if (other.gameObject.CompareTag(environmentTag[en]))
            {
                if (col.bounds.center.y <= other.bounds.max.y)
                    ActiveEnvironmentUI(true);
                else if (col.bounds.center.y > other.bounds.max.y)
                    ActiveEnvironmentUI(false);
                break;
            }
        }
    }
    public void ActiveEnvironmentUI(bool active)
    {
        environmentUI.enabled = active;
        waveEffect.SetActive(active);
    }
    private void OnTriggerEnter(Collider other)
    {
        for (int e = 0; e < environmentTag.Length; e++)
        {
            if (other.gameObject.CompareTag(environmentTag[e])) 
            {
                if (e == 2)
                    SetEnvironment(25, 3);
                ActivateEnvironment(e + 1); 
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        for (int e = 0; e < environmentTag.Length; e++)
        {
            if (other.gameObject.CompareTag(environmentTag[e])) 
            {
               
                if(e == 2)
                    SetEnvironment(0, 0);
                if (headUnderWater)
                {
         
                    headUnderWater = false;
                }
            }

        }
    }
    public void ActivateEnvironment(int index)
    {
        if (index > 0)
        {
            float vel = 0;
            inputSystem.moveDirection.y = vel;
            headUnderWater = true;
            environmentUI.sprite = environmentSprites[index - 1];
        }
    }
    public void ActivateSwimming(bool active)
    {
        inputSystem.isSwimming = active;
        if (active)
        {
            float vel = inputSystem.moveDirection.y / 2;
            inputSystem.moveDirection.y = vel;
        }
    }
}

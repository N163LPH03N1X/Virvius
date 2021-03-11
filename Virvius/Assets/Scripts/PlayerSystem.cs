using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerSystem : MonoBehaviour
{
    public static PlayerSystem playerSystem;
    public EnvironmentSystem environmentSystem;
    // Player Controller Fields
    [Header("Player Controller")]
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    InputSystem inputSystem;
    Transform head;
    public WeaponSystem weaponSystem;
    // Player Health Armor and Ammo
    private float health = 100;
    private float armor = 50;
    // Player UI
    [Header("Player UI")]
    [SerializeField]
    private Image armorUI;
    [SerializeField]
    private Image flashUI;
    [SerializeField]
    public Sprite[] flashSprites = new Sprite[2];   
   
    [SerializeField]
    private Text ammoText;
    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text armorText;
    // Player Sound
    [Header("Player Sound")]
    public AudioClip[] playerDmgSounds = new AudioClip[3];
    public AudioClip[] playerHealSounds = new AudioClip[3];
    public AudioClip[] playerDiedSounds = new AudioClip[3];
    // Player Attributes
    [Header("Player Attributes")]
 
    public bool isDamaged = false;
    public bool isHealed = false;
    public bool isDead = false;

    private float flashTime = 0.05f;
    private float flashTimer;
    private void Awake()
    {
        playerSystem = this;
    }
    private void Start()
    {
        playerStartPosition = transform.localPosition;
        playerStartRotation = transform.localRotation;
        flashTimer = flashTime;
        inputSystem = GetComponent<InputSystem>();
        head = transform.GetChild(0);
        ApplyPlayerHeatlhAndArmor();
    }
    private void Update()
    {
        ResetPlayer();
        if (isDead)
            return;
        DamageFlash();
        HealFlash();
        if (health == 0)
            KillPlayer();
    }
    // Player UI ==================================================
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
        if (isDamaged)
        {
            Vector3 headRot = new Vector3(head.localRotation.x, 0, 3);
            Quaternion rot = Quaternion.Euler(headRot);
            head.localRotation = rot;
        }
        if (flashUI.sprite != flashSprites[spriteIndex])
            flashUI.sprite = flashSprites[spriteIndex];
        flashUI.enabled = true;
        flashTimer -= Time.deltaTime;
        flashTimer = Mathf.Clamp(flashTimer, 0.0f, flashTime);
        if (flashTimer == 0)
        {
            flashTimer = flashTime;
            flashUI.enabled = false;
            if (isHealed) isHealed = false;
            else if (isDamaged)
            {
                Vector3 headRot = new Vector3(head.localRotation.x, 0, 0);
                Quaternion rot = Quaternion.Euler(headRot);
                head.localRotation = rot;
                isDamaged = false;
            }
        }
    }
    // Player Health ==============================================

 
    public void Damage(int amount)
    {
        if(!environmentSystem.isDrowning)
            AudioSystem.PlayAudioSource(playerDmgSounds[Random.Range(0, playerDmgSounds.Length)], 1, 1);
        isDamaged = true;
        health -= amount;
        health = Mathf.Clamp(health, 0, 200);
        if (health == 0 && !isDead)
        {
            Debug.Log("Player Has Died");
        }
        ApplyPlayerHeatlhAndArmor();
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
        ApplyPlayerHeatlhAndArmor();
    }
    private void ApplyPlayerHeatlhAndArmor()
    {
        if (health <= 25)
            healthText.color = Color.red;
        else if (health > 25)
            healthText.color = Color.white;
        armorText.text = armor.ToString();
        healthText.text = health.ToString();
    }
 
    private void KillPlayer()
    {
        isDead = true;
        flashUI.enabled = false;
        if (environmentSystem.environmentIndex == 0)
            AudioSystem.PlayAudioSource(playerDiedSounds[0],1 ,1);
        else
            AudioSystem.PlayAudioSource(playerDiedSounds[1], 1, 1);
        AudioSystem.MusicPlayStop(false);
        Vector3 headLoc = new Vector3(head.localPosition.x, -2, 0);
        Vector3 headRot = new Vector3(head.localRotation.x, head.localRotation.y, -50);
        Quaternion rot = Quaternion.Euler(headRot);
        head.localRotation = rot;
        head.localPosition = headLoc;
        weaponSystem.weapon.SetActive(false);
    }
    public void ResetPlayer()
    {
        if (inputSystem.inputPlayer.GetButtonDown("A") && isDead)
        {
            transform.localPosition = playerStartPosition;
            transform.localRotation = playerStartRotation;
            isDead = false;
            AudioSystem.MusicPlayStop(true);
            Vector3 headLoc = new Vector3(head.localPosition.x, 5, 0);
            Vector3 headRot = Vector3.zero;
            Quaternion rot = Quaternion.Euler(headRot);
            head.localRotation = rot;
            head.localPosition = headLoc;
            weaponSystem.weapon.SetActive(true);
            health = 100;
            weaponSystem.ammo = 100;
            armor = 0;
            environmentSystem.SetEnvironment(0, 0);
            environmentSystem.ActivateEnvironment(0);
            environmentSystem.ActiveEnvironmentUI(false);
            ApplyPlayerHeatlhAndArmor();
        }
    }
    // Player Collision ===========================================
  
    private void OnTriggerEnter(Collider other)
    {
        // Player Collisions with Environment
        if (other.gameObject.CompareTag("Lava")) environmentSystem.SetEnvironment(0.5f, 1);
        else if (other.gameObject.CompareTag("Acid")) environmentSystem.SetEnvironment(1f, 2);
        else if (other.gameObject.CompareTag("Water")) environmentSystem.SetEnvironment(25, 3);

        // Player Collisions with Weapons
        //else if (other.gameObject.CompareTag("Shotgun"))
        //    environmentIndex = 1;
        //else if (other.gameObject.CompareTag("ShotgunAmmo"))
        //    environmentIndex = 1;

        // Player Collisions with Items
        if (other.gameObject.CompareTag("Health100"))
        {
            AudioSystem.PlayAudioSource(playerHealSounds[2], 1, 1);
            Recover(100, true);
            other.gameObject.SetActive(false);
        }
        else if (other.gameObject.CompareTag("Health25"))
        {
            if (health < 100)
            {
                AudioSystem.PlayAudioSource(playerHealSounds[1], 1, 1);
                Recover(25, false);
                other.gameObject.SetActive(false);
            }
        }
        else if (other.gameObject.CompareTag("Health15"))
        {
            if (health < 100)
            {
                AudioSystem.PlayAudioSource(playerHealSounds[0], 1, 1);
                Recover(15, false);
                other.gameObject.SetActive(false);
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Lava")) environmentSystem.SetEnvironment(0, 0);
        else if (other.gameObject.CompareTag("Acid")) environmentSystem.SetEnvironment(0, 0);
        else if (other.gameObject.CompareTag("Water")) environmentSystem.SetEnvironment(0, 0);
    }
    // Player Weapon ==============================================
  
    // Player Movement ============================================
   
    // Game Utilities =============================================
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
    private float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
}
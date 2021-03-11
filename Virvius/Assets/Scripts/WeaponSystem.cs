using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSystem : MonoBehaviour
{
    private InputSystem inputSystem;
    private BobSystem bobSystem;
    private PlayerSystem playerSystem;
    public enum WeaponType { Unarmed, ShotGun, DoubleBarrel, ChainGun, MiniGun, Grenade, RocketLauncher, RailGun, PlasmaCannon, BlackHole };
    private WeaponType wType;
    private int weaponIndex = 0;
    [HideInInspector]
    public float ammo = 100;
    [SerializeField]
    private Image ammoImage;
    [SerializeField]
    private Text ammoText;
    private float gunflashTime = 0.05f;
    private float gunflashTimer;
    private bool gunFlash = false;
    // Single Assignment Values
    private Transform[] weaponEmitter;
    [HideInInspector]
    public GameObject weapon;
    private MeshRenderer weaponMuzzle;
    private AudioClip weaponSound;
    private Transform bulletPool;
    private GameObject bulletPrefab;
    // Multiple Assignment Values
    [Header("Weapon Array Section")]
    public Transform[] weaponSEmitters = new Transform[1];
    public GameObject[] weapons = new GameObject[1];
    public MeshRenderer[] weaponMuzzles = new MeshRenderer[1];
    public AudioClip[] weaponSounds = new AudioClip[1];
    public Transform[] bulletPools = new Transform[1];
    public GameObject[] bulletPrefabs = new GameObject[1];
    public ParticleSystem[] weaponSmoke = new ParticleSystem[1];
    public bool[] weaponEquipped = new bool[1];
    public bool[] weaponObtained = new bool[1];
    public int[] weaponAmmo = new int[1];
    public Sprite[] weaponAmmoSprites = new Sprite[1];
    private List<int> emitterW0List = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    [HideInInspector]
    public bool isShooting = false;
    void Start()
    {
        inputSystem = InputSystem.inputSystem;
        playerSystem = PlayerSystem.playerSystem;
        gunflashTimer = gunflashTime;
        WeaponSetup(WeaponType.ShotGun);
    }
   
    void Update()
    {
        if (playerSystem.isDead)
            return;
        GunFlash();
        ShootWeapon();
    }
    private void WeaponSetup(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Unarmed: weaponIndex = 1; break;
            case WeaponType.ShotGun: weaponIndex = 0; break;
            case WeaponType.DoubleBarrel: weaponIndex = 2; break;
            case WeaponType.ChainGun: weaponIndex = 3; break;
            case WeaponType.MiniGun: weaponIndex = 4; break;
            case WeaponType.Grenade: weaponIndex = 5; break;
            case WeaponType.RocketLauncher: weaponIndex = 6; break;
            case WeaponType.RailGun: weaponIndex = 7; break;
            case WeaponType.PlasmaCannon: weaponIndex = 8; break;
            case WeaponType.BlackHole: weaponIndex = 9; break;
        }
        for (int w = 0; w < weapons.Length; w++)
        {
            if (w == weaponIndex) weapons[w].SetActive(true);
            else weapons[w].SetActive(false);
        }
        for (int w = 0; w < weaponMuzzles.Length; w++)
            weaponMuzzles[w].enabled = false;
        if (weaponSmoke[weaponIndex].isPlaying)
            weaponSmoke[weaponIndex].Stop();
        weapon = weapons[weaponIndex];
        bobSystem = weapon.GetComponent<BobSystem>();
        weaponMuzzle = weaponMuzzles[weaponIndex];
        weaponSound = weaponSounds[weaponIndex];
        ammo = weaponAmmo[weaponIndex];
        ammoText.text = ammo.ToString();
        ammoImage.sprite = weaponAmmoSprites[weaponIndex];
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
    private void ShootWeapon()
    {
        if (inputSystem.inputPlayer.GetButton("RT") && !bobSystem.isRecoiling)
        {
            bobSystem.isRecoiling = true;
            isShooting = true;
            FireWeaponType(wType);
            AudioSystem.PlayAudioSource(weaponSound, 1, 1);
            gunFlash = true;
        }
        else if (inputSystem.inputPlayer.GetButtonUp("RT"))
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
                    weaponSmoke[weaponIndex].transform.localPosition = weaponEmitter[0].localPosition;
                    weaponSmoke[weaponIndex].Play();
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
    private GameObject AccessWeaponBullet()
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
}

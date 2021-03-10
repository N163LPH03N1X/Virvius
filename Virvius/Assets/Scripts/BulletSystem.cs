using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    float bulletForce = 0;
    float bulletTimer = 0;
    bool bulletActive = false;
    public GameObject spark;
    public GameObject hole;
    Transform sparkPool;
    Transform holePool;
    RaycastHit hit;
    Vector3 fwd;
    // Update is called once per frame
    private void Start()
    {
        sparkPool = GameObject.Find("GameSystem/Game/ObjectPool/SparkPool").transform;
        holePool = GameObject.Find("GameSystem/Game/ObjectPool/HolePool").transform;
    }
    void Update()
    {
        fwd = transform.TransformDirection(Vector3.forward);
        Debug.DrawRay(transform.position, fwd * Mathf.Infinity, Color.red);
        KillTimer();
    }
    public void SetupBullet(float bulletForce, float bulletTime)
    {
        this.bulletForce = bulletForce;
        bulletTimer = bulletTime;
        bulletActive = true;
    }
    private void KillTimer()
    {
        if (!bulletActive)
            return;
        bulletTimer -= Time.deltaTime;
        if (bulletTimer < 0) { bulletTimer = 0; bulletActive = false; gameObject.SetActive(false); }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject)
        {
            RayCast();
            EnableSpark();
        }
    }
    private void RayCast()
    {
        if (Physics.Raycast(transform.position, fwd, out hit, Mathf.Infinity))
            EnableHole();
    }
    private void EnableSpark()
    {
        GameObject spark = AccessSpark();
        ParticleSystem ps = spark.GetComponent<ParticleSystem>();
        spark.SetActive(true);
        ps.Play();
        spark.transform.position = transform.position;
        gameObject.SetActive(false);
    }
    private GameObject AccessSpark()
    {
        for (int b = 0; b < sparkPool.childCount; b++)
        {
            if (!sparkPool.GetChild(b).gameObject.activeInHierarchy)
                return sparkPool.GetChild(b).gameObject;
        }
        if (GameSystem.expandBulletPool)
        {
            GameObject newSpark = Instantiate(spark, sparkPool);
            return newSpark;
        }
        else
            return null;
    }
    private void EnableHole()
    {
        GameObject hole = AccessHole();
        HoleSystem holeSystem = hole.GetComponent<HoleSystem>();
        holeSystem.SetupHole(5);
        hole.transform.position = hit.point;
        hole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        hole.SetActive(true);
    }
    private GameObject AccessHole()
    {
        for (int b = 0; b < holePool.childCount; b++)
        {
            if (!holePool.GetChild(b).gameObject.activeInHierarchy)
                return holePool.GetChild(b).gameObject;
        }
        if (GameSystem.expandBulletPool)
        {
            GameObject newHole = Instantiate(hole, Vector3.zero, Quaternion.identity, holePool);
            return newHole;
        }
        else
            return null;
    }
}

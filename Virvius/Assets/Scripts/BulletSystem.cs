using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    float bulletForce = 0;
    float bulletTimer = 0;
    bool bulletActive = false;
    // Update is called once per frame
    void Update()
    {
        Move();
        KillTimer();
    }
    private void Move()
    {
        if (!bulletActive)
            return;
        transform.Translate(Vector3.forward * Time.deltaTime * bulletForce);
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
            gameObject.SetActive(false);
    }
}

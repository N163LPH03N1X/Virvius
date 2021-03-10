using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleSystem : MonoBehaviour
{
    private bool holeActive;
    private float lifeTimer;
    private bool isFading;
    private float fadePercentage = 1;
    private MeshRenderer meshRend;
    
    private void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        
    }

    void Update()
    {
        KillTimer();
        FadeOut();
    }
    private void KillTimer()
    {
        if (!holeActive)
            return;
        lifeTimer -= Time.deltaTime;
        if (lifeTimer < 0) { lifeTimer = 0; holeActive = false; isFading = true; }
    }
    public void SetupHole(float lifeTime)
    {
        if(meshRend == null)
            meshRend = GetComponent<MeshRenderer>();
        meshRend.material.color = new Color(meshRend.material.color.r, meshRend.material.color.g, meshRend.material.color.b, 1);
        lifeTimer = lifeTime;
        holeActive = true;
    }
    private void FadeOut()
    {
        if (isFading == false) return;
        float speedAbsolute = 1.0f / 2;  // speed desired by user
        float speedDirection = speedAbsolute;  // + or -
        float deltaFade = Time.deltaTime * speedDirection;  // how much volume changes in 1 frame
        fadePercentage -= deltaFade;  // implement change
        fadePercentage = Mathf.Clamp(fadePercentage, 0.0f, 1.0f);  // make sure you're in 0..100% 
        meshRend.material.color = new Color(meshRend.material.color.r, meshRend.material.color.g, meshRend.material.color.b, fadePercentage);
        if (fadePercentage == 0.0f) { isFading = false; gameObject.SetActive(false); }
    }
}

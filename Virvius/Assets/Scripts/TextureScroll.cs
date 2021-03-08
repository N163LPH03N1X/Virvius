using UnityEngine;
using System.Collections;

public enum Move { Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, None}

public class TextureScroll : MonoBehaviour
{
    public int materialNum = 0;
    [Header("Set Texture Direction")]
    [Space]
    public Move move;
    [Header("Set Texture Speed")]
    [Space]
    public float scrollSpeed = 0.5F;
    [Header("Reset Texture Direction")]
    [Space]
    public bool reset;
    IEnumerator routine;
    [HideInInspector]
    public bool allMaterials = false;
    public bool isActive = false;
    public bool byPass;
    public bool autoStart = true;
    private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    void OnEnable()
    {
        Start();
    }
    public void Start()
    {
        if (autoStart)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = SetDirection(move);
            StartCoroutine(routine);
        }
    }
    public void Stop()
    {
        if (routine != null)
            StopCoroutine(routine);
    }
    public void Update()
    {
        if (!isActive && !reset && autoStart)
        {
            isActive = true;
            if (routine != null)
                StopCoroutine(routine);
            routine = SetDirection(move);
            StartCoroutine(routine);
        }
        else if (reset && isActive && autoStart)
        {
            if (routine != null)
                StopCoroutine(routine);
            isActive = false;
        }
    }
    public IEnumerator SetDirection(Move direction)
    {
        Renderer rend = GetComponent<Renderer>();
        if (allMaterials)
            materialNum = rend.materials.Length;
        switch (direction)
        {
            case Move.Up:
                {
                    while (isActive)
                    {
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x, rend.materials[materialNum].mainTextureOffset.y + offset);
                        yield return endOfFrame; 
                    }
                    break;
                }
            case Move.UpRight:
                {
                    while (isActive)
                    {
                      
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x + offset, rend.materials[materialNum].mainTextureOffset.y + offset);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.Right:
                {
                    while (isActive)
                    {
                     
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x + offset, rend.materials[materialNum].mainTextureOffset.y);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.DownRight:
                {
                    while (isActive)
                    {
                       
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x + offset, rend.materials[materialNum].mainTextureOffset.y - offset);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.Down:
                {
                    while (isActive)
                    {
                      
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x, rend.materials[materialNum].mainTextureOffset.y - offset);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.DownLeft:
                {
                    while (isActive)
                    {
                      
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x - offset, rend.materials[materialNum].mainTextureOffset.y - offset);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.Left:
                {
                    while (isActive)
                    {
                        
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x - offset, rend.materials[materialNum].mainTextureOffset.y);
                        yield return endOfFrame;
                    }
                    break;
                }
            case Move.UpLeft:
                {
                    while (isActive)
                    {
                      
                        float offset = Time.deltaTime * scrollSpeed;
                        rend.materials[materialNum].mainTextureOffset = new Vector2(rend.materials[materialNum].mainTextureOffset.x - offset, rend.materials[materialNum].mainTextureOffset.y + offset);
                        yield return endOfFrame;
                    }
                    break;
                }
        }
    }
    public void ChangeScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}

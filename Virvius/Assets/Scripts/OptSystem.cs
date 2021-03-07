using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class OptSystem
{
    //New Player -> Rewireds InputSystem
    private Player _Input;
    //New Color
    private Color _Color;
    //new 3D Vector
    private Vector3 _Vector3;
    //new 2D Vector
    private Vector2 _Vector2;
    //new Rotation
    private Quaternion _Quaternion;
    //new Wait for Seconds -> TimeScale Controlled
    private Dictionary<float, WaitForSeconds> _TimeInterval;
    //new Wait for Seconds -> TimeScale UnControlled
    private Dictionary<float, WaitForSecondsRealtime> _RealTimeInterval;
    //new Wait for Frame End
    public WaitForEndOfFrame _EndOfFrame;
    //new Wait for Fixed Updated
    public WaitForFixedUpdate _FixedUpdate;

    //Awake, Start Update Menthods

    //Methods
    public Player Input { get { _Input = ReInput.players.GetPlayer(0); return _Input; } }
    public WaitForEndOfFrame EndOfFrame { get { return _EndOfFrame; } }
    public WaitForFixedUpdate FixedUpdate { get { return _FixedUpdate; } }
    public WaitForSeconds Wait(float seconds)
    {
        _TimeInterval = new Dictionary<float, WaitForSeconds>(100);
        if (!_TimeInterval.ContainsKey(seconds))
            _TimeInterval.Add(seconds, new WaitForSeconds(seconds));
        return _TimeInterval[seconds];
    }
    public WaitForSecondsRealtime WaitRealtime(float seconds)
    {
        _RealTimeInterval = new Dictionary<float, WaitForSecondsRealtime>(100);
        if (!_RealTimeInterval.ContainsKey(seconds))
            _RealTimeInterval.Add(seconds, new WaitForSecondsRealtime(seconds));
        return _RealTimeInterval[seconds];
    }
  
    public Color Color(float r, float g, float b, float a)
    {
        _Color.r = r;
        _Color.g = g;
        _Color.b = b;
        _Color.a = a;
        return _Color;
    }
    public Vector3 Vector3(float x, float y, float z)
    {
        _Vector3.x = x;
        _Vector3.y = y;
        _Vector3.z = z;
        return _Vector3;
    }
    public Vector2 Vector2(float x, float y)
    {
        _Vector2.x = x;
        _Vector2.y = y;
        return _Vector2;
    }
    public Quaternion Quaternion(float x, float y, float z, float w)
    {
        _Quaternion.x = x;
        _Quaternion.y = y;
        _Quaternion.y = z;
        _Quaternion.y = w;
        return _Quaternion;
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
   
}

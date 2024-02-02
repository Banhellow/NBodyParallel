using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public float deltaTime;
    public float framesCount;
    void Start()
    {
        
    }
    
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        framesCount = 1.0f / deltaTime;
    }
    
    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(20, 20, w - 40, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        float msec = deltaTime * 1000.0f;
        float fpsRounded = Mathf.Round(framesCount);
        string text = $"{fpsRounded:0.} FPS ({msec:0.0} ms)";
        GUI.Label(rect, text, style);
    }
}

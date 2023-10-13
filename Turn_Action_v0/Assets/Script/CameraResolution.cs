using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraResolution : MonoBehaviour
{
    static readonly int cx = 1920, cy = 1080;

    private void Awake()
    {
        Application.targetFrameRate = 40;
    }

    void Start()
    {
        Screen.SetResolution(cx, cy, false);
        float scaleheight = ((float)Screen.width / Screen.height) / ((float)16 / 9); // (가로 / 세로) 
        int x = cx, y = cy;
        if ((cx / cy) > scaleheight) // Screen.width Screen.height
        {
            x = Screen.currentResolution.width - (Screen.width % 16) - 216;
            y = (x / 16) * 9;
        }
        else
        {
            y = Screen.currentResolution.height - (Screen.height % 9) - 108;
            x = (y / 9) * 16;
        }
        Screen.SetResolution(x, y, false);

        //public void SetUpCanvasScaler(int setWidth, int setHeight)
        //{
        CanvasScaler canvasScaler = FindObjectOfType<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(cx, cy);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        //}

        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;
        float scalewidth = 1f / scaleheight;
        if (scaleheight < 1)
        {
            rect.height = scaleheight;
            rect.y = (1f - scaleheight) / 2f;
        }
        else
        {
            rect.width = scalewidth;
            rect.x = (1f - scalewidth) / 2f;
        }
        camera.rect = rect;

        //if(camera.orthographic)
        //    camera.orthographicSize = camera.orthographicSize * scaleheight;


    }

    void OnPreCull() => GL.Clear(true, true, Color.black);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateGif : MonoBehaviour
{
    public Texture2D[] frames;
    private float framesPerSecond = 12.5f;
    private RawImage im = null;
    private Renderer rend = null;
    private void Awake()
    {
        im = GetComponent<RawImage>();
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        float index = Time.time * framesPerSecond;
        index = index % frames.Length;
        if (rend != null)
        {
            rend.material.mainTexture = frames[(int)index];
        }
        else
        {
            im.texture = frames[(int)index];
        }
    }
}

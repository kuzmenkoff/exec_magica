using UnityEngine;
using UnityEngine.UI;

public class AnimateGif : MonoBehaviour
{
    public Texture2D[] frames;
    public RawImage backgroundImage;
    private float framesPerSecond = 12.5f;

    private void Update()
    {
        if (backgroundImage != null)
        {
            float index = Time.time * framesPerSecond;
            index = index % frames.Length;
            backgroundImage.texture = frames[(int)index];
        }
    }
}

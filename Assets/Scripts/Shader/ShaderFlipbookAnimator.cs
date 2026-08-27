using UnityEngine;

public class ShaderFlipbookAnimator : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    [SerializeField] private Texture2D[] frames;
    [SerializeField] private float frameRate = 4f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;

    [SerializeField] private string textureProperty = "_MainTex";

    private MaterialPropertyBlock propertyBlock;

    private int currentFrame;
    private float frameTimer;
    private bool isPlaying;

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        SetFrame(0);
    }

    void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }
    void Update()
    {
        if(!isPlaying || frames == null || frames.Length == 0)
        {
            return;
        }

        if (frameRate <= 0f)
        {
            return;
        }

        frameTimer += Time.deltaTime;

        float frameDuration = 1f / frameRate;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            AdvanceFrame();
        }
    }

    void AdvanceFrame()
    {
        currentFrame++;

        if(currentFrame >= frames.Length)
        {
            if(loop)
            {
                currentFrame = 0;
            } else
            {
                currentFrame = frames.Length - 1;
                Pause();
            }
        }

        ApplyFrame();
    }

    void ApplyFrame()
    {
        if (targetRenderer == null || frames.Length == 0)
        {
            return;
        }

        propertyBlock.Clear();
        targetRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetTexture(textureProperty, frames[currentFrame]);

         targetRenderer.SetPropertyBlock(propertyBlock);
    }

     public void Play()
    {
        if (frames == null || frames.Length == 0)
            return;

        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        frameTimer = 0f;

        ApplyFrame();
    }

    public void SetFrame(int frame)
    {
        if (frames == null || frames.Length == 0)
            return;

        currentFrame = Mathf.Clamp(frame, 0, frames.Length - 1);
        frameTimer = 0f;

        ApplyFrame();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}

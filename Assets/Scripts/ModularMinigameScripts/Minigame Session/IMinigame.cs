using System;

public interface IMinigame
{
    void Initialize(MinigameConfig config, Action onComplete);
}
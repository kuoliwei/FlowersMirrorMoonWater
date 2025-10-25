// VideoState.cs
using MirrorWater;

public class VideoState : IGameState
{
    private readonly GameFlowController controller;

    public VideoState(GameFlowController controller)
    {
        this.controller = controller;
    }

    public void Enter() => controller.PlayVideo();
    public void Update()
    {
        if (controller.IsVideoFinished)
            controller.ChangeToInteractive();
    }
    public void Exit() => controller.DisableInteraction();
}

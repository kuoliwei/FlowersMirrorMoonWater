// InteractiveState.cs
using MirrorWater;

public class InteractiveState : IGameState
{
    private readonly GameFlowController controller;

    public InteractiveState(GameFlowController controller)
    {
        this.controller = controller;
    }

    public void Enter() => controller.EnableInteraction();
    public void Update()
    {
        if (controller.IsInteractionFinished)
            controller.ChangeToVideo();
    }
    public void Exit() => controller.DisableInteraction();
}

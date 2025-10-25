using UnityEngine;

namespace MirrorWater
{
    public class GameStateMachine
    {
        private IGameState currentState;

        public void ChangeState(IGameState newState)
        {
            if (currentState == newState)
                return;

            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();

            Debug.Log($"[StateMachine] �����ܪ��A�G{newState.GetType().Name}");
        }

        public void Update()
        {
            currentState?.Update();
        }
    }
}

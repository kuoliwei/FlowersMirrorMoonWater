namespace MirrorWater
{
    public interface IGameState
    {
        void Enter();        // 狀態進入時
        void Update();       // 每幀更新
        void Exit();         // 狀態離開時
    }
}

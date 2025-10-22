using System;
using UnityEngine;

namespace DreamOfRedMansion.Core
{
    public class GameStateMachine
    {
        /// <summary>
        /// 目前遊戲狀態。預設為 Idle。
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Idle;

        /// <summary>
        /// 狀態變化事件。所有需要隨狀態更新的模組都應訂閱此事件。
        /// </summary>
        public event Action<GameState> OnStateChanged;

        /// <summary>
        /// 切換到新狀態。若狀態相同則不重複觸發。
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);

            Debug.Log($"[StateMachine] 狀態切換為：{newState}");
        }

        /// <summary>
        /// 啟動時主動廣播一次「目前狀態」，
        /// 讓所有已完成訂閱的模組在初始化階段就能收到 Idle。
        /// </summary>
        public void Bootstrap()
        {
            Debug.Log($"[StateMachine] 啟動初始化狀態：{CurrentState}");
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}

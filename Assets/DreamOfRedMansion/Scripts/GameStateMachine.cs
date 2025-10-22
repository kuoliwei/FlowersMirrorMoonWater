using System;
using UnityEngine;

namespace DreamOfRedMansion.Core
{
    public class GameStateMachine
    {
        /// <summary>
        /// �ثe�C�����A�C�w�]�� Idle�C
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Idle;

        /// <summary>
        /// ���A�ܤƨƥ�C�Ҧ��ݭn�H���A��s���Ҳճ����q�\���ƥ�C
        /// </summary>
        public event Action<GameState> OnStateChanged;

        /// <summary>
        /// ������s���A�C�Y���A�ۦP�h������Ĳ�o�C
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);

            Debug.Log($"[StateMachine] ���A�������G{newState}");
        }

        /// <summary>
        /// �ҰʮɥD�ʼs���@���u�ثe���A�v�A
        /// ���Ҧ��w�����q�\���Ҳզb��l�ƶ��q�N�ব�� Idle�C
        /// </summary>
        public void Bootstrap()
        {
            Debug.Log($"[StateMachine] �Ұʪ�l�ƪ��A�G{CurrentState}");
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}

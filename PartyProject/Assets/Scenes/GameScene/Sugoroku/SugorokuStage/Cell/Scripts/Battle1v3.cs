using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public class Battle1v3 : Cell
{
    private bool m_WinPlayer;
    private enum ActionState
    {
        MINIGAME_START,
        MINIGAME_PLAYING,
        MINIGAME_END,
        MOVE,
        END,
    }
    private Dictionary<ActionState, Action> m_Actions;
    private ActionState m_CurrentActionState;
    private void Start()
    {

        m_Actions = new Dictionary<ActionState, Action>
        {
            {ActionState.MINIGAME_START,Action_MiniGameStart },
            {ActionState.MINIGAME_PLAYING,Action_MiniGamePlaying },
            {ActionState.MINIGAME_END,Action_MiniGameEnd },
            {ActionState.MOVE,Action_Move },
            {ActionState.END,Action_End },
        };
        m_CurrentActionState = ActionState.MINIGAME_START;
    }

    override public void Play()
    {
        if (m_Actions.TryGetValue(m_CurrentActionState, out var action))
        {
            action.Invoke();
        }
    }

    private void Action_MiniGameStart()
    {
        m_CurrentActionState = ActionState.END;
        return;

        // ミニゲームのスタート
        if (m_Player.IsInput())
        {
            //ミニゲームセレクト
            var miniGame = MiniGameListManager.instance.GameTypeSelect(MiniGameManager.GameType.ONE_THREE).GetRandom<MiniGameBase>();

            List<MainPlayer> miniGamePlayers = new List<MainPlayer>(PlayerManager.instance.m_Characters);
            MainPlayer oneSidePlayer = miniGamePlayers[m_Player.GetPlayerNum()];
            // その要素が存在するか確認
            if (miniGamePlayers.Contains(oneSidePlayer))
            {
                // まず取り除く
                miniGamePlayers.Remove(oneSidePlayer);

                // 先頭に挿入
                miniGamePlayers.Insert(0, oneSidePlayer);
            }
            MiniGameManager.instance.MiniGameStart(miniGame, miniGamePlayers);

            m_CurrentActionState = ActionState.MINIGAME_PLAYING;

        }
    }
    private void Action_MiniGamePlaying()
    {
        if (MiniGameManager.instance.GetIsDesideRank() == false)
        {
            MiniGameManager.instance.MiniGameUpdate();
        }
        else
        {
            m_CurrentActionState = ActionState.MINIGAME_END;
        }
    }
    private void Action_MiniGameEnd()
    {
        if (m_Player.IsInput())
        {
            var rankList = MiniGameManager.instance.GetRankList();
            MiniGameManager.instance.MiniGameEnd();
            m_WinPlayer = (int)rankList[1].Type == m_Player.GetPlayerNum();
            m_CurrentActionState = ActionState.MOVE;
        }
        PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.RANK_SORT);
    }
    private void Action_Move()
    {
        CoroutineRunner.instance.RunOnce("battle1v3_Move", Move());
    }

    private IEnumerator Move()
    {
        if(m_WinPlayer)
        {
            m_Player.SetMoveCellNum(10);
            yield return m_Player.MoveCell();
        }
    }
    private void Action_End()
    { 
        m_Player.EndCellAction();
        m_CurrentActionState = ActionState.MINIGAME_START;
    }
}


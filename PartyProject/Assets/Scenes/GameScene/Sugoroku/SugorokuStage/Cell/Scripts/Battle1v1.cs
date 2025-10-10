using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static CameraManager;

public class Battle1v1 : Cell
{
    private int m_PlayerNum;
    private Sugoroku_PlayerBase m_Rival;
    private int m_RivalNum;
    private bool m_WinPlayer;
    private enum ActionState
    {
        START,
        DICE,
        MINIGAME,
        MOVE,
        END,
    }
    private Dictionary<ActionState, Action> m_Actions;
    private ActionState m_CurrentActionState;
    private void Start()
    {
        m_Rival = null;
        m_RivalNum = -1;
        m_Actions = new Dictionary<ActionState, Action>
        {
            {ActionState.START,Action_Start },
            {ActionState.DICE,Action_Dice },
            {ActionState.MINIGAME, Action_MiniGame},
            {ActionState.MOVE,Action_Move },
            {ActionState.END,Action_End },
        };
        m_CurrentActionState = ActionState.START;
    }

    override public void Play()
    {
        if (m_Actions.TryGetValue(m_CurrentActionState, out var action))
        {
            action.Invoke();
        }
    }

    private void Action_Start()
    {
        CoroutineRunner.instance.RunOnce("Battle1v1_CellStart", CellStart());
    }

    private IEnumerator CellStart()
    {
        yield return CellExplanation.instance.ShowFadeExplanation("ミニゲーム1v1");

        yield return CellExplanation.instance.ShowWaitExplanation(
            "サイコロで出たプレイヤーと1v1のバトル！\n\r勝ったプレイヤーは７マス進み、\n\r負けたプレイヤーは７マス戻るぞ！", m_Player.Player
            );
        m_CurrentActionState = ActionState.DICE;
    }

    private void Action_Dice()
    {
        CoroutineRunner.instance.RunOnce("Battle1v1_Dice", Dice());
    }

    private IEnumerator Dice()
    {
        m_PlayerNum = m_Player.GetPlayerNum();
        DiceController selectRivalDice = DiceManager.instance.GetDice(m_PlayerNum + 4, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        selectRivalDice.gameObject.SetActive(true);

        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
        yield return new WaitUntil(() => m_Player.IsInput());
        InputIconDisplayer.instance.HideAllIcon();
        selectRivalDice.RollDice();
        yield return new WaitUntil(() => selectRivalDice.GetIsRolling() == false);
        m_RivalNum = selectRivalDice.GetDiceNum();
        if (m_PlayerNum <= m_RivalNum) m_RivalNum++;
        m_Rival = SugorokuManager.instance.GetPlayer(m_RivalNum);
        yield return new WaitForSeconds(1.5f);
        Destroy(selectRivalDice.gameObject);
        m_CurrentActionState = ActionState.MINIGAME;
    }

    private void Action_MiniGame()
    {
        CoroutineRunner.instance.RunOnce("Battle1v1_Minigame", Minigame());
    }

    private IEnumerator Minigame()
    {
        SugorokuManager.instance.IsActiveSugorokuUI(false);

        //ミニゲームセレクト
        var miniGame = MiniGameListManager.instance.GameTypeSelect(MiniGameManager.GameType.ONE_ONE).GetRandom<MiniGameBase>();
        List<MainPlayer> miniGamePlayers = new();
        miniGamePlayers.Add(PlayerManager.instance.m_Characters[m_PlayerNum]);
        miniGamePlayers.Add(PlayerManager.instance.m_Characters[m_RivalNum]);
        MiniGameManager.instance.MiniGameStart(miniGame, miniGamePlayers);

        // ミニゲームプレイループ
        while (!MiniGameManager.instance.GetIsDesideRank())
        {
            MiniGameManager.instance.MiniGameUpdate();
            yield return null;
        }

        var rankList = MiniGameManager.instance.GetRankList();
        m_WinPlayer = (int)rankList[1].Type == m_PlayerNum;
        MiniGameManager.instance.MiniGameEnd();

        yield return new WaitForSeconds(1.5f);

        CameraManager.instance.SwitchToSugorokuCam((SugorokuCameraType)m_Player.Player.Type);
        SugorokuManager.instance.IsActiveSugorokuUI(true);

        m_CurrentActionState = ActionState.MOVE;
    }

    private void Action_Move()
    {
        PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.RANK_SORT);
        CoroutineRunner.instance.RunOnce("battle1v1_Move", Move());
    }

    private IEnumerator Move()
    {
        int playerMove = 7;
        int rivalMove = 7;
        if (m_WinPlayer)
        {
            rivalMove *= -1;
        }
        else
        {
            playerMove *= -1;
        }
        m_Player.SetMoveCellNum(playerMove);
        m_Rival.SetMoveCellNum(rivalMove);
        yield return m_Player.MoveCell();

        yield return new WaitForSeconds(1.5f);
        CameraManager.instance.SwitchToSugorokuCam((SugorokuCameraType)m_Rival.Player.Type);

        yield return m_Rival.MoveCell();
        yield return new WaitForSeconds(1.0f);

        m_CurrentActionState = ActionState.END;
    }
    private void Action_End()
    {
        m_PlayerNum = -1;
        m_Rival = null;
        m_RivalNum = -1;
        m_Player.EndCellAction();
        m_CurrentActionState = ActionState.START;
    }
}


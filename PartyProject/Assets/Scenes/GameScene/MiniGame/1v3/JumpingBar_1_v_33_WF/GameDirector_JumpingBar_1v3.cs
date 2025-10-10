using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector_JumpingBar_1v3 : MiniGameBase
{
    public enum MiniGameResult
    {
        None,
        BarWin,
        JumperWin
    }

    private MiniGameResult m_GameResult;

    [SerializeField] private List<PlayerBase_JumpingBar_1v3> m_Players;
    [SerializeField] private List<Player_JumpingBar_1v3> m_AroundPlayers;
    [SerializeField] private Bar_JumpingBar_1v3 m_CenterPlayer;
    [SerializeField] private Text m_TimeText;
    private float m_TimeLimit;
    private int m_Bar = 0;

    private bool m_GameFinished;

    private void Start()
    {
        m_GameResult = MiniGameResult.None;
        m_TimeLimit = 10.0f;
        m_GameFinished = false;

        AssignPlayers();

        foreach (var player in m_Players)
        {
            player.GameStart();
        }
    }

    void Update()
    {
        if(m_GameFinished) return;

        m_TimeLimit -= Time.deltaTime;

        if(m_TimeLimit <= 0)
        {
            m_TimeLimit = 0;
        }

        m_TimeText.text = $"残り{m_TimeLimit:F1}秒";

        foreach (var player in m_Players)
        {
            player.GameUpdate();
        }

        CheckGameResult();
    }

    private void AssignPlayers()
    {
        m_Players.Clear();
        for (int i = 0; i < 4; i++)
        {
            if (i == m_Bar)
            {
                m_Players.Add(m_CenterPlayer);
            }
            else
            {
                // m_Bar の位置を飛ばして、周囲のプレイヤーを割り当て
                m_Players.Add(m_AroundPlayers[(i < m_Bar) ? i : i - 1]);
            }

            if (m_CenterPlayer == null)
            {
                Debug.LogWarning("CenterPlayerが設定されていません");
                return;
            }

            if (m_AroundPlayers == null || m_AroundPlayers.Count < 3)
            {
                Debug.LogWarning("AroundPlayersが設定されていません");
                return;
            }
        }
    }

    private void CheckGameResult()
    {
        // バー側が倒された3人を全滅させたか
        int aliveAroundCount = 0;
        foreach (var aroundPlayer in m_AroundPlayers)
        {
            if (aroundPlayer.IsAlive)
                aliveAroundCount++;
        }

        if (aliveAroundCount == 0)
        {
            GameEnd(MiniGameResult.BarWin);
            return;
        }

        // 30秒経過して、まだ3人側が残っているか
        if (m_TimeLimit == 0)
        {
            GameEnd(MiniGameResult.JumperWin);
        }
    }

    private void GameEnd(MiniGameResult result)
    {
        m_GameFinished = true;
        m_GameResult = result;
        Debug.Log($"【試合終了】{result}");
        switch (result)
        {
            case MiniGameResult.BarWin:
                Debug.Log("バー側の勝利！");
                break;
            case MiniGameResult.JumperWin:
                Debug.Log("ジャンプ側の勝利！");
                break;
        }
    }
}

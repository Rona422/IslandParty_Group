using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector_MeteorPanic : MiniGameBase
{
    enum RoundState
    {
        START,
        COUNTDOWN,
        METEOR,
        JUDGE,
        END
    }

    [SerializeField] private Text m_TimeUIText;
    [SerializeField] private GameObject m_BlueFloor;
    [SerializeField] private GameObject m_RedFloor;
    [SerializeField] private MeteorController_MeteorPanic m_MeteorPrefab;
    private bool m_IsRed;
    private int m_RoundCount;
    private float m_CurrentCountDownTime;
    private float m_CountDownTime;
    private RoundState m_CurrentRoundState;
    private Dictionary<RoundState,Action> m_RoundActions = new();
    private float m_CenterX;
    private int m_AliveCount;
    private int m_RoundMax;
    private float m_JudgeWaitTime;
    private bool m_IsFinish;

    private void Start()
    {
        m_RoundActions = new Dictionary<RoundState, Action>
        {
            {RoundState.START, RoundStart },
            {RoundState.COUNTDOWN, CountDown },
            {RoundState.METEOR, FallMeteor },
            {RoundState.JUDGE, Judge },
            {RoundState.END, RoundEnd },
        };
        m_IsRed = true;
        m_RoundCount = 1;
        m_CurrentCountDownTime = 0.0f;
        m_CountDownTime = 3.0f;
        m_CenterX = (m_BlueFloor.transform.position.x + m_RedFloor.transform.position.x) / 2;
        m_AliveCount = m_MiniGamePlayers.Count;
        m_RoundMax = 1;
        m_JudgeWaitTime = 3.0f;
        m_IsFinish = false;
        
        m_CurrentRoundState = RoundState.START;
    }

    void Update()
    {
        if(m_IsFinish)
        {
            return;
        }
        m_RoundActions[m_CurrentRoundState]();
    }

    private IEnumerator FinishCoroutine()
    {
        yield return MiniGameManager.instance.ShowFinishText();
        // 最後にミニゲーム終了
        base.Finish();
        Debug.Log("メテオフィニッシュ");
    }
    private void RoundStart()
    {
        foreach (var playerBase in m_MiniGamePlayers)
        {
            var playerMP = playerBase.GetComponent<Player_MeteorPanic>();
            if (playerMP != null && playerMP.IsAlive)
            {
                playerMP.SetCanMove(true);
            }
        }

        m_IsRed = UnityEngine.Random.Range(0, 2) == 0;
        m_CurrentCountDownTime = m_CountDownTime;
        m_TimeUIText.text = Mathf.CeilToInt(m_CurrentCountDownTime).ToString();
        m_TimeUIText.gameObject.SetActive(true);

        m_CurrentRoundState = RoundState.COUNTDOWN;
    }

    private void CountDown()
    {
        m_CurrentCountDownTime -= Time.deltaTime;

        // テキスト更新
        m_TimeUIText.text = Mathf.CeilToInt(m_CurrentCountDownTime).ToString();

        if (m_CurrentCountDownTime < 0.0f)
        {
            foreach (var playerBase in m_MiniGamePlayers)
            {
                var playerMP = playerBase.GetComponent<Player_MeteorPanic>();
                if (playerMP != null && playerMP.IsAlive)
                {
                    playerMP.SetCanMove(false);
                }
            }

            m_CurrentCountDownTime = 0.0f;
            m_TimeUIText.gameObject.SetActive(false);

            m_CurrentRoundState = RoundState.METEOR;
        }
    }

    private void FallMeteor()
    {
        var targetFloor = m_IsRed ? m_RedFloor : m_BlueFloor;
        Instantiate(m_MeteorPrefab, targetFloor.transform.position + new Vector3(0.0f, 20.0f, 0.0f), Quaternion.identity);
        m_CurrentRoundState = RoundState.JUDGE;
    }

    private void Judge()
    {
        CoroutineRunner.instance.DelayedCallOnce("PlayerJudge", m_JudgeWaitTime, PlayerJudge);
    }

    private void PlayerJudge()
    {
        int deadCount = 0;
        foreach(var playerBase in m_MiniGamePlayers)
        {
            var playerMP = playerBase.GetComponent<Player_MeteorPanic>();
            if (playerMP != null && playerMP.IsAlive)
            {
                if((playerMP.transform.position.x < m_CenterX) == m_IsRed)
                {
                    playerMP.TakeDamage();
                    if(playerMP.IsAlive == false)
                    {
                        // それぞれのPlayer.rankに、脱落順に0,1,2,3と入れていく
                        // 値の多いほうが順位が高い
                        playerBase.Player.rank = m_MiniGamePlayers.Count - (m_AliveCount);
                        deadCount++;
                    }
                }
            }
        }
        m_AliveCount -= deadCount;
        m_CurrentRoundState = RoundState.END;
    }

    private void RoundEnd()
    {
        if(m_AliveCount <= 1)
        {
            GameSet();
            return;
        }
        m_RoundCount++;
        if (m_RoundCount > m_RoundMax)
        {
            GameSet();
            return;
        }
        m_CurrentRoundState = RoundState.START;
    }


    private void GameSet()
    {
        // 生き残りプレイヤーを収集
        List<PlayerBase> survivors = new();
        foreach (var playerBase in m_MiniGamePlayers)
        {
            var playerMP = playerBase.GetComponent<Player_MeteorPanic>();
            if (playerMP != null && playerMP.IsAlive)
            {
                survivors.Add(playerBase);
            }
        }

        // ライフの多い順に並べる（同じライフ数なら順不同）
        survivors.Sort((a, b) =>
        {
            var pA = a.GetComponent<Player_MeteorPanic>();
            var pB = b.GetComponent<Player_MeteorPanic>();
            return pB.Life.CompareTo(pA.Life); // ライフが多い順
        });

        // 順位の開始（すでに rank を付けた脱落者数から）
        int baseRank = m_MiniGamePlayers.Count - m_AliveCount;

        // 同ライフ → 同順位
        int currentRank = baseRank;
        int sameLifeCount = 0;
        int? lastLife = null;

        foreach (var survivor in survivors)
        {
            var playerMP = survivor.GetComponent<Player_MeteorPanic>();
            int life = playerMP.Life;

            // 前のプレイヤーとライフが違う → 順位を更新
            if (lastLife == null || life != lastLife)
            {
                currentRank += sameLifeCount;
                sameLifeCount = 1;
                lastLife = life;
            }
            else
            {
                sameLifeCount++; // 同じライフ → 順位はそのまま
            }

            survivor.Player.rank = currentRank;
        }
        m_IsFinish = true;
        CoroutineRunner.instance.RunCoroutine(FinishCoroutine());
    }
}

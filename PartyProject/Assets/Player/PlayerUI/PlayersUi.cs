using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Event_Sugoroku;
public class PlayersUi : SingletonMonoBehaviour<PlayersUi>
{
    [SerializeField]
    private GameObject m_RankSortImage;
    [System.Serializable]
    public class PlayerUiStatus
    {
        public GameObject m_PlayersUiObject;
        [System.NonSerialized]
        public PlayerUi m_PlayerUi;
        [System.NonSerialized]
        public RectTransform m_Rect;
        [System.NonSerialized]
        public Image m_Image;
    }
    [SerializeField]
    private List<PlayerUiStatus> m_PlayersUi = new();
    public enum PlayersUiState
    {
        STANDBY,
        TITLESCENE,
        MINIGAME_START,
        RANK_SORT,
    }
    private readonly Dictionary<PlayersUiState, Func<IEnumerator>> m_Actions = new();
    private PlayersUiState m_CurrentPayersUiState;
    private bool m_CurrentIsHorizontal = true;
    private bool IsHorizontal { get { return m_CurrentIsHorizontal; } }
    private List<Vector2> m_HorizontalPos = new();
    private List<Vector2> m_VerticalPos = new();
    protected override void Awake()
    {
        base.Awake();
        m_Actions[PlayersUiState.STANDBY] = SetStandby;
        m_Actions[PlayersUiState.TITLESCENE] = SetTitle;
        m_Actions[PlayersUiState.RANK_SORT] = SetRank;
        m_Actions[PlayersUiState.MINIGAME_START] = SetMiniGame;
    }
    private void Start()
    {
        //PlayerManagerのとの整合性
        if (PlayerManager.instance.m_Characters.Count != m_PlayersUi.Count)
        {
            Debug.LogError("PlayerUi分キャラクターが生成されていません");
        }
        //ObjectからComponentを引っ張って設定
        for (int i = 0; i < m_PlayersUi.Count; i++)
        {
            var UiObject = m_PlayersUi[i].m_PlayersUiObject;
            m_PlayersUi[i].m_PlayerUi = UiObject.GetComponent<PlayerUi>();
            m_PlayersUi[i].m_Rect = UiObject.GetComponent<RectTransform>();
            m_PlayersUi[i].m_Image = UiObject.GetComponent<Image>();
            //ついでに縦横状態の各位置も
            m_HorizontalPos.Add(new Vector2(-165 + 110 * i, 0.0f));
            m_VerticalPos.Add(new Vector2(0.0f, -165 + 110 * i));
        }
        //PlayerUiにMainPlayerをアタッチ
        MiniGameBase.SetPlayer(m_PlayersUi.Select(x => x.m_PlayerUi).OfType<PlayerBase>().ToList(), PlayerManager.instance.m_Characters);
        SetColor();
    }
    private void Update()
    {
        m_RankSortImage.SetActive(m_CurrentPayersUiState == PlayersUiState.RANK_SORT);
    }
    public IEnumerator StateReflection(PlayersUiState _State)
    {
        m_CurrentPayersUiState = _State;
        if (m_Actions.TryGetValue(m_CurrentPayersUiState, out var action))
        {
            yield return CoroutineRunner.instance.RunCoroutine(action.Invoke());
        }
    }
    private IEnumerator SetStandby()
    {
        //位置適用
        PositionReflection(false);
        //座標適用
        SetPos(new Vector2(940.0f, 300.0f));
        //サイズ適用
        SetScale(new Vector3(1.0f, 1.0f, 0.0f));
        yield break;
    }
    private IEnumerator SetTitle()
    {
        //位置適用
        PositionReflection(true);
        //座標適用
        SetPos(new Vector3(0.0f, -250.0f, 0.0f));
        //サイズ適用
        SetScale(new Vector3(2.0f, 2.0f, 0.0f));
        yield return new WaitForSeconds(0.1f);
    }
    private IEnumerator SetRank()
    {
        Dictionary<int, int> Ranks = SugorokuManager.instance.RankDictionary;
        // m_PlayersUi を Rank の昇順に並べ替える
        List<PlayerUiStatus> NewPlayersUi = new();
        for (int i = 0; i < Ranks.Count; i++)
        {
            for (int j = 0; j < m_PlayersUi.Count; j++)
            {
                if ((int)(m_PlayersUi[j].m_PlayerUi.Player.Type) == Ranks[i + 1])
                {
                    NewPlayersUi.Add(m_PlayersUi[j]);
                    break;
                }
            }
        }
        m_PlayersUi = NewPlayersUi;
        //位置適用
        PositionReflection(true);
        //座標適用
        SetPos(new Vector3(600.0f, 400.0f, 0.0f));
        //サイズ適用
        SetScale(new Vector3(1.5f, 1.5f, 0.0f));
        yield return new WaitForSeconds(0.1f);
    }
    private IEnumerator SetMiniGame()
    {
        //位置適用
        PositionReflection(true);
        //座標適用
        SetPos(new Vector3(550.0f, -400.0f, 0.0f));
        //サイズ適用
        SetScale(new Vector3(1.5f, 1.5f, 0.0f));
        yield break;
    }
    //座標設定
    private void SetPos(Vector3 _TargetPos)
    {
        RectTransform ThisRectTransform = GetComponent<RectTransform>();
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector2>(
                v => ThisRectTransform.anchoredPosition = v,
                ThisRectTransform.anchoredPosition,
                _TargetPos,
                0.1f,
                Vector2.Lerp
            )
        );
    }
    //スケール設定
    private void SetScale(Vector3 _Scale)
    {
        RectTransform ThisRectTransform = GetComponent<RectTransform>();
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>(
                v => ThisRectTransform.transform.localScale = v,
                ThisRectTransform.localScale,
                _Scale,
                0.1f,
                Vector3.Lerp
            )
        );
    }
    //色更新
    private void SetColor()
    {
        foreach (var PlayerUi in m_PlayersUi.Select(x => x.m_PlayerUi).ToList())
        {
            PlayerUi.SetColor(PlayerUi.Player.SkinColor * PlayerUi.StandbyColor);
        }
    }
    //各UIポジション適応
    private void PositionReflection(bool _IsHorizontal)
    {
        m_CurrentIsHorizontal = _IsHorizontal;
        for (int i = 0; i < m_PlayersUi.Count; i++)
        {
            var ui = m_PlayersUi[i];
            Vector2 TargetPos = (m_CurrentIsHorizontal) ? m_HorizontalPos[i] : m_VerticalPos[i];
            Debug.Log(i + " lk;zsdf " + TargetPos);
            Debug.Log(m_PlayersUi[i].m_PlayerUi.Player.Type);

            CoroutineRunner.instance.RunCoroutine(
                CoroutineRunner.instance.LerpValue<Vector2>(
                    v => ui.m_Rect.anchoredPosition = v,
                    ui.m_Rect.anchoredPosition,
                    TargetPos,
                    1.0f,
                    Vector2.Lerp
                )
            );
        }
    }
    //参加&脱退
    public void JoinLeftPlayer(PlayerManager.CharacterType _Type)
    {
        var JoinPlayer = m_PlayersUi[(int)_Type];
        Vector2 JoinIconMoveVec;
        {
            RectTransform ThisPosition = GetComponent<RectTransform>();
            if (m_CurrentIsHorizontal)
            {
                JoinIconMoveVec = (ThisPosition.anchoredPosition.y < 0) ? new Vector2(0.0f, 1.0f) : new Vector2(0.0f, -1.0f);
            }
            else
            {
                JoinIconMoveVec = (ThisPosition.anchoredPosition.x < 0) ? new Vector2(1.0f, 0.0f) : new Vector2(-1.0f, 0.0f);
            }
        }
        CoroutineRunner.instance.
                RunCoroutine(JoinPlayer.m_PlayerUi.SetIcon(JoinIconMoveVec));
    }
    //全員準備完了まで
    public IEnumerator IsAllCheck()
    {
        //全員チェックするまで待機
        for (; ; )
        {
            //チェックの更新を回してやる
            foreach (var playersUi in m_PlayersUi.Select(x => x.m_PlayerUi))
            {
                playersUi.IsCheckCheck();
            }
            //全員チェック状態か確認
            if (m_PlayersUi.All(x => x.m_PlayerUi.IsCheck))
            {
                break;
            }
            yield return null;
        }
        AudioManager.instance.PlaySE("Player", "OllCheck");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "Player", "OllCheck").length);
        //チェックを消し処理終了
        foreach (var playersUi in m_PlayersUi.Select(x => x.m_PlayerUi))
        {
            playersUi.CheckHyde();
        }
    }
}

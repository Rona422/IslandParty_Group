using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static CameraManager;
using Image = UnityEngine.UI.Image;

public class SugorokuManager : SingletonMonoBehaviour<SugorokuManager>
{
    [SerializeField] private GameObject m_PlayerBase;
    private List<Sugoroku_PlayerBase> m_Players = new();
    public List<Sugoroku_PlayerBase> GetPlayers() { return m_Players; }
    private enum SugorokuState
    {
        START,
        PLAY,
        RESULT,
    }
    private Dictionary<SugorokuState, Action> m_SugorokuStateActions;
    private SugorokuState m_CurrentSugorokuState;
    private bool m_IsPlaying;

    private Sugoroku_Tutorial m_Sugoroku_Tutorial;

    private int m_TurnCount;
    private int m_NowPlayer;
    private Dictionary<int, int> m_RankDictionary = new(); // <�����N(1~4),�v���C���[�ԍ�(0~3)>
    public Dictionary<int, int> RankDictionary { get { return m_RankDictionary; } }

    [SerializeField] private GameObject m_SugorokuUI;
    [Header("Text�̏���\n\rstateMessage,move,remainingNum")]
    [SerializeField] private List<Text> m_Texts = new();
    private enum TextKind : int
    {
        STATE_MESSAGE,
        MOVE,
        REMAINING,
    }
    [SerializeField] private Image m_RemainingImage;
    [SerializeField] private Image m_StateMessageImage;
    protected override void Awake()
    {
        base.Awake();
        m_SugorokuStateActions = new Dictionary<SugorokuState, Action>
        {
            {SugorokuState.START, Sugoroku_Tutorial },
            {SugorokuState.PLAY, Sugoroku_Play },
            {SugorokuState.RESULT, Sugoroku_Result },
        };
        m_CurrentSugorokuState = SugorokuState.START;
        for (int i = 0; i < PlayerManager.PlayerMax; i++)
            m_RankDictionary[i + 1] = i;
    }
    // Start is called before the first frame update
    void Start()
    {
        // �v���C���[�̐ݒ�
        {
            float offset = 0.25f;
            List<Vector3> cellOfsetPos = new()
            {
                new Vector3(-offset, 10f, +offset), // ����
                new Vector3(+offset, 10f, +offset), // �E��
                new Vector3(-offset, 10f, -offset), // ����
                new Vector3(+offset, 10f, -offset)  // �E��
            };
            for (int i = 0; i < PlayerManager.PlayerMax; i++)
            {
                GameObject _Obj = Instantiate(m_PlayerBase,gameObject.transform);
                _Obj.name = "Sugoroku_Player_" + (i + 1).ToString();
                _Obj.SetActive(true);
                Sugoroku_PlayerBase player;
                player = _Obj.AddComponent<Sugoroku_PlayerBase>();
                player.SetOffsetPos(cellOfsetPos[i]);

                player.SetPlayerNumber(i);
                m_Players.Add(player);
            }
            MiniGameBase.SetPlayer(m_Players.Cast<PlayerBase>().ToList(), PlayerManager.instance.m_Characters);
            MiniGameBase.ChangeColor(m_Players.Cast<PlayerBase>().ToList(), PlayerManager.instance.m_Characters);
        }

        m_Sugoroku_Tutorial = GetComponent<Sugoroku_Tutorial>();

        m_TurnCount = 0;
        m_NowPlayer = 0;
        foreach (var text in m_Texts)
        {
            text.gameObject.SetActive(false);
        }
        m_RemainingImage.gameObject.SetActive(false);

        CameraManager.instance.SetPlayerTransform(m_Players);
        CameraManager.instance.SwitchToSugorokuCam(SugorokuCameraType.STAGE);
    }
    // Update is called once per frame
    void Update()
    {
        if (m_SugorokuStateActions.TryGetValue(m_CurrentSugorokuState, out var action))
        {
            action.Invoke();
        }
    }
    private void Sugoroku_Tutorial()
    {
        m_Sugoroku_Tutorial.Tutorial_Update();
        if (m_Sugoroku_Tutorial.SugorokuStart())
        {
            Debug.Log("start");
            m_CurrentSugorokuState++;
        }
    }
    private void Sugoroku_Play()
    {
        CoroutineRunner.instance.RunOnce("PlayFlowCoroutine", PlayFlowCoroutine());
    }
    private void Sugoroku_Result()
    {
        Debug.Log("result");
    }
    private IEnumerator PlayFlowCoroutine()
    {
        m_IsPlaying = true;
        while (true)
        {
            yield return TurnEvent_Start_Coroutine();

            yield return TurnEvent_Minigame_Coroutine();

            yield return TurnEvent_Players_Coroutine();

            if (m_IsPlaying == false)
            {
                m_CurrentSugorokuState = SugorokuState.RESULT;
                yield break;
            }
        }
    }

    private IEnumerator TurnEvent_Start_Coroutine()
    {
        m_TurnCount++;

        yield return ShowTextFade(m_Texts[(int)TextKind.STATE_MESSAGE], $"�^�[�� {m_TurnCount}", m_StateMessageImage);

        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator TurnEvent_Minigame_Coroutine()
    {
        yield return ShowTextFade(m_Texts[(int)TextKind.STATE_MESSAGE], "���Ԍ��߃Q�[��", m_StateMessageImage);

        var miniGame = MiniGameListManager.instance
            .GameTypeSelect(MiniGameManager.GameType.ONE_ALL)
            .GetRandom<MiniGameBase>();

        MiniGameManager.instance.MiniGameStart(miniGame, PlayerManager.instance.m_Characters);

        // �~�j�Q�[���v���C���[�v
        while (!MiniGameManager.instance.GetIsDesideRank())
        {
            MiniGameManager.instance.MiniGameUpdate();
            yield return null;
        }

        // �~�j�Q�[���I������
        var rankList = MiniGameManager.instance.GetRankList();

        foreach (var rank in rankList)
        {
            m_RankDictionary[rank.Key] = (int)rank.Value.Type;
        }

        for (int i = 1; i < 5; i++)
        {
            m_Players[m_RankDictionary[i]].SetRank(i);
        }
        MiniGameManager.instance.MiniGameEnd();

        CameraManager.instance.SwitchToSugorokuCam(
            (SugorokuCameraType)m_Players[m_RankDictionary[m_NowPlayer + 1]].Player.Type);

        yield return new WaitForSeconds(1.0f);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.RANK_SORT));

    }

    private IEnumerator TurnEvent_Players_Coroutine()
    {
        Text messageText = m_Texts[(int)TextKind.STATE_MESSAGE];
        Text remainingText = m_Texts[(int)TextKind.REMAINING];
        Text moveText = m_Texts[(int)TextKind.MOVE];

        var currentPlayer = m_Players[m_RankDictionary[m_NowPlayer + 1]];

        // UI���v���C���[�̐F�ɐݒ�
        m_StateMessageImage.color = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);
        m_RemainingImage.color = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);
        m_RemainingImage.color = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);
        moveText.GetComponent<Outline>().effectColor = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);

        // ����v���C���[�̃^�[���J�n�t�F�[�h�\��
        yield return ShowTextFade(messageText, $"�v���C���[{m_RankDictionary[m_NowPlayer + 1] + 1}", m_StateMessageImage);
        remainingText.gameObject.SetActive(true);
        moveText.gameObject.SetActive(true);
        m_RemainingImage.gameObject.SetActive(true);
        MinimapUI.instance.ShowMinimap();



        currentPlayer.TurnStart();

        while (true)
        {
            // �v���C���[��TurnProcess�𖈃t���[���Ă�
            currentPlayer.TurnProcess();

            if (m_IsPlaying == false)
            {
                yield break;
            }

            if (currentPlayer.IsTurnEnd())
            {
                m_NowPlayer++;

                if (m_NowPlayer == 4)
                {
                    m_NowPlayer = 0;
                    m_StateMessageImage.color = Color.white.WithHSVA(a: 0.8f);
                    CameraManager.instance.SwitchToSugorokuCam(SugorokuCameraType.STAGE);
                    break; // ���[�v�����ă^�[���I��
                }

                currentPlayer = m_Players[m_RankDictionary[m_NowPlayer + 1]];

                currentPlayer.TurnStart();

                remainingText.gameObject.SetActive(false);
                moveText.gameObject.SetActive(false);
                m_RemainingImage.gameObject.SetActive(false);

                CameraManager.instance.SwitchToSugorokuCam(
                    (SugorokuCameraType)currentPlayer.Player.Type);

                MinimapUI.instance.HideMinimap();

                yield return new WaitForSeconds(1.0f);

                // UI���v���C���[�̐F�ɐݒ�
                m_StateMessageImage.color = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);
                m_RemainingImage.color = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);
                moveText.GetComponent<Outline>().effectColor = currentPlayer.Player.SkinColor.WithHSVA(s: 0.6f);

                // ���v���C���[�̃^�[���J�n�t�F�[�h�\��
                yield return ShowTextFade(messageText, $"�v���C���[{m_RankDictionary[m_NowPlayer + 1] + 1}", m_StateMessageImage);

                remainingText.gameObject.SetActive(true);
                moveText.gameObject.SetActive(true);
                m_RemainingImage.gameObject.SetActive(true);

                MinimapUI.instance.ShowMinimap();
            }
            remainingText.text =
                (SugorokuStage.instance.GetCellMax() - currentPlayer.GetCurrentCellNum()).ToString();

            int moveNum = currentPlayer.GetMoveCellNum();
            moveText.text = moveNum != 0 ? moveNum.ToString() : "";

            yield return null;
        }

        remainingText.gameObject.SetActive(false);
        moveText.gameObject.SetActive(false);
        m_RemainingImage.gameObject.SetActive(false);

        MinimapUI.instance.HideMinimap();
    }

    public Sugoroku_PlayerBase GetPlayer(int _playerNum)
    {
        return m_Players[_playerNum];
    }


    public Sugoroku_PlayerBase GetOtherPlayer(int n, Sugoroku_PlayerBase basePlayer)
    {
        int i;
        for (i = 0; i < 4; i++)
        {
            if (m_Players[i].Equals(basePlayer))
            {
                break;
            }
        }
        if (i <= n) n++;
        return m_Players[n];
    }
    public void FinishSugoroku()
    {
        m_IsPlaying = false;
    }

    public void IsActiveSugorokuUI(bool isActive)
    {
        m_SugorokuUI.gameObject.SetActive(isActive);
    }

    public IEnumerator ShowTextFade(Text targetText, string message, Image targetImage = null, float fadeDuration = 0.25f, float displayTime = 1.5f)
    {
        // �e�L�X�g�Z�b�g�A�b�v
        targetText.text = message;
        Color textColor = targetText.color;
        float textOriginalAlpha = textColor.a; // ���̃A���t�@�l
        targetText.color = textColor.WithHSVA(a: 0f); // �A���t�@��0��
        targetText.gameObject.SetActive(true);

        // �摜�Z�b�g�A�b�v�i����ꍇ�j
        Color imageColor = Color.white;
        float imageOriginalAlpha = 1f; // �f�t�H���g�͕s����
        if (targetImage != null)
        {
            imageColor = targetImage.color;
            imageOriginalAlpha = imageColor.a; // ���̃A���t�@�l��ۑ�
            targetImage.color = imageColor.WithHSVA(a: 0f); // �A���t�@��0��
            targetImage.gameObject.SetActive(true);
        }

        // �t�F�[�h�C��
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeDuration;

            targetText.color = textColor.WithHSVA(a: Mathf.Lerp(0f, textOriginalAlpha, progress));

            if (targetImage != null)
            {
                targetImage.color = imageColor.WithHSVA(a: Mathf.Lerp(0f, imageOriginalAlpha, progress));
            }

            yield return null;
        }

        // �ŏI�l�␳
        targetText.color = textColor.WithHSVA(a: textOriginalAlpha);
        if (targetImage != null)
        {
            targetImage.color = imageColor.WithHSVA(a: imageOriginalAlpha);
        }

        // �\���ێ�
        yield return new WaitForSeconds(displayTime);

        // �t�F�[�h�A�E�g
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeDuration;

            targetText.color = textColor.WithHSVA(a: Mathf.Lerp(textOriginalAlpha, 0f, progress));

            if (targetImage != null)
            {
                targetImage.color = imageColor.WithHSVA(a: Mathf.Lerp(imageOriginalAlpha, 0f, progress));
            }

            yield return null;
        }

        // �t�F�[�h�A�E�g��ɔ�\��
        targetText.color = textColor.WithHSVA(a: textOriginalAlpha);
        targetText.gameObject.SetActive(false);

        if (targetImage != null)
        {
            targetImage.color = imageColor.WithHSVA(a: imageOriginalAlpha);
            targetImage.gameObject.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class Sugoroku_Tutorial : MonoBehaviour
{
    // チュートリアルの状態管理用の列挙型
    private enum State
    {
        START,      // チュートリアル開始前（初期状態）
        ASK,        // チュートリアルを行うか確認する状態
        TUTORIAL,   // チュートリアルスライドを表示する状態
        GAMESTART,  // チュートリアル終了、すごろくゲーム開始状態
    }

    [SerializeField] private GameObject m_AskUI; // 質問時のUI

    [SerializeField] private List<Sprite> m_TutorialSlides; // チュートリアル用のスライド画像リスト
    [SerializeField] private Image m_SlideImage;           // 現在表示中のスライド用Image
    private const float m_FadeDuration = 0.5f;  // フェードイン/アウトの時間
    [SerializeField] private GameObject m_ArrowW;
    [SerializeField] private GameObject m_ArrowE;

    private int m_CurrentSlideIndex;   // 現在表示しているスライドのインデックス
    private State m_CurrentState;      // 現在のチュートリアル状態
    private MainPlayer m_Player;       // チュートリアル対象のプレイヤー
    private bool m_IsFading;           // スライドのフェード中かどうか

    private bool m_SugorokuStart;      // すごろくゲームが開始したかどうか

    // 初期化処理
    private void Start()
    {
        m_AskUI.SetActive(false);
        m_ArrowW.SetActive(false);
        m_ArrowE.SetActive(false);
        m_SlideImage.gameObject.SetActive(false); // スライド画像は非表示で開始
        m_CurrentSlideIndex = 0;                  // 最初のスライドを設定
        m_CurrentState = State.START;             // 初期状態はSTART
        m_IsFading = false;                       // フェード中フラグ初期化
        m_SugorokuStart = false;                  // ゲーム開始フラグ初期化
    }

    // チュートリアルの更新処理（Updateからではなく外部から呼ぶ想定）
    public void Tutorial_Update()
    {
        switch (m_CurrentState)
        {
            case State.START:
                // プレイヤー取得
                m_Player = SugorokuManager.instance.GetPlayers()
                    .FirstOrDefault(p => !p.Player.IsNpc)?.Player;
                // 次の状態へ
                m_CurrentState = State.ASK;
                // ASKのUIを表示
                m_AskUI.gameObject.SetActive(true);
                InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.TUTORIAL_ASK_YES, m_Player);
                InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.B, InputIconDisplayer.PositionType.TUTORIAL_ASK_NO, m_Player);

                break;

            case State.ASK:
                // チュートリアル実行確認の入力処理
                HandleAskTutorial();
                break;

            case State.TUTORIAL:
                // 実際のチュートリアルスライド操作
                HandleTutorial();
                break;

            case State.GAMESTART:
                // チュートリアル終了、ゲーム開始準備
                m_SlideImage.enabled = false;
                m_SugorokuStart = true;
                break;
        }
    }

    // ゲーム開始フラグ取得
    public bool SugorokuStart()
    {
        return m_SugorokuStart;
    }

    // チュートリアルを行うか確認する状態での入力処理
    private void HandleAskTutorial()
    {
        if (m_Player.Input.actions["A_Decision"].WasReleasedThisFrame())
        {
            // Aボタン押下 → チュートリアル開始
            // 質問UIを非表示
            m_AskUI.gameObject.SetActive(false);
            InputIconDisplayer.instance.HideAllIcon();
            // スライド画像を表示
            m_SlideImage.gameObject.SetActive(true);
            m_CurrentSlideIndex = 0;
            m_SlideImage.sprite = m_TutorialSlides[m_CurrentSlideIndex];
            StartCoroutine(FadeInImage(m_SlideImage));
            m_CurrentState = State.TUTORIAL;
        }
        else if (m_Player.Input.actions["B_Cancel"].WasReleasedThisFrame())
        {
            // Bボタン押下 → チュートリアルスキップ
            // 質問UIを非表示
            m_AskUI.gameObject.SetActive(false);
            InputIconDisplayer.instance.HideAllIcon();
            m_CurrentState = State.GAMESTART;
        }
    }

    // チュートリアルスライド操作状態での入力処理
    private void HandleTutorial()
    {
        if (m_IsFading) return; // フェード中は入力無効

        if (m_Player.Input.actions["A_Decision"].WasReleasedThisFrame())
        {
            // Aボタン押下 → 次のスライドへ
            if (m_CurrentSlideIndex < m_TutorialSlides.Count - 1)
            {
                m_CurrentSlideIndex++;
                StartCoroutine(ChangeSlideWithFade(m_TutorialSlides[m_CurrentSlideIndex]));
            }
            else
            {
                m_ArrowW.SetActive(false);
                m_ArrowE.SetActive(false);
                InputIconDisplayer.instance.HideAllIcon();
                // 最後のスライド → チュートリアル終了
                m_CurrentState = State.GAMESTART;
                return;
            }
        }
        else if (m_Player.Input.actions["B_Cancel"].WasReleasedThisFrame())
        {
            // Bボタン押下 → 前のスライドに戻る
            if (m_CurrentSlideIndex > 0)
            {
                m_CurrentSlideIndex--;
                StartCoroutine(ChangeSlideWithFade(m_TutorialSlides[m_CurrentSlideIndex]));
            }
        }
        // 基本を非表示にしてから必要なものだけONにする
        m_ArrowW.SetActive(true);
        m_ArrowE.SetActive(true);
        InputIconDisplayer.instance.HideAllIcon();

        if (m_CurrentSlideIndex == 0)
        {
            m_ArrowW.SetActive(false);
            InputIconDisplayer.instance.ShowInputIcon(
                InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.TUTORIAL_A, m_Player
                );
        }
        else if (m_CurrentSlideIndex == m_TutorialSlides.Count)
        {
            m_ArrowE.SetActive(false);
            InputIconDisplayer.instance.ShowInputIcon(
                InputIconDisplayer.InputKey.B, InputIconDisplayer.PositionType.TUTORIAL_B, m_Player
            );
        }
        else
        {
            InputIconDisplayer.instance.ShowInputIcon(
                InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.TUTORIAL_A, m_Player
            );
            InputIconDisplayer.instance.ShowInputIcon(
                InputIconDisplayer.InputKey.B, InputIconDisplayer.PositionType.TUTORIAL_B, m_Player
            );
        }

    }

    // スライド変更をフェードで行うコルーチン
    private IEnumerator ChangeSlideWithFade(Sprite nextSprite)
    {
        m_IsFading = true;
        yield return StartCoroutine(FadeOutImage(m_SlideImage));
        m_SlideImage.sprite = nextSprite;
        yield return StartCoroutine(FadeInImage(m_SlideImage));
        m_IsFading = false;
    }

    // 画像をフェードアウトするコルーチン
    private IEnumerator FadeOutImage(Image img)
    {
        Color startColor = img.color;
        Color endColor = startColor;
        endColor.a = 0f;

        float time = 0f;
        while (time < m_FadeDuration)
        {
            time += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, time / m_FadeDuration);
            yield return null;
        }
        img.color = endColor;
    }

    // 画像をフェードインするコルーチン
    private IEnumerator FadeInImage(Image img)
    {
        Color startColor = img.color;
        Color endColor = startColor;
        endColor.a = 1f;

        float time = 0f;
        while (time < m_FadeDuration)
        {
            time += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, time / m_FadeDuration);
            yield return null;
        }
        img.color = endColor;
    }
}

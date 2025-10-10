using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TitleScene : BaseScene
{
    [SerializeField]
    private SpriteRenderer m_Title;
    [SerializeField]
    private GameObject m_Canvas;
    void Start()
    {
        m_Canvas.SetActive(false);
        CoroutineRunner.instance.RunCoroutine(TitleStart());
    }
    private IEnumerator TitleStart()
    {
        yield return CoroutineRunner.instance.RunCoroutine(TitleFadeSequence());
        CoroutineRunner.instance.RunCoroutine(ReadyGame());
    }
    private IEnumerator TitleFadeSequence()
    {
        yield return null;
        AudioManager.instance.PlaySE("Title", "Title");
        float Duration = AudioManager.instance.GetClip(true, "Title", "Title").length * 0.5f;
        // フェードイン
        yield return CoroutineRunner.instance.RunCoroutine(FadeTitleColor(Color.black, Color.white, Duration));
        // フェードアウト
        yield return CoroutineRunner.instance.RunCoroutine(FadeTitleColor(Color.white, new Color(1.0f, 1.0f, 1.0f, 0.0f), Duration));
    }
    private IEnumerator ReadyGame()
    {
        // フェード完了後、キャンバス表示
        AudioManager.instance.PlayBGM("Title", "TitleBGM");
        //プレイヤーUI表示開始
        m_Canvas.SetActive(true);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.TITLESCENE));
        //参加受付開始
        PlayerManager.instance.IsJoined(true);
        yield return CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.IsAllCheck());
        SceneController.instance.SceneChange("GameScene");
    }
    private IEnumerator FadeTitleColor(Color from, Color to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            m_Title.color = Color.Lerp(from, to, time / duration);
            yield return null;
        }
        m_Title.color = to; // 最終色を確実にセット
    }
}

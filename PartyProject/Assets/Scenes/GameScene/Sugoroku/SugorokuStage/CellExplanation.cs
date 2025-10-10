using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellExplanation : SingletonMonoBehaviour<CellExplanation>
{
    [SerializeField] private Text m_FadeText;
    [SerializeField] private Image m_FadeImage;
    [SerializeField] private Text m_WaitText;
    [SerializeField] private Image m_WaitImage;
    private MainPlayer m_Player;

    private IEnumerator FadeText(string message, float duration, float displayTime, bool waitInput)
    {
        if (string.IsNullOrEmpty(message)) yield break;

        Text text;
        Image image;
        if (waitInput)
        {
            text = m_WaitText;
            image = m_WaitImage;
        }
        else
        {
            text = m_FadeText;
            image = m_FadeImage;
        }

        // 初期化
        text.text = message;
        text.gameObject.SetActive(true);
        image.gameObject.SetActive(true);

        Color textColor = text.color;
        Color imageColor = image.color;

        text.color = textColor.WithHSVA(a: 0f);
        image.color = imageColor.WithHSVA(a: 0f);

        // --- フェードイン ---
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = t / duration;
            text.color = textColor.WithHSVA(a: alpha);
            image.color = imageColor.WithHSVA(a: alpha);
            yield return null;
        }
        text.color = textColor.WithHSVA(a: 1f);
        image.color = imageColor.WithHSVA(a: 1f);

        // --- 表示状態 ---
        if (waitInput)
        {
            InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.CELL_EXPLANATION, m_Player);
            // プレイヤー入力待ち
            if (m_Player.IsNpc)
            {
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                yield return new WaitUntil(() => m_Player.Input.actions["A_Decision"].WasReleasedThisFrame());
            }
            InputIconDisplayer.instance.HideAllIcon();
        }
        else
        {
            // 一定時間表示
            yield return new WaitForSeconds(displayTime);
        }

        // --- フェードアウト ---
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = 1f - (t / duration);
            text.color = textColor.WithHSVA(a: alpha);
            image.color = imageColor.WithHSVA(a: alpha);
            yield return null;
        }

        text.color = textColor.WithHSVA(a: 0f);
        image.color = imageColor.WithHSVA(a: 0f);

        // 非表示
        text.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }

    // フェードイン → 時間経過で消える
    public IEnumerator ShowFadeExplanation(string fadeText)
    {
        yield return FadeText(fadeText, 0.5f, 1.5f, false);
        yield return new WaitForSeconds(0.5f);
    }

    // フェードイン → 入力待ち → 消える
    public IEnumerator ShowWaitExplanation(string waitText, MainPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("入力するプレイヤーがおらぬ");
            yield break;
        }
        m_Player = player;
        AudioManager.instance.PlaySE("Sugoroku", "ShowWaitExplanation");
        yield return FadeText(waitText, 0.5f, 0f, true);
        yield return new WaitForSeconds(0.5f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputIconDisplayer;

public class InputIcon : MonoBehaviour
{
    [SerializeField] private Image m_TargetImage; // 表示する画像
    [SerializeField] private Text m_PlayerText;   // 入力するプレイヤーテキスト
    [SerializeField] private Image m_PlayerTextBack;   // 入力するプレイヤーテキスト
    private void Start()
    {
        HideInputIcon();
    }
    public void ShowInputIcon(InputKey inputKey, PositionType posType, MainPlayer player = null)
    {
        this.gameObject.SetActive(true);

        // アイコン取得
        if (!InputIconDisplayer.instance.IconDictionary.TryGetValue(inputKey, out var iconPair))
        {
            Debug.LogWarning($"入力キー '{inputKey}' に対応するアイコンが登録されていません。");
            return;
        }

        // --- 仮デバイス判定 ---
        bool isUsingController = true; // デフォルトはコントローラー
        if (player != null)
        {
            // TODO: 実際は player から判定

        }

        // アイコン設定
        Sprite icon = isUsingController ? iconPair.controller : iconPair.keyboard;
        if (icon != null)
        {
            m_TargetImage.sprite = icon;
        }

        // プレイヤーテキスト設定
        if (player != null)
        {
            m_PlayerTextBack.gameObject.SetActive(true);
            m_PlayerText.text = $"P{(int)player.Type + 1}";
            m_PlayerText.color = player.SkinColor;
        }
        else
        {
            m_PlayerTextBack.gameObject.SetActive(false);
        }

        // ポジション反映
        RectTransform rect = this.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = InputIconDisplayer.instance.PositionDictionary[posType];
        }
    }

    public void HideInputIcon()
    {
        this.gameObject.SetActive(false);
    }
}

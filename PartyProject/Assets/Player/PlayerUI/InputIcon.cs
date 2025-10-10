using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputIconDisplayer;

public class InputIcon : MonoBehaviour
{
    [SerializeField] private Image m_TargetImage; // �\������摜
    [SerializeField] private Text m_PlayerText;   // ���͂���v���C���[�e�L�X�g
    [SerializeField] private Image m_PlayerTextBack;   // ���͂���v���C���[�e�L�X�g
    private void Start()
    {
        HideInputIcon();
    }
    public void ShowInputIcon(InputKey inputKey, PositionType posType, MainPlayer player = null)
    {
        this.gameObject.SetActive(true);

        // �A�C�R���擾
        if (!InputIconDisplayer.instance.IconDictionary.TryGetValue(inputKey, out var iconPair))
        {
            Debug.LogWarning($"���̓L�[ '{inputKey}' �ɑΉ�����A�C�R�����o�^����Ă��܂���B");
            return;
        }

        // --- ���f�o�C�X���� ---
        bool isUsingController = true; // �f�t�H���g�̓R���g���[���[
        if (player != null)
        {
            // TODO: ���ۂ� player ���画��

        }

        // �A�C�R���ݒ�
        Sprite icon = isUsingController ? iconPair.controller : iconPair.keyboard;
        if (icon != null)
        {
            m_TargetImage.sprite = icon;
        }

        // �v���C���[�e�L�X�g�ݒ�
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

        // �|�W�V�������f
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

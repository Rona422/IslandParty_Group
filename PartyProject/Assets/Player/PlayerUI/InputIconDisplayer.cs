using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIconDisplayer : SingletonMonoBehaviour<InputIconDisplayer>
{
    [SerializeField] private InputIcon[] m_InputIcons = new InputIcon[5]; // 管理する最大数は 5

    [Header("画像の入れる順番\r\n        A,\r\n        B,\r\n        X,\r\n        Y,\r\n        LS")]
    [SerializeField] private List<Sprite> m_ButtonSprites;
    [SerializeField] private List<Sprite> m_KeyboardSprites;

    // 入力キー列挙体
    public enum InputKey
    {
        A,
        B,
        X,
        Y,
        LS
    }
    // 入力キーごとのアイコンセット (keyboard, controller)
    private Dictionary<InputKey, (Sprite keyboard, Sprite controller)> m_IconDictionary = new();
    public Dictionary<InputKey, (Sprite keyboard, Sprite controller)> IconDictionary {  get { return m_IconDictionary; } }
    // ポジションタイプ列挙体
    public enum PositionType
    {
        DICE,
        MIGAME_HOWTOPLAY,
        MIGAME_END,
        CELL_EXPLANATION,
        TUTORIAL_ASK_YES,
        TUTORIAL_ASK_NO,
        TUTORIAL_A,
        TUTORIAL_B,
    }
    private Dictionary<PositionType, Vector2> m_PositionDictionary = new();
    public Dictionary<PositionType, Vector2> PositionDictionary {  get { return m_PositionDictionary; } }

    protected override void Awake()
    {
        base.Awake();

        m_IconDictionary[InputKey.A] = (
            m_ButtonSprites[(int)InputKey.A],
            m_KeyboardSprites[(int)InputKey.A]
        );

        m_IconDictionary[InputKey.B] = (
                        m_ButtonSprites[(int)InputKey.B],
            m_KeyboardSprites[(int)InputKey.B]
        );

        m_IconDictionary[InputKey.X] = (
            m_ButtonSprites[(int)InputKey.X],
            m_KeyboardSprites[(int)InputKey.X]
        );

        m_IconDictionary[InputKey.Y] = (
            m_ButtonSprites[(int)InputKey.Y],
            m_KeyboardSprites[(int)InputKey.Y]
        );

        m_IconDictionary[InputKey.LS] = (
            m_ButtonSprites[(int)InputKey.LS],
            m_KeyboardSprites[(int)InputKey.LS]
        );

        m_PositionDictionary[PositionType.DICE] = new Vector2(0, -465);
        m_PositionDictionary[PositionType.MIGAME_HOWTOPLAY] = new Vector2(335, -216);
        m_PositionDictionary[PositionType.MIGAME_END] = new Vector2(800, -400);
        m_PositionDictionary[PositionType.CELL_EXPLANATION] = new Vector2(640, -360);
        m_PositionDictionary[PositionType.TUTORIAL_ASK_YES] = new Vector2(-400, -200);
        m_PositionDictionary[PositionType.TUTORIAL_ASK_NO] = new Vector2(390, -200);
        m_PositionDictionary[PositionType.TUTORIAL_A] = new Vector2(80, -375);
        m_PositionDictionary[PositionType.TUTORIAL_B] = new Vector2(-80, -375);
    }

    public void ShowInputIcon(InputKey inputKey, PositionType posType, MainPlayer player = null)
    {
        foreach (var icon in m_InputIcons)
        {
            if (!icon.gameObject.activeSelf)
            {
                icon.ShowInputIcon(inputKey, posType, player);
                return;
            }
        }

        Debug.LogWarning("空きの InputIcon がありません！");
    }

    public void HideAllIcon()
    {
        foreach (var icon in m_InputIcons)
        {
            icon.HideInputIcon();
        }
    }
}
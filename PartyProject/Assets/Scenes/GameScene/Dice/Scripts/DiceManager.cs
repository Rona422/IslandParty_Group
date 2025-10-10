using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DiceManager : SingletonMonoBehaviour<DiceManager>
{
    [Header("Diceを入れる順番\r\n" +
            "        WHITE,\r\n" +
            "        GOLD,\r\n" +
            "        SILVER,\r\n" +
            "        BRONZE,\r\n" +
            "        BLUE_EXCEPT,\r\n" +
            "        RED_EXCEPT,\r\n" +
            "        GREEN_EXCEPT,\r\n" +
            "        YELLOW_EXCEPT,")]
    [SerializeField]
    private List<DiceController> m_OriginalDiceList;
    public enum DiceType
    {
        WHITE,
        GOLD,
        SILVER,
        BRONZE,
        BLUE_EXCEPT,
        RED_EXCEPT,
        GREEN_EXCEPT,
        YELLOW_EXCEPT,
    }
    public enum DicePositionType
    {
        SUGOROKU_LEFT,
        SUGOROKU_MIDDLE,
        SUGOROKU_RIGHT,
        SUGOROKU_TWO_LEFT,
        SUGOROKU_TWO_RIGHT,
        RESULT_1,
        RESULT_2,
        RESULT_3,
        RESULT_4,
        // ここ必要な分追加
    }
    // 各出目に対応する回転
    [System.NonSerialized]
    public readonly Dictionary<int, Quaternion> m_FaceRotations = new()
        {
            {1, Quaternion.Euler(-90, 0, 0)},   // Z+ → Y+
            {2, Quaternion.Euler(0, 0, 0)},     // Y+ → Y+
            {3, Quaternion.Euler(0, 0, -90)},   // X- → Y+
            {4, Quaternion.Euler(0, 0, 90)},    // Z- → Y+
            {5, Quaternion.Euler(180, 0, 0)},   // X+ → Y+
            {6, Quaternion.Euler(90, 0, 0)}     // Y- → Y+
        };
    private readonly Dictionary<DicePositionType, Vector3> m_DiceLocalPositions = new()
    {
           { DicePositionType.SUGOROKU_LEFT, new Vector3(-3f, -1.5f, 6f)},
           { DicePositionType.SUGOROKU_MIDDLE, new Vector3(0f, -1f, 6f)},
           { DicePositionType.SUGOROKU_RIGHT, new Vector3(3f, -1.5f, 6f)},
           { DicePositionType.SUGOROKU_TWO_LEFT, new Vector3(-1.5f, -1.5f, 6f)},
           { DicePositionType.SUGOROKU_TWO_RIGHT, new Vector3(1.5f, -1.5f, 6f)},
           { DicePositionType.RESULT_1, new Vector3(-0.6f, -0.4f, 1.2f)},
           { DicePositionType.RESULT_2, new Vector3(-0.2f, -0.4f, 1.2f)},
           { DicePositionType.RESULT_3, new Vector3(0.2f, -0.4f, 1.2f)},
           { DicePositionType.RESULT_4, new Vector3(0.6f, -0.4f, 1.2f)},
    };
    // さいころを取得する関数
    public DiceController GetDice(int type)
    {
        return Instantiate(m_OriginalDiceList[type]);
    }
    public DiceController GetDice(int type, DicePositionType posType)
    {
        var dice = Instantiate(m_OriginalDiceList[type]);
        dice.SetTransform(posType);
        if ((int)posType >= 5)
        {
            dice.transform.localScale = Vector3.one * 0.2f;
        }
        return dice;
    }
    public Vector3 GetDiceLocalPos(DicePositionType posType)
    {
        return m_DiceLocalPositions[posType];
    }
}

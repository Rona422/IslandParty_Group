using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerUi : PlayerBase
{
    [SerializeField] private GameObject m_PlayerIcon;
    [SerializeField] private GameObject m_CpuIcon;
    [SerializeField] private GameObject m_Check;
    private bool m_IsCheck = false;
    public bool IsCheck { get { return m_IsCheck; } }
    private static float SpotTime = 0.1f;
    public static float StandbyColor = 0.7f;
    void Update()
    {
    }
    public void IsCheckCheck()
    {
        if (Player.IsNpc)
        {
            m_IsCheck = PlayerManager.instance.m_Characters.Any(x => !x.IsNpc);
        }
        else
        {
            m_IsCheck = Player.Input.actions["A_Decision"].IsPressed();
        }
        m_Check.SetActive(m_IsCheck);
    }
    public void CheckHyde()
    {
        m_IsCheck = false;
        m_Check.SetActive(m_IsCheck);
    }
    public void SetColor(Color _Color)
    {
        GetComponent<Image>().color = _Color;
        m_PlayerIcon.GetComponent<Image>().color = _Color;
        m_CpuIcon.GetComponent<Image>().color = _Color;
    }
    public IEnumerator SetIcon(Vector2 _SpotPosVec)
    {
        if (Player.IsNpc)
        {
            m_PlayerIcon.SetActive(false);
            m_CpuIcon.SetActive(true);
        }
        else
        {
            m_CpuIcon.SetActive(false);
            m_PlayerIcon.SetActive(true);
        }
        RectTransform ThisTransform = GetComponent<RectTransform>();        Vector2 StandbyPos = ThisTransform.anchoredPosition;
        Vector2 SpotPos = ThisTransform.anchoredPosition += (_SpotPosVec*20);
        //スポット化
        SetColor(Player.SkinColor);
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector2>(
                v => ThisTransform.anchoredPosition = v,
                StandbyPos,
                SpotPos,
                SpotTime,
                Vector2.Lerp
            )
        );
        //逆スポット化
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector2>(
                v => ThisTransform.anchoredPosition = v,
                SpotPos,
                StandbyPos,
                SpotTime,
                Vector2.Lerp
            )
        );
        SetColor(Player.SkinColor * StandbyColor);
    }
}

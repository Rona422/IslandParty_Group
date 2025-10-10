using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public abstract class BarrierBase : Cell
{
    // �֖�����s����
    protected bool m_IsExistence;
    protected virtual void Start()
    {
        m_IsExistence = true;
    }
    public bool IsExistence()
    {
        return m_IsExistence;
    }
    public override void Play()
    {

        if (m_IsExistence)
        {
            CoroutineRunner.instance.RunOnce("BarrierFlowCoroutine",Event());
        }
        else
        {
            base.Play();
        }
    }
    private IEnumerator Event()
    {
        Event_Sugoroku.instance.SetPlayer(m_Player);
        SugorokuManager.instance.IsActiveSugorokuUI(false);
        yield return BarrierFlowCoroutine();
        SugorokuManager.instance.IsActiveSugorokuUI(true);
    }
    protected abstract IEnumerator BarrierFlowCoroutine();
}
// �O����Ă΂����� Play()
// m_Player.EndCellAction();

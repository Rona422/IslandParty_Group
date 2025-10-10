using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorPanicStarter : MonoBehaviour
{
    [SerializeField]
    GameObject m_MeteorPanic;
    private void Awake()
    {
        m_MeteorPanic.SetActive(false);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_MeteorPanic.SetActive(true);
        }
    }
}

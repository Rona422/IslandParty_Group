using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineFreeLook;

public class Camera_SugorokuStage : MonoBehaviour
{
    private CinemachineVirtualCamera m_VCam;
    private CinemachineOrbitalTransposer m_Orbital;
    private const float SPEED = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        m_VCam = GetComponent<CinemachineVirtualCamera>();
        m_Orbital = m_VCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        // �}�E�X���͂𖳌���
        m_Orbital.m_XAxis.m_InputAxisName = "";

    }

    // Update is called once per frame
    void Update()
    {
        // vcam���A�N�e�B�u�ȂƂ�������]
        if (m_VCam.isActiveAndEnabled)
        {
            m_Orbital.m_Heading.m_Bias += SPEED * Time.deltaTime;
        }
    }
}

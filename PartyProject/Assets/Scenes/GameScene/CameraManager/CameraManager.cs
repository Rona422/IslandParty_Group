using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
    public enum SugorokuCameraType
    {
        PLAYER_1,
        PLAYER_2,
        PLAYER_3,
        PLAYER_4,
        STAGE,
        SWAP,
        EVENT_1,
        EVENT_2,
        EVENT_2_1,
        EVENT_2_2,
        EVENT_2_3,
        EVENT_3,
        EVENT_3_BALL,
        EVENT_3_BALLEND,
        EVENT_LAST,
        EVENT_LAST_1,
        TYPE_MAX,
    }
    [Header("�����낭�J���� : �v���C���[4�l���A�X�e�[�W")]
    [SerializeField] private CinemachineVirtualCamera[] m_SugorokuVCams = new CinemachineVirtualCamera[(int)SugorokuCameraType.TYPE_MAX];
    private CinemachineVirtualCamera m_MiniGameVCam;
    private CinemachineBrain m_Brain;
    protected override void Awake()
    {
        base.Awake();
        m_Brain = Camera.main.GetComponent<CinemachineBrain>();
        if (m_Brain != null) m_Brain.m_DefaultBlend.m_Time = 0.0f;
        else Debug.LogError("�����܂˂���I�H");

        // ������ԂőS�Ė�����
        DisableAllVCams();
    }
    private void DisableAllVCams()
    {
        foreach (var vcam in m_SugorokuVCams)
            if (vcam != null) vcam.Priority = 0;

        if (m_MiniGameVCam != null)
            m_MiniGameVCam.Priority = 0;
    }
    public void SwitchToSugorokuCam(SugorokuCameraType type)
    {
        if (type == SugorokuCameraType.TYPE_MAX) return;

        DisableAllVCams();
        m_SugorokuVCams[(int)type].Priority = 10;
    }
    public void SwitchToMiniGameCam()
    {
        if (m_MiniGameVCam != null)
        {
            DisableAllVCams();
            m_MiniGameVCam.enabled = true;
            m_MiniGameVCam.Priority = 10;
        }
        else
        {
            Debug.LogError("�~�j�Q�[���̃J�������˂���I�H");
        }
    }
    public void SetPlayerTransform(IReadOnlyList<Sugoroku_PlayerBase> players)
    {
        for(int i = 0;i < 4;i++)
        {
            if (m_SugorokuVCams[i] == null)
            {
                Debug.LogError("player" + i + "�̃J�������˂���I�H");
                continue;
            }
            if (players[i] == null)
            {
                Debug.LogError("�����낭��player" + i + "�̃I�u�W�F�N�g�˂���I�H");
                continue;
            }
            m_SugorokuVCams[i].Follow = players[i].transform;
            m_SugorokuVCams[i].LookAt = players[i].transform;
        }
    }
    public void SetMiniGameCamera(CinemachineVirtualCamera miniGameCam)
    {
        m_MiniGameVCam = miniGameCam;
    }
    public IEnumerator WaitCameraMove()
    {
        Vector3 prevCameraPos = Camera.main.transform.position;
        for (; ; )
        {
            yield return null;
            Vector3 cameraPos = Camera.main.transform.position;
            if (Vector3.Distance(cameraPos, prevCameraPos) < 0.001f)
            {
                break;
            }
            prevCameraPos = cameraPos;
        }
    }
    public CinemachineVirtualCamera GetSwapVCam()
    {
        return m_SugorokuVCams[(int)SugorokuCameraType.SWAP];
    }
}

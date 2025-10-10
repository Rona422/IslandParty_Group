using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class MiniGameBase : MonoBehaviour
{
    [Header("Awake()�g�p�֎~�AStart()���g�p")]
    [Header("���U���g��ʂɑJ�ڂ���Ƃ��̓v���C���[�̏��ʂ�ݒ肵��Finish()")]
    [Header("�Q�[���^�C�v�����Ă�")]
    public  MiniGameManager.GameType m_GameType;
    [Header("�`�[���l���̏��Ȃ�����4�l�܂œ���Ă�������")]
    [SerializeField]protected List<PlayerBase> m_MiniGamePlayers = new ();
    [Header("MiniGame�Ŏg�p����J����")]
    [SerializeField] protected CinemachineVirtualCamera m_MiniGameCam;
    public List<PlayerBase> GetPlayers() { return m_MiniGamePlayers; }
    [Header("�Q�[���^�C�g��")]
    [SerializeField,TextArea] public String m_GameTitle;
    [Header("�v���C�摜")]
    [SerializeField] public Sprite m_PlayImage;
    [Header("�������")]
    [SerializeField, TextArea] public  String m_GameControl;
    [Header("�Q�[������")]
    [SerializeField, TextArea] public  String m_GameTutorial;
    //�~�j�Q�[���ɎQ������v���C���[���X�g
    private List<MainPlayer> m_MiniGameJoinPlayers = new ();
    public void InitPlayers()
    {
        //�Q�����鏇�ɂȂ��Ă�v���C���[�������Ăяo��
        m_MiniGameJoinPlayers = MiniGameManager.instance.GetMiniGameJoinPlayers();
        //�~�j�Q�[�����ł̃v���C���[���X�g�ƃ~�j�Q�[���Q���v���C���[���X�g�͓����l���łȂ���΂Ȃ�Ȃ�
        if (m_MiniGamePlayers.Count != m_MiniGameJoinPlayers.Count)
        {
            Debug.LogError("gamePlayers����m_MiniGamePlayers�����s��v�ł�");
            Debug.LogError(m_MiniGamePlayers.Count);
            Debug.LogError(m_MiniGameJoinPlayers.Count);
            return;
        }
        SetPlayer(m_MiniGamePlayers, m_MiniGameJoinPlayers);
        ChangeColor(m_MiniGamePlayers, m_MiniGameJoinPlayers);
    }
    public static void SetPlayer(List<PlayerBase> _SetPlayers, List<MainPlayer> _JoinPlayers)
    {
        //�v���C���[����������
        for (int i = 0; i < _JoinPlayers.Count; i++)
        {
            //�~�j�Q�[�����̃v���C���[��PlayerBase���Ă�
            PlayerBase _playerBase = _SetPlayers[i];
            if (_playerBase == null)
            {
                //�~�j�Q�[�����̃v���C���[�ɂ�PlayerBase���p�������X�N���v�g��\��Ȃ��Ƃ����Ȃ�
                Debug.LogError("PlayerBase�����ƃv���C���[�̃Z�b�g�o����̂₯��");
                return;
            }
            //�~�j�Q�[�����̃I�u�W�F�N�g�Ƀv���C���[���Z�b�g
            _playerBase.SetMainPlayer(_JoinPlayers[i]);
        }
    }
    public static void ChangeColor(List<PlayerBase> _SetPlayers, List<MainPlayer> _JoinPlayers)
    {
        //�v���C���[����������
        for (int i = 0; i < _JoinPlayers.Count; i++)
        {
            MeshRenderer _mesh = _SetPlayers[i].gameObject.GetComponent<MeshRenderer>();
            if (_mesh == null)
            {
                Debug.LogWarning("�X�L�����B��̃v���C���[�̔��ʕ��@�����ǁAMeshRenderer�����đ��v���H");
                return;
            }
            //�~�j�Q�[�����̃I�u�W�F�N�g�ɃX�L���𔽉f
            _mesh.material.color = _JoinPlayers[i].SkinColor;
        }
    }
    public void Finish()
    {
        Debug.Log("finish��΂ꂽ��������");
        //�v���C���[�̃����L���O���ݒ肳��Ă��邩�m�F
        for (int i = 0; i < m_MiniGameJoinPlayers.Count;i++)
        {
            if (PlayerManager.instance.m_Characters[i].rank == -2147483647)
            {
                Debug.LogWarning("�v���C���[�̏��ʂ����̂܂�܂̓z������˂�\n�ꉞ�ꗥ�ŉ��ʂŐi��ǂ���");
            }
        }
        //�~�j�Q�[���̏�Ԃ����U���g��Ԃɐ���
        MiniGameManager.instance.PlayFinish();
    }
    public void SetFrozen(bool freeze)
    {
        //�S�Ă̎q�I�u�W�F�N�g�̃R���|�[�l���g���~
        foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = !freeze;
        }
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = freeze;
            if (freeze)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        foreach (var animator in GetComponentsInChildren<Animator>())
        {
            animator.enabled = !freeze;
        }
        foreach (var agent in GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
        {
            agent.enabled = !freeze;
        }
    }
    public CinemachineVirtualCamera GetMiniGameVCam()
    {
        return m_MiniGameCam;
    }
}

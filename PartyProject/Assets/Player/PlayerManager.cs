using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.TextCore.Text;
using UnityEngine.Windows;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    //�ő�v���C���[��
    public const int PlayerMax = 4;
    //�厲�ƂȂ�v���C���[�̃v���t�@�u
    public GameObject PlayerPrefab;
    //�v���C���[�̎��
    public enum CharacterType
    {
        ONE,
        TWO,
        THREE,
        FOUR,
    }
    //NPC�̋���
    public enum CharacterStrong
    {
        NORMAL,
        STRONG,
        NONE,
    }
    //�L�����N�^�[���X�g(�e�ʏ����������Ő����͂��Ă��Ȃ�
    [System.NonSerialized]
    public List<MainPlayer> m_Characters = new(PlayerMax);
    [Header("���v���C���[�̎Q�����󂯕t���Ă����Ԃ�")]
    [SerializeField] private bool m_IsJoin;
    protected override void Awake()
    {
        base.Awake();
        m_IsJoin = false;
        m_Characters.Clear();
        //�C���X�y�N�^�[��ł̃A�i���O����`�F�b�N
        if (PlayerPrefab == null)
        {
            Debug.LogError("PlayerPrefab���ݒ肳��Ă��܂���I");
            return;
        }
        for (int i = 0; i < PlayerMax; i++)
        {
            // BLUE, RED, GREEN, YELLOW �����蓖��
            MainPlayer _mainPlayer = Instantiate(PlayerPrefab).GetComponent<MainPlayer>();
            //�C���X�y�N�^�[��ł̃A�i���O����`�F�b�N
            if (_mainPlayer == null)
            {
                Debug.LogError("PlayerPrefab��MainPlayer�R���|�[�l���g������܂���I");
                continue;
            }
            //NPC�Ƃ��ď�����
            _mainPlayer.Init((CharacterType)i);
            m_Characters.Add(_mainPlayer);
        }
    }
    private void Update()
    {
        //null�ɂȂ��Ă���NPC��
        for (int i = 0; i < m_Characters.Count; i++)
        {
            var character = m_Characters[i];
            //�v���C���[�������Ă���
            if (!character.IsNpc)
            {
                var input = character.Input;
                // Input���̂�null�Ȃ�ގ�
                if (input == null)
                {
                    Debug.Log("���͏��null�ł�");
                    OnPlayerLeft(character, i);
                    continue;
                }
                // devices��null�܂��͗L����device���Ȃ�
                var devices = input.devices;
                //�f�o�C�X���y�A�����O������
                if (!character.isDeviceJoin && devices.Count > 0 && devices.Any(d => d.added && d.enabled))
                {
                    character.isDeviceJoin = true;
                }
                //��x�ł��f�o�C�X���y�A�����O���Ă�����
                if (character.isDeviceJoin)
                {
                    if (devices.Count == 0)
                    {
                        Debug.Log("�R���g���[���[�Ȃ�!!�idevices.Count==0�j");
                        OnPlayerLeft(character, i);
                        continue;
                    }
                    //�S�Ẵf�o�C�X���폜���ꂽ��ގ�
                    bool allInvalid = true;
                    foreach (var device in devices)
                    {
                        if (device.added && device.enabled)
                        {
                            allInvalid = false;
                            break;
                        }
                    }
                    if (allInvalid)
                    {
                        Debug.Log("�R���g���[���[�Ԃ���!!");
                        OnPlayerLeft(character, i);
                    }
                }
            }
        }
    }
    // �v���C���[����������
    public void OnPlayerJoined(PlayerInput input)
    {
        //��t��ԂłȂ���΃u���b�N
        if (!m_IsJoin)
        {
            Destroy(input.gameObject);
            return;
        }
        if (input == null || input.user == null || !input.user.valid)
        {
            Debug.LogWarning("������User�������I");
            if (input != null) Destroy(input.gameObject);
            return;
        }
        //���ɎQ�����Ă���f�o�C�X����
        foreach (var character in m_Characters)
        {
            if (!character.IsNpc && character.Input.user == input.user)
            {
                Debug.LogWarning("���̃f�o�C�X�͊��ɎQ�����Ă��܂��I");
                Destroy(input.gameObject);
                return;
            }
        }
        //NPC����������v���C���[��
        foreach (var character in m_Characters)
        {
            if (character.IsNpc)
            {
                character.SetPlayer(input);
                PlayersUi.instance.JoinLeftPlayer(character.Type);
                AudioManager.instance.PlaySE("Player", "Join");
                Debug.Log($"�v���C���[#{input.user.index}�������I�iInput ID: {input.user.id}�j");
                return;
            }
        }
        Debug.Log("����ȏ�v���C���[��ǉ��ł��܂���B");
        Destroy(input.gameObject);
    }
    // �v���C���[�ގ�����
    public void OnPlayerLeft(MainPlayer _MainPlayer, int _i)
    {
        var input = _MainPlayer.Input;
        ulong leftUserId = input?.user.id ?? 0;
        //Input��GameObject���폜�inull�łȂ��ꍇ�̂݁j
        if (input != null)
        {
            Destroy(input.gameObject);
        }
        _MainPlayer.SetNpc();
        PlayersUi.instance.JoinLeftPlayer(_MainPlayer.Type);
        AudioManager.instance.PlaySE("Player", "Left");
        Debug.Log($"�v���C���[#{_i}���ގ��I�iInput ID: {leftUserId}�j");
    }
    // �����������Ă������̏��
    public void IsJoined(bool _Switch)
    {
        m_IsJoin = _Switch;
    }
}

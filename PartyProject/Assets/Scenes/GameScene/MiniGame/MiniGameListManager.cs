using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class MiniGameListManager : SingletonMonoBehaviour<MiniGameListManager>
{
    //�S�~�j�Q�[�����X�g
    [SerializeField]
    private List<MiniGameBase> miniGames = new();
    //�w�肵���Q�[���^�C�v�̃��X�g��Ԃ�
    public List<MiniGameBase> GameTypeSelect(MiniGameManager.GameType _type)
    {
        //�����ɓ��Ă͂܂�Q�[�������Ă������X�g
        List<MiniGameBase> _list = new();
        //�Q�[���Q��S�Č���
        for (int i = 0; i < miniGames.Count; i++)
        {
            //�I�����ꂽ�Q�[���^�C�v�Ɠ����Ȃ烊�X�g�ɓ���Ă���
            if (miniGames[i].m_GameType == _type)
            {
                _list.Add(miniGames[i]);
            }
        }
        //�f�o�b�O���O
        if (_list == null || _list.Count == 0) Debug.LogWarning($"{_type}�̃Q�[���͑��݂��˂���");
        //���X�g��Ԃ�
        return _list;
    }
}
//���X�g���烉���_���őI������֐�(.GetRandom()
public static class ListRandom
{
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("��̃��X�g���烉���_���Ɏ擾���悤�Ƃ��܂���");
            return default;
        }
        return list[Random.Range(0, list.Count)];
    }
}
// �~�j�Q�[���̑I�ѕ�
// var miniGame = MiniGameListManager.instance.GameTypeSelect(MiniGameManager.GameType.ONE_ALL).GetRandom<>();
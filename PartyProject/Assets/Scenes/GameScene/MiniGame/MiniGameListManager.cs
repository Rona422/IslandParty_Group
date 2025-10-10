using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class MiniGameListManager : SingletonMonoBehaviour<MiniGameListManager>
{
    //全ミニゲームリスト
    [SerializeField]
    private List<MiniGameBase> miniGames = new();
    //指定したゲームタイプのリストを返す
    public List<MiniGameBase> GameTypeSelect(MiniGameManager.GameType _type)
    {
        //条件に当てはまるゲームを入れていくリスト
        List<MiniGameBase> _list = new();
        //ゲーム群を全て見る
        for (int i = 0; i < miniGames.Count; i++)
        {
            //選択されたゲームタイプと同じならリストに入れていく
            if (miniGames[i].m_GameType == _type)
            {
                _list.Add(miniGames[i]);
            }
        }
        //デバッグログ
        if (_list == null || _list.Count == 0) Debug.LogWarning($"{_type}のゲームは存在しねぇよ");
        //リストを返す
        return _list;
    }
}
//リストからランダムで選択する関数(.GetRandom()
public static class ListRandom
{
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("空のリストからランダムに取得しようとしました");
            return default;
        }
        return list[Random.Range(0, list.Count)];
    }
}
// ミニゲームの選び方
// var miniGame = MiniGameListManager.instance.GameTypeSelect(MiniGameManager.GameType.ONE_ALL).GetRandom<>();
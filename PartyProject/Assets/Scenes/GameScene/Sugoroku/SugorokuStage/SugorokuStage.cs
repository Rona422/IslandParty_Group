using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SugorokuStage : SingletonMonoBehaviour<SugorokuStage>
{
    public enum CellType
    {
        NONE,           // a
        FORWARD5,       // b
        BACK5,          // c
        DICE_FORWARD,   // d
        DICE_BACK,      // e
        ONE_ONE,        // f
        ONE_THREE,      // g
        SWAP,           // h
        BARRIER_ONE,    // i
        BARRIER_TWO,    // j
        BARRIER_THREE,  // k
        BARRIER_LAST,   // l
    }

    [Header("        NONE,\r\n        FORWARD5,\r\n        BACK5,\r\n        DICE_FORWARD,\r\n        DICE_BACK,\r\n        ONE_ONE,\r\n        ONE_THREE,\r\n        SWAP")]
    [SerializeField] private List<Material> m_Materials = new();
    [SerializeField] private List<GameObject> m_Cells = new();

    private HashSet<int> m_Gates = new () { 13, 26, 39, 52 };
    private Dictionary<int, CellType> m_GeneratedBoard = new();

    private void Start()
    {
        GenerateBoard();
        SetCells();
        //PrintBoard();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) this.Start();

    }

    private void GenerateBoard()
    {
        // すごろく盤面作成ルール
        // 条件を設けてランダムで生成
        // 条件１ : 4セクションにする
        // 条件２ : b, c, d, eを1まとまりとして考え、各セクションにbcdeのうちどれかを４回,f,g,hをそれぞれ1回,ほかは何もないマスを配置する
        // 条件３ : bcdeをbdとceに分け、セクション１ではbdが２回以上ceが1回以上、それ以外のセクションはbd,ceそれぞれ１回以上配置する
        // 条件４ : 4連続以上同じマスにはならない
        // 条件５ : 13,26,39,52は関門マスで固定
        // 条件６ : セクション４のhは46~50のどれかに配置する

        m_GeneratedBoard.Clear();

        // セクション分割
        var sections = new Dictionary<int, List<int>>()
        {
            {1, new List<int>()},
            {2, new List<int>()},
            {3, new List<int>()},
            {4, new List<int>()}
        };

        // 関門を設定
        for (int i = 1; i <= 52; i++)
        {
            if (m_Gates.Contains(i))
            {
                m_GeneratedBoard.Add(i, CellType.NONE);
                continue;
            }

            if (i <= 13) sections[1].Add(i);
            else if (i <= 26) sections[2].Add(i);
            else if (i <= 39) sections[3].Add(i);
            else sections[4].Add(i);
        }

        // セクションごとの生成
        foreach (var kv in sections)
        {
            int secNum = kv.Key;
            List<int> cells = kv.Value;

            // セクション4はhを先にセット
            if (secNum == 4)
            {
                int hCell = UnityEngine.Random.Range(46, 51);
                m_GeneratedBoard[hCell] = CellType.SWAP;
                cells.Remove(hCell); // セクション4のリストから除く
            }

            List<CellType> assigned = GenerateSection(secNum, cells);

            for (int i = 0; i < cells.Count; i++)
            {
                m_GeneratedBoard[cells[i]] = assigned[i];
            }
        }
        m_GeneratedBoard[13] = CellType.BARRIER_ONE;
        m_GeneratedBoard[26] = CellType.BARRIER_TWO;
        m_GeneratedBoard[39] = CellType.BARRIER_THREE;
        m_GeneratedBoard[52] = CellType.BARRIER_LAST;
    }

    private List<CellType> GenerateSection(int section, List<int> cells)
    {
        List<CellType> pool = new ();

        List<CellType> bd = new () { CellType.FORWARD5, CellType.DICE_FORWARD };
        List<CellType> ce = new () { CellType.BACK5, CellType.DICE_BACK };

        List<CellType> bcdeChosen = new ();

        // セクション1 : bd2回以上、ce1回以上
        if (section == 1)
        {
            bcdeChosen.Add(bd[UnityEngine.Random.Range(0, bd.Count)]);
            bcdeChosen.Add(bd[UnityEngine.Random.Range(0, bd.Count)]);
            bcdeChosen.Add(ce[UnityEngine.Random.Range(0, ce.Count)]);
            var all = new List<CellType>() { CellType.FORWARD5, CellType.BACK5, CellType.DICE_FORWARD, CellType.DICE_BACK };
            bcdeChosen.Add(all[UnityEngine.Random.Range(0, all.Count)]);
        }
        // セクション2~4 : bd1回以上、ce1回以上 
        else
        {
            bcdeChosen.Add(bd[UnityEngine.Random.Range(0, bd.Count)]);
            bcdeChosen.Add(ce[UnityEngine.Random.Range(0, ce.Count)]);
            var all = new List<CellType>() { CellType.FORWARD5, CellType.BACK5, CellType.DICE_FORWARD, CellType.DICE_BACK };
            bcdeChosen.Add(all[UnityEngine.Random.Range(0, all.Count)]);
            bcdeChosen.Add(all[UnityEngine.Random.Range(0, all.Count)]);
        }

        // bcde4個を追加
        pool.AddRange(bcdeChosen);

        // f,gを1個ずつ
        pool.Add(CellType.ONE_ONE);
        pool.Add(CellType.ONE_THREE);

        // hを1個
        // セクション4は設定済み
        if (section != 4)
        {
            pool.Add(CellType.SWAP);
        }

        // 残りはNone
        int rest = cells.Count - pool.Count;
        for (int i = 0; i < rest; i++) pool.Add(CellType.NONE);

        // シャッフル
        Shuffle(pool);

        // 偏りチェック(4連続同一マス不可)
        pool = PreventRepeats(pool, 4);

        return pool;
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private List<CellType> PreventRepeats(List<CellType> pool, int maxRepeat)
    {
        // 先頭から確認し、4連続になりそうなら末尾の別要素と入れ替え
        for (int i = maxRepeat - 1; i < pool.Count; i++)
        {
            bool allSame = true;
            for (int j = 1; j < maxRepeat; j++)
            {
                if (!pool[i - j].Equals(pool[i]))
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame)
            {
                // i以降で別種類を探す
                for (int k = i + 1; k < pool.Count; k++)
                {
                    if (!pool[k].Equals(pool[i]))
                    {
                        var temp = pool[k];
                        pool[k] = pool[i];
                        pool[i] = temp;
                        break;
                    }
                }
            }
        }
        return pool;
    }

    private void PrintBoard()
    {
        for (int i = 1; i <= 52; i++)
        {
            if (m_GeneratedBoard.ContainsKey(i))
            {
                Debug.Log($"{i}: {m_GeneratedBoard[i]}");
            }
            else
            {
                Debug.Log($"{i}: None");
            }
        }
    }



    private void SetCells()
    {
        // セルのオブジェクトにCellTypeを反映
        SetCellType(m_Cells[0], CellType.NONE);
        SetCellType(m_Cells[53], CellType.NONE);
        foreach (var kv in m_GeneratedBoard)
        {
            SetCellType(m_Cells[kv.Key], kv.Value);
        }
    }

    private void SetCellType(GameObject obj, CellType type)
    {
        // Materialを設定
        Transform quadTransform = obj.transform.Find("Quad");
        if (quadTransform != null)
        {
            var meshRenderer = quadTransform.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = m_Materials[(int)type];
            }
        }

        // スクリプトを設定
        switch (type)
        {
            case CellType.NONE:
                obj.AddComponent<Cell>();
                break;
            case CellType.FORWARD5:
                obj.AddComponent<Forward5>();
                break;
            case CellType.BACK5:
                obj.AddComponent<Back5>();
                break;
            case CellType.DICE_FORWARD:
                obj.AddComponent<DiceForward>();
                break;
            case CellType.DICE_BACK:
                obj.AddComponent<DiceBack>();
                break;
            case CellType.ONE_ONE:
                obj.AddComponent<Battle1v1>();
                break;
            case CellType.ONE_THREE:
                obj.AddComponent<Battle1v3>();
                break;
            case CellType.SWAP:
                obj.AddComponent<SwapPlayer>();
                break;
            case CellType.BARRIER_ONE:
                obj.AddComponent<Barrier_1>();
                break;
            case CellType.BARRIER_TWO:
                obj.AddComponent<Barrier_2>();
                break;
            case CellType.BARRIER_THREE:
                obj.AddComponent<Barrier_3>();
                break;
            case CellType.BARRIER_LAST:
                obj.AddComponent<Barrier_Last>();
                break;
            default:
                break;
        }

        // 名前を設定
        obj.name += " (" + type.ToString() + ")";
    }

    public Cell GetCell(int n)
    {
        if (n < 0 || m_Cells.Count <= n) return null;
        return m_Cells[n].GetComponent<Cell>();
    }

    public int GetCellMax()
    {
        return m_Cells.Count;
    }

    public bool IsBarrierCell(int cellNum)
    {
        // 関門のセルかどうか
        return cellNum != 0 && cellNum % 13 == 0;
    }

    public List<Transform> GetCellTransform()
    {
        return m_Cells.Select(c => c.transform).ToList();
    }

}
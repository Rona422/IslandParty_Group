using System.Collections;
using UnityEngine;
public class DiceController : MonoBehaviour
{
    private int m_TargetFace;               // ダイスの面(目的地)
    private Quaternion m_LookRotation;      // カメラ方向を向く回転
    private Vector3 m_AngularVelocity;      // 回転速度
    private bool m_IsRolling;               // 回転中フラグ
    private bool m_IsStop;
    [SerializeField] private DiceManager.DiceType m_DiceType;
    void Awake()
    {
        //デフォの向きランダムにしとく
        transform.rotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f));
        float x = UnityEngine.Random.Range(0f, 1500f);
        float y = 1500f - x;
        float z = UnityEngine.Random.Range(-360f, 360f);
        //回転量もランダムで決めとく
        m_AngularVelocity = new Vector3(x, y, z);
        m_TargetFace = 0;
        m_IsRolling = true;
        m_IsStop = false;
    }
    private void Start()
    {
        CoroutineRunner.instance.RunCoroutine(DiceUpdate());
    }
    // サイコロを振るメソッド
    public void RollDice(int _TargetFaceNum = 0)
    {
        AudioManager.instance.PlaySE("Sugoroku", "DiceRowling");
        //1<_TargetFaceNum<7じゃないとき
        if (_TargetFaceNum <= 0 || 6 < _TargetFaceNum)
        {
            m_TargetFace = UnityEngine.Random.Range(1, 7);
        }
        else
        {
            m_TargetFace = _TargetFaceNum;
        }
        m_IsStop=true;
    }
    // ダイスの値を取得
    public int GetDiceNum()
    {
        switch (m_DiceType)
        {
            case DiceManager.DiceType.WHITE:
            case DiceManager.DiceType.GOLD:
                return m_TargetFace;
            case DiceManager.DiceType.BRONZE:
                return ((m_TargetFace - 1) % 2) + 1;
            case DiceManager.DiceType.SILVER:
                return (m_TargetFace > 3) ? 7 - m_TargetFace : m_TargetFace;
            case DiceManager.DiceType.BLUE_EXCEPT:
            case DiceManager.DiceType.RED_EXCEPT:
            case DiceManager.DiceType.GREEN_EXCEPT:
            case DiceManager.DiceType.YELLOW_EXCEPT:
                return (m_TargetFace > 3) ? 7 - m_TargetFace - 1 : m_TargetFace - 1;
            default:
                break;
        }
        return 0;
    }
    // 回転中かを取得
    public bool GetIsRolling()
    {
        return m_IsRolling;
    }
    // さいころのトランスフォーム設定
    public void SetTransform(DiceManager.DicePositionType posType)
    {
        Camera cam = Camera.main;
        Vector3 worldPos = cam.transform.TransformPoint(DiceManager.instance.GetDiceLocalPos(posType));
        transform.position = worldPos;
        // 水平方向のみに回転させてカメラ方向を向く
        Vector3 lookDir = cam.transform.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.001f)
            lookDir = Vector3.forward; // fallback
        float lookYAngle = Quaternion.LookRotation(lookDir).eulerAngles.y;
        // X軸に前かがみ（-40度）、Y軸はカメラ方向
        Quaternion forwardTiltRotation = Quaternion.Euler(40f, lookYAngle, 0f);
        transform.rotation = forwardTiltRotation;
        // 保存（回転補間用、必要なら forwardTiltRotation にする）
        m_LookRotation = forwardTiltRotation;
    }
    private IEnumerator DiceUpdate()
    {
        yield return (WaitRolling());
        float DurationTime = UnityEngine.Random.Range(1.0f, 2.0f);
        float SectionTime = DurationTime * UnityEngine.Random.Range(0.6f, 0.9f);
        yield return (StopRolling(DurationTime, SectionTime, 90f));
        yield return new WaitForSeconds(1.5f);
        m_IsRolling = false;
    }
    private IEnumerator WaitRolling()
    {
        //m_IsRollingがtrueになるまで
        for (; ; )
        {
            //1秒でm_AngularVelocity分回転
            transform.Rotate(m_AngularVelocity * Time.deltaTime, Space.World);
            yield return null;
            if (m_IsStop) break;
        }
    }
    private IEnumerator StopRolling(float _DurationTime, float _SectionTime, float _OriginLength)
    {
        // 終着点
        Vector3 TargetRot = (m_LookRotation * DiceManager.instance.m_FaceRotations[m_TargetFace]).eulerAngles;
        // 終着点から一定離れた点
        Vector3 SectionRot = TargetRot + UnityEngine.Random.onUnitSphere * _OriginLength;
        //回転と同時に上に弾く
        CoroutineRunner.instance.RunCoroutine(DiceHopping(_SectionTime));
        //終点から_OriginLength離れた点まで急特攻で回転
        yield return (CoroutineRunner.instance.LerpValue<Vector3>(
                    q =>
                    {
                        transform.rotation = Quaternion.Euler(q);
                    },
                    transform.rotation.eulerAngles+(Vector3.one*720f),
                    SectionRot,
                    _SectionTime,
                    Vector3.Slerp
                ));
        CoroutineRunner.instance.RunCoroutine(DiceHopping((_DurationTime - _SectionTime)*0.2f));
        //_OriginLengthの距離を残りの時間ゆっくり終点まで発進
        yield return (CoroutineRunner.instance.LerpValue<Vector3>(
            q =>
            {
                transform.rotation = Quaternion.Euler(q);
            },
            SectionRot,
            TargetRot,
            _DurationTime - _SectionTime,
            Vector3.Slerp
        ));
    }
    private IEnumerator DiceHopping(float _DurationTime)
    {
        Vector3 StartPos = this.transform.position;
        Vector3 UpPos = StartPos + new Vector3(0.0f,_DurationTime*0.4f, 0.0f);
        //_DurationTimeの間で往復上下運動
        //_DurationTimeの4割で上がる
        yield return (CoroutineRunner.instance.LerpValue<Vector3>(
                    p =>
                    {
                        transform.position = p;
                    },
                    StartPos,
                    UpPos,
                    _DurationTime*0.4f,
                    Vector3.Slerp
        ));
        //_DurationTimeの6割で下がる
        yield return (CoroutineRunner.instance.LerpValue<Vector3>(
                    p =>
                    {
                        transform.position = p;
                    },
                    UpPos,
                    StartPos,
                    _DurationTime*0.6f,
                    Vector3.Slerp
        ));
    }
    public IEnumerator MoveDiceToPosition(DiceManager.DicePositionType targetPosType, float duration = 0.4f)
    {
        float elapsed = 0f;

        // 現在位置
        Vector3 startPos = transform.position;

        // 一時的に同じ種類のダイスを生成して位置取得（またはマネージャーから直接座標参照）
        var tempDice = DiceManager.instance.GetDice((int)m_DiceType, targetPosType);
        Vector3 targetPos = tempDice.transform.position;
        Destroy(tempDice.gameObject); // ダミーなので破棄

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            elapsed += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }
}

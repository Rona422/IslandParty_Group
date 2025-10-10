using System.Collections;
using UnityEngine;
public class DiceController : MonoBehaviour
{
    private int m_TargetFace;               // �_�C�X�̖�(�ړI�n)
    private Quaternion m_LookRotation;      // �J����������������]
    private Vector3 m_AngularVelocity;      // ��]���x
    private bool m_IsRolling;               // ��]���t���O
    private bool m_IsStop;
    [SerializeField] private DiceManager.DiceType m_DiceType;
    void Awake()
    {
        //�f�t�H�̌��������_���ɂ��Ƃ�
        transform.rotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f));
        float x = UnityEngine.Random.Range(0f, 1500f);
        float y = 1500f - x;
        float z = UnityEngine.Random.Range(-360f, 360f);
        //��]�ʂ������_���Ō��߂Ƃ�
        m_AngularVelocity = new Vector3(x, y, z);
        m_TargetFace = 0;
        m_IsRolling = true;
        m_IsStop = false;
    }
    private void Start()
    {
        CoroutineRunner.instance.RunCoroutine(DiceUpdate());
    }
    // �T�C�R����U�郁�\�b�h
    public void RollDice(int _TargetFaceNum = 0)
    {
        AudioManager.instance.PlaySE("Sugoroku", "DiceRowling");
        //1<_TargetFaceNum<7����Ȃ��Ƃ�
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
    // �_�C�X�̒l���擾
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
    // ��]�������擾
    public bool GetIsRolling()
    {
        return m_IsRolling;
    }
    // ��������̃g�����X�t�H�[���ݒ�
    public void SetTransform(DiceManager.DicePositionType posType)
    {
        Camera cam = Camera.main;
        Vector3 worldPos = cam.transform.TransformPoint(DiceManager.instance.GetDiceLocalPos(posType));
        transform.position = worldPos;
        // ���������݂̂ɉ�]�����ăJ��������������
        Vector3 lookDir = cam.transform.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.001f)
            lookDir = Vector3.forward; // fallback
        float lookYAngle = Quaternion.LookRotation(lookDir).eulerAngles.y;
        // X���ɑO�����݁i-40�x�j�AY���̓J��������
        Quaternion forwardTiltRotation = Quaternion.Euler(40f, lookYAngle, 0f);
        transform.rotation = forwardTiltRotation;
        // �ۑ��i��]��ԗp�A�K�v�Ȃ� forwardTiltRotation �ɂ���j
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
        //m_IsRolling��true�ɂȂ�܂�
        for (; ; )
        {
            //1�b��m_AngularVelocity����]
            transform.Rotate(m_AngularVelocity * Time.deltaTime, Space.World);
            yield return null;
            if (m_IsStop) break;
        }
    }
    private IEnumerator StopRolling(float _DurationTime, float _SectionTime, float _OriginLength)
    {
        // �I���_
        Vector3 TargetRot = (m_LookRotation * DiceManager.instance.m_FaceRotations[m_TargetFace]).eulerAngles;
        // �I���_�����藣�ꂽ�_
        Vector3 SectionRot = TargetRot + UnityEngine.Random.onUnitSphere * _OriginLength;
        //��]�Ɠ����ɏ�ɒe��
        CoroutineRunner.instance.RunCoroutine(DiceHopping(_SectionTime));
        //�I�_����_OriginLength���ꂽ�_�܂ŋ}���U�ŉ�]
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
        //_OriginLength�̋������c��̎��Ԃ������I�_�܂Ŕ��i
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
        //_DurationTime�̊Ԃŉ����㉺�^��
        //_DurationTime��4���ŏオ��
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
        //_DurationTime��6���ŉ�����
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

        // ���݈ʒu
        Vector3 startPos = transform.position;

        // �ꎞ�I�ɓ�����ނ̃_�C�X�𐶐����Ĉʒu�擾�i�܂��̓}�l�[�W���[���璼�ڍ��W�Q�Ɓj
        var tempDice = DiceManager.instance.GetDice((int)m_DiceType, targetPosType);
        Vector3 targetPos = tempDice.transform.position;
        Destroy(tempDice.gameObject); // �_�~�[�Ȃ̂Ŕj��

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

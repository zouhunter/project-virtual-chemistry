using DG.Tweening;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Assertions.Comparers;
using UnityStandardAssets.Characters.FirstPerson;

public delegate void ImmediateMoveAction(Vector3 pos);

[RequireComponent(typeof(PersonCharacter))]
public class CharacterControl : MonoBehaviour
{
    [SerializeField] private MouseLook m_MouseLook = new MouseLook();
    private PersonCharacter m_Character;
    private NavMeshAgent m_Agent;
    [SerializeField] private Transform m_Cam;
    private Vector3 m_CamForward;
    private Vector3 m_Move;

    public Transform CharacterLookAt
    {
        get { return m_Cam; }
    }

    void Start ()
	{
        m_Character = GetComponent<PersonCharacter>();
	    m_Agent = GetComponentInChildren<NavMeshAgent>();
	    m_Agent.updateRotation = false;
	    m_Agent.updatePosition = true;
        m_MouseLook.Init(transform, m_Cam.transform);
	}

    void Update()
    {
        m_MouseLook.LookRotation(transform, m_Cam.transform);
    }

    public void ImmediateMove(Vector3 pos, Quaternion dir)
    {
        m_Agent.updatePosition = false;
        transform.position = pos;
        transform.rotation = dir;
        m_Agent.Warp(transform.position);
        m_Agent.updatePosition = true;
    }

    private void FixedUpdate()
    {
        float h;
        float v;
        float r;
        bool isInput = IsInputControl(out h, out v, out r);
        m_Move = Vector3.zero;

        if (isInput)
        {
            m_Move = GetInputMovement(h, v, r);
        }

        m_Character.FirstPersonMove(m_Move);
    }

    Vector3 GetInputMovement(float h, float v, float r)
    {
        Vector3 movement;
        if (m_Cam != null)
        {
            var calculateForward = transform.forward;
            var calculateRight = transform.right;
            if (v < 0f)
            {
                calculateForward = m_Cam.forward;
                calculateRight = m_Cam.right;
            }
            m_CamForward = Vector3.Scale(calculateForward, new Vector3(1, 0, 1)).normalized;
            movement = v*m_CamForward + h*calculateRight;
        }
        else
        {
            movement = v*Vector3.forward + h*Vector3.right;
        }

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            movement *= 0.5f;
            r *= 0.5f;
        }

        return movement;
    }

    bool IsInputControl(out float h, out float v, out float r)
    {
        h = CrossPlatformInputManager.GetAxis("Horizontal");
        v = CrossPlatformInputManager.GetAxis("Vertical");
		r = 0f;

        if (!FloatComparer.s_ComparerWithDefaultTolerance.Equals(h, 0))
        {
            return true;
        }

        if (!FloatComparer.s_ComparerWithDefaultTolerance.Equals(v, 0))
        {
            return true;
        }

        return false;
    }
}

using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PersonCharacter : MonoBehaviour
{
    
    [SerializeField] private float m_MovingTurnSpeed = 100;
    [SerializeField] private float m_StationaryTurnSpeed = 50;
    [SerializeField] private float m_MoveSpeedMultiplier = 1f;
    [SerializeField] private float m_AnimSpeedMultiplier = 1f;
    public float TurnSpeed
    {
        get { return m_MovingTurnSpeed; }
        set
        {
            m_MovingTurnSpeed = value*2;
            m_StationaryTurnSpeed = value;
        }
    }
    public float MoveSpeedMultiplier
    {
        get { return m_MoveSpeedMultiplier; }
        set
        {
            m_MoveSpeedMultiplier = value;
            m_AnimSpeedMultiplier = value;
        }
    }

    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
    private float m_TurnAmount;
    private float m_ForwardAmount;

    // Use this for initialization
    void Start ()
	{
	    m_Animator = GetComponent<Animator>();
	    m_Rigidbody = GetComponent<Rigidbody>();

	    m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX
	                              | RigidbodyConstraints.FreezeRotationY
	                              | RigidbodyConstraints.FreezeRotationZ;
	}

    #region First Person Move
    public void FirstPersonMove(Vector3 move)
    {
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        Vector3 desiredMove = Vector3.ProjectOnPlane(move, Vector3.up);

        m_Rigidbody.velocity = desiredMove*m_MoveSpeedMultiplier*4f;        
    }
    #endregion

    #region Third Person Move
    public void ThirdPersonMove(Vector3 move)
    {
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        move = transform.InverseTransformDirection(move);
        m_Animator.applyRootMotion = true;
        move = Vector3.ProjectOnPlane(move, Vector3.up);
        if (!FloatComparer.AreEqual(move.x, 0f, Mathf.Epsilon) 
            || !FloatComparer.AreEqual(move.z, 0f, Mathf.Epsilon))
        {
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
        }
        else
        {
            m_TurnAmount = 0f;
        }

        m_ForwardAmount = move.z;
        
        ApplyExtraTurnRotation();

        UpdateAnimator(move);
    }

    void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount*turnSpeed*Time.deltaTime, 0);
    }

    void UpdateAnimator(Vector3 move)
    {
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        m_Animator.SetBool("Crouch", false);
        m_Animator.SetBool("OnGround", true);

        m_Animator.speed = m_AnimSpeedMultiplier;
    }

    public void OnAnimatorMove()
    {
        if (Time.deltaTime > 0&& m_Animator)
        {
            Vector3 v = (m_Animator.deltaPosition*m_MoveSpeedMultiplier)/Time.deltaTime;
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }
    #endregion
}

using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float speed = 10f;
    public float jumpSpeed = 1f;
    public LayerMask GroundMask;
    public AudioClip Jump;

    private AudioSource m_SoundSource;
    private Animator m_Animator;
    private Transform m_GroundCheck, m_HeadCheck;
    private Rigidbody2D m_RigidBody2D;
    private bool isGrounded;
    private bool isHit;

	// Use this for initialization
	void Start () {
        m_Animator = GetComponent<Animator>();
        m_RigidBody2D = GetComponent<Rigidbody2D>();
        m_GroundCheck =  transform.FindChild("GroundCheck");
        m_HeadCheck = transform.FindChild("HeadCheck");
        isGrounded = true;
        isHit = false;
        Jump = (AudioClip)Resources.Load("Sounds/jump");
        m_SoundSource = Camera.main.transform.FindChild("Sound").GetComponent<AudioSource>();
	}

    //Physics engine needs to use this method
    void FixedUpdate()
    {
        float hspeed = Input.GetAxis("Horizontal");
        isGrounded = Physics2D.OverlapPoint(m_GroundCheck.position, GroundMask);
        isHit = Physics2D.OverlapPoint(m_HeadCheck.position, GroundMask);

        m_Animator.SetFloat("Speed", Mathf.Abs(hspeed));
        m_Animator.SetFloat("Velocity_y", m_RigidBody2D.velocity.y);

        if (m_Animator.GetBool("IsDead"))
            return;

        if (isHit && isGrounded)
        {
            m_Animator.SetBool("IsDead", true);
            m_RigidBody2D.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            this.transform.gameObject.layer = 0;
        }
        if (Input.GetAxis("Jump") > 0 && isGrounded)
        {
            m_RigidBody2D.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse );
            m_SoundSource.PlayOneShot(Jump);
        }

        if (hspeed > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            m_RigidBody2D.velocity = new Vector2(hspeed * speed, m_RigidBody2D.velocity.y);
        }
        else if (hspeed < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            m_RigidBody2D.velocity = new Vector2(hspeed * speed, m_RigidBody2D.velocity.y);
        }
        else if (isGrounded) 
        {
            m_RigidBody2D.velocity = new Vector2(0, m_RigidBody2D.velocity.y);
        }
    }
}

using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PlayerControllerNetworked : NetworkBehaviour
{
    // Networked Variables
    [Networked, OnChangedRender(nameof(NickNameChanged))] public NetworkString<_16> nickName { get; set; }
    [Networked, OnChangedRender(nameof(ClassChanged))] public int characterClass { get; set; }
    [Networked] public float maxHealth { get; set; } = 100;
    [Networked, OnChangedRender(nameof(HPChanged))] public float currentHealth { get; set; } = 100;
    [Networked] public PlayerRef player { get; set; }
    [Networked] public int CurrentServerTick { get; set; }

    // Local Variables
    NetworkRigidbody2D _rb;
    PlayerInputConsumer _input;
    TestBuffIndicator buffIndicator;
    PlayerBuffs buffs;
    Collider2D _collider;
    Animator _anim;
    NetworkMecanimAnimator _mecanim;
    public Dictionary<string, float> skillList;
    public DurationIndicator durationIndicator;
    public Image healthBar;
    public OtherStatusPanel otherStatusPanel;

    #region
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _enemyLayer;

    [Space]
    public float speed;
    public float attackSpeed;
    public Items.Weapon weapon;

    //[SerializeField] float _jumpForce = 10f; 
    [SerializeField] float _jumpHeight = 10f;
    [SerializeField] float _timeToApex = 0.5f;
    //[SerializeField] float _DoubleJumpForce = 8f;
    [SerializeField] float _DoubleJumpHeight = 8f;
    [SerializeField] float _maxVelocity = 8f;

    [SerializeField] float _dashDuration = 0.5f; // ?�� ???? ?��?
    [SerializeField] float _dashDistance = 3.0f; // ?�� ???

    [Space]

    [SerializeField] private float fallMultiplier = 3.3f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Space]

    [SerializeField] Vector2 _groundHorizontalDragVector = new Vector2(.1f, 1);
    [SerializeField] Vector2 _airHorizontalDragVector = new Vector2(.98f, 1);
    [SerializeField] Vector2 _horizontalSpeedReduceVector = new Vector2(.95f, 1);
    [SerializeField] Vector2 _verticalSpeedReduceVector = new Vector2(1, .95f);

    [Space]
    [SerializeField]
    bool IsGrounded = false;
    bool _isDashing = false;

    float _jumpBufferThreshold = .2f;
    float _jumpBufferTime;

    float _rollBufferThreshold = .2f;
    float _rollBufferTime;


    float CoyoteTimeThreshold = .1f;
    float TimeLeftGrounded;
    bool CoyoteTimeCD;
    bool WasGrounded;
    bool hasDoubleJumped;

    [SerializeField]
    Vector3 Velocity;
    [SerializeField]
    Vector3 GroundcheckPosition;
    #endregion

    void Awake()
    {
        // Initialize local variables
        _rb = gameObject.GetComponent<NetworkRigidbody2D>();
        _input = gameObject.GetComponent<PlayerInputConsumer>();
        _collider = GetComponent<Collider2D>();
        _anim = GetComponent<Animator>();
        _mecanim = GetComponent<NetworkMecanimAnimator>();
        durationIndicator = GameObject.FindGameObjectWithTag("DurationUI").GetComponent<DurationIndicator>();
        healthBar = GameObject.FindGameObjectWithTag("CharacterHealthUI").GetComponent<Image>();
        buffs = gameObject.GetComponent<PlayerBuffs>();
    }

    void Start()
    {
        if (HasInputAuthority)
        {
            // Set camera follow target
            Camera.main.GetComponent<CameraMovement>().followTarget = gameObject;
            buffIndicator = GameObject.FindGameObjectWithTag("BuffIndicator").GetComponent<TestBuffIndicator>();
            buffIndicator.playerBuffs = buffs;
            buffs.buffIndicator = buffIndicator;
            buffs.Test();
            RPC_SetNickName(Runner.gameObject.GetComponent<NetworkManager>().nickName);
            RPC_SetClass(1);
        }
        else
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            GameObject otherStatusPrefab = Runner.gameObject.GetComponent<NetworkManager>().otherStatusPrefab;
            GameObject other = Instantiate(otherStatusPrefab, canvas.gameObject.transform);
            OtherStatusPanel osp = other.GetComponent<OtherStatusPanel>();
            otherStatusPanel = osp;
            osp.player = this;
            osp.buffIndicator.playerBuffs = buffs;
            buffs.buffIndicator = osp.buffIndicator;

            other.SetActive(true);

            OtherPanelUpdate();
            UpdateCharacterClass(characterClass);
        }
    }

    // Networked animation
    public override void Render()
    {
        base.Render();
    }

    // Character class change function
    void UpdateCharacterClass(int characterClass)
    {
        Debug.Log("Set character class: " + characterClass);
        CharacterClass.ChangeClass(characterClass, gameObject);
    }

    // Test function to check if player is grounded
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireCube((Vector2)transform.position + (Vector2)GroundcheckPosition, Vector2.one * .85f);
    // }

    // Ground check function
    void DetectGroundAndWalls()
    {
        WasGrounded = IsGrounded;
        IsGrounded = default;
        IsGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox((Vector2)transform.position + (Vector2)GroundcheckPosition, Vector2.one * .85f, 0, _groundLayer);
        // Jump animation and run animation depends on this value
        _anim.SetBool("Grounded", IsGrounded);
        if (IsGrounded)
        {
            CoyoteTimeCD = false;
            hasDoubleJumped = false;
            return;
        }

        if (WasGrounded)
        {
            if (CoyoteTimeCD)
            {
                CoyoteTimeCD = false;
            }
            else
            {
                TimeLeftGrounded = Runner.SimulationTime;
            }
        }
    }

    // Networked physics
    public override void FixedUpdateNetwork()
    {
        InputTask();
        Velocity = _rb.Rigidbody.velocity;
        CurrentServerTick = (int)Runner.Tick;
        if (!HasInputAuthority)
        {
            OtherPanelHPUpdate();
        }
        // RPC_GetTickDeltaBetweenClients(nickName);
    }

    void InputTask()
    {
        // dir is the direction of the player(left, right)
        var dir = _input.dir.normalized;
        UpdateMovement(dir.x);
        // PlayerButtons are set in OnInput function of CharacterSpawner.cs script
        Jump(_input.pressed.IsSet(PlayerButtons.Jump));
        BetterJumpLogic(_input.pressed.IsSet(PlayerButtons.Jump));
        Roll(_input.pressed.IsSet(PlayerButtons.Roll));
        // Run attack coroutine of weapon script directly
        if (_input.pressed.IsSet(PlayerButtons.Attack))
        {
            StartCoroutine(weapon.Attack(_anim, _mecanim, gameObject.transform));
        }

    }
    void UpdateMovement(float input)
    {
        if (_isDashing) return;
        // Run animation
        if (input != 0)
        {
            _anim.SetInteger("RunState", 1);
        }
        else
        {
            _anim.SetInteger("RunState", 0);
        }
        // Ground check
        DetectGroundAndWalls();
        if (input < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x > 0 && IsGrounded)
            {
                _rb.Rigidbody.velocity *= Vector2.up;
            }
            _rb.Rigidbody.AddForce(speed * Vector2.left, ForceMode2D.Impulse);
        }
        else if (input > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x < 0 && IsGrounded)
            {
                _rb.Rigidbody.velocity *= Vector2.up;
            }
            _rb.Rigidbody.AddForce(speed * Vector2.right, ForceMode2D.Impulse);
        }
        else
        {
            //Different horizontal drags depending if grounded or not.
            if (IsGrounded)
                _rb.Rigidbody.velocity *= _groundHorizontalDragVector;
            else
                _rb.Rigidbody.velocity *= _airHorizontalDragVector;
        }

        LimitSpeed();
    }

    private void LimitSpeed()
    {
        //Limit horizontal velocity
        if (Mathf.Abs(_rb.Rigidbody.velocity.x) > _maxVelocity)
        {
            _rb.Rigidbody.velocity *= _horizontalSpeedReduceVector;
        }

        if (Mathf.Abs(_rb.Rigidbody.velocity.y) > _maxVelocity * 2)
        {
            _rb.Rigidbody.velocity *= _verticalSpeedReduceVector;
        }
    }


    private void Jump(bool jump)
    {

        //Jump
        if (jump || CalculateJumpBuffer())
        {
            // Run jump animation
            _mecanim.SetTrigger("Jump", true);
            // Deprecated jump function
            // void _jump(float __jumpForce)
            // {
            //     _rb.Rigidbody.velocity *= Vector2.right; //Reset y Velocity
            //     _rb.Rigidbody.AddForce(Vector2.up * __jumpForce, ForceMode2D.Impulse);
            //     CoyoteTimeCD = true;

            // }
            void advanced_jump(float jumpHeight, float timeToApex = 0.5f)
            {
                float gravity = (2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
                float initialJumpVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);

                _rb.Rigidbody.velocity = new Vector2(_rb.Rigidbody.velocity.x, initialJumpVelocity);
                Physics.gravity = new Vector3(0, -gravity, 0);
                CoyoteTimeCD = true;
            }
            if (!IsGrounded && jump)
            {
                if (!hasDoubleJumped)
                {
                    advanced_jump(_DoubleJumpHeight, _timeToApex);
                    hasDoubleJumped = true;
                }

                _jumpBufferTime = Runner.SimulationTime;
            }

            if (IsGrounded || CalculateCoyoteTime())
            {
                //_jump(_jumpForce);
                advanced_jump(_jumpHeight, _timeToApex);
            }


        }
    }

    private bool CalculateJumpBuffer()
    {
        return (Runner.SimulationTime <= _jumpBufferTime + _jumpBufferThreshold) && IsGrounded;
    }

    private bool CalculateCoyoteTime()
    {
        return (Runner.SimulationTime <= TimeLeftGrounded + CoyoteTimeThreshold);
    }

    private void BetterJumpLogic(bool input)
    {
        if (IsGrounded) { return; }
        if (_rb.Rigidbody.velocity.y < 0)
        {
            if (_rb.Rigidbody.velocity.y > 0 && !input)
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Runner.DeltaTime;
            }
            else
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Runner.DeltaTime;
            }
        }

    }
    // In progress
    private void Roll(bool dash)
    {
        if (dash || CalculateRollBuffer())
        {
            if (_isDashing || !IsGrounded && dash)
            {
                _rollBufferTime = Runner.SimulationTime;
            }

            if (IsGrounded && (dash) && !_isDashing) // IsGrounded?? ?��???? ???? ????? ?????? ??????? ????
            {
                StartCoroutine(DashCoroutine());
            }
        }
    }
    // It will be called by animation event
    // projectile fire
    public void FireProjectile()
    {
        weapon.FireProjectileAlt(_anim.GetInteger("AttackState"), gameObject.transform);
    }

    private bool CalculateRollBuffer()
    {
        return (Runner.SimulationTime <= _rollBufferTime + _rollBufferThreshold) && IsGrounded;
    }

    private IEnumerator DashCoroutine()
    {
        _isDashing = true;

        Vector2 dashDirection = _rb.Rigidbody.velocity.normalized; // ????????? ???? ??? ???? (??????)
        _rb.Rigidbody.velocity = dashDirection * Vector2.right * _dashDistance / _dashDuration;
        _collider.excludeLayers = _enemyLayer;
        yield return new WaitForSeconds(_dashDuration);
        _collider.excludeLayers = 0;

        _rb.Rigidbody.velocity = Vector2.zero; // ?�� ?? ??? ????
        _isDashing = false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetNickName(string nick)
    {
        nickName = nick;
    }

    private void NickNameChanged()
    {
        if (!otherStatusPanel)
        {
            return;
        }
        else
        {
            otherStatusPanel.SetName($"[{player.AsIndex}] {nickName}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetClass(int classTypeInt)
    {
        characterClass = classTypeInt;
        RPC_UpdateClass(classTypeInt);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateClass(int classTypeInt)
    {
        UpdateCharacterClass(classTypeInt);
    }
    // 클라이언트 플레이어의 체력 감소가 적용되지 않는 오류가 있음.
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_OnPlayerHit(BossAttack.AttackData attackData)
    {
        currentHealth -= attackData.damage;
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    // TEST RPC FUNCTION
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetTickDeltaBetweenClients(NetworkString<_16> nick)
    {
        Debug.Log($"{nick} - Tick delta:  {(float)Runner.LocalRenderTime * Runner.TickRate - CurrentServerTick}");
    }

    private void ClassChanged()
    {
        UpdateCharacterClass(characterClass);
        if (otherStatusPanel)
        {
            otherStatusPanel.SetClass(characterClass);
        }
    }

    private void HPChanged()
    {
        if (otherStatusPanel)
        {
            otherStatusPanel.SetHP(currentHealth, maxHealth);
        }
    }

    public void OtherPanelUpdate()
    {
        if (otherStatusPanel)
        {
            otherStatusPanel.SetClass(characterClass);
            otherStatusPanel.SetName($"{nickName}");
            otherStatusPanel.SetHP(currentHealth, maxHealth);
        }
    }

    public void OtherPanelHPUpdate()
    {
        if (otherStatusPanel)
        {
            otherStatusPanel.SetHP(currentHealth, maxHealth);
        }
    }
}

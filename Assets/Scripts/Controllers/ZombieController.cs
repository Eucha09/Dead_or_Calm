using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : LivingEntity
{
    Define.ZombieState _state;

    public Define.ZombieState State
    {
        get { return _state; }
        set
        {
            _state = value;
            switch (_state)
            {
                case Define.ZombieState.Idle:
                    _agent.isStopped = true;
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    break;
                case Define.ZombieState.Patrol:
                    _agent.stoppingDistance = 0.1f;
                    _agent.speed = _patrolSpeed;
                    _agent.isStopped = false;
                    _animator.applyRootMotion = false;
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    break;
                case Define.ZombieState.Scream:
                    _animator.SetTrigger("Scream");
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    break;
                case Define.ZombieState.Tracking:
                    _agent.stoppingDistance = _attackDistance;
                    _agent.speed = _runSpeed;
                    _agent.isStopped = false;
                    _animator.applyRootMotion = false;
                    break;
                case Define.ZombieState.AttackBegin:
                    _agent.isStopped = true;
                    _animator.SetTrigger("Attack");
                    break;
                case Define.ZombieState.Attacking:
                    break;
                case Define.ZombieState.Damage:
                    _animator.applyRootMotion = true;
                    _animator.SetTrigger("Damage");
                    _agent.isStopped = true;
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    break;
            }
        }
    }

    NavMeshAgent _agent; // ��ΰ�� AI ������Ʈ
    Animator _animator; // �ִϸ����� ������Ʈ

    Vector3 _startPosition;

    float _runSpeed = 10f;
    float _patrolSpeed = 1f;
    [Range(0.01f, 2f)] float _turnSmoothTime = 0.1f;
    float _turnSmoothVelocity;

    [SerializeField]
    ZombieData _zombieData;

    [SerializeField]
    Transform _attackRoot;
    float _damage = 30f;
    float _attackRadius = 0.5f;
    float _attackDistance;

    [SerializeField]
    Transform _sensingRoot;
    [SerializeField][Range(1f, 10f)]
    float _sensingRange = 5f;

    LivingEntity _targetEntity; // ������ ���
    [SerializeField]
    LayerMask _whatIsTarget; // ���� ��� ���̾�


    RaycastHit[] _hits = new RaycastHit[10];
    List<LivingEntity> _lastAttackedTargets = new List<LivingEntity>();

    bool HasTarget => _targetEntity != null && !_targetEntity.Dead;
    bool IsTargetHiding => _targetEntity != null && _targetEntity.GetComponent<PlayerController>().State == Define.PlayerState.Hide;

    float _stunTime;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _attackDistance = Vector3.Distance(transform.position,
                             new Vector3(_attackRoot.position.x, transform.position.y, _attackRoot.position.z)) +
                         _attackRadius;

        _attackDistance += _agent.radius;

        _startPosition = transform.position;
        _sensingRoot.localScale = new Vector3(_sensingRange, 1f, _sensingRange);

        Setup(_zombieData);
    }

    // �� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public void Setup(ZombieData data)
    {
        // ü�� ����
        this.StartingHealth = data.health;
        this.Health = data.health;

        // ����޽� ������Ʈ�� �̵� �ӵ� ����
        this._runSpeed = data.runSpeed;
        this._patrolSpeed = data.patrolSpeed;

        this._damage = data.damage;
    }

    private void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� ���� ��ƾ ����
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (Dead) return;

        if (_stunTime > 0.0f) // ������ �ް� �ִ� ��
        {
            _stunTime -= Time.deltaTime;
            return;
        }
        else if (State == Define.ZombieState.Damage)
        {
            _stunTime = 0.0f;
            if (!Dead)
                State = Define.ZombieState.Tracking;
        }

        if (State == Define.ZombieState.Tracking && HasTarget &&
            Vector3.Distance(_targetEntity.transform.position, transform.position) <= _attackDistance)
        {
            BeginAttack();
        }

        // ���� ����� ���� ���ο� ���� �ٸ� �ִϸ��̼��� ���
        _animator.SetFloat("Speed", _agent.desiredVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (Dead) return;


        if (State == Define.ZombieState.AttackBegin || State == Define.ZombieState.Attacking)
        {
            if (HasTarget)
            {
                var lookRotation = Quaternion.LookRotation(_targetEntity.transform.position - transform.position);
                var targetAngleY = lookRotation.eulerAngles.y;

                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY,
                                            ref _turnSmoothVelocity, _turnSmoothTime);
            }
        }

        if (State == Define.ZombieState.Attacking)
        {
            var direction = transform.forward;
            var deltaDistance = _agent.velocity.magnitude * Time.deltaTime;

            var size = Physics.SphereCastNonAlloc(_attackRoot.position, _attackRadius, direction, _hits, deltaDistance,
                _whatIsTarget);

            for (var i = 0; i < size; i++)
            {
                var attackTargetEntity = _hits[i].collider.GetComponent<LivingEntity>();

                if (attackTargetEntity != null && !_lastAttackedTargets.Contains(attackTargetEntity))
                {
                    Vector3 hitPoint;
                    Vector3 hitNormal;

                    if (_hits[i].distance <= 0f)
                    {
                        hitPoint = _attackRoot.position;
                    }
                    else
                    {
                        hitPoint = _hits[i].point;
                    }

                    hitNormal = _hits[i].normal;

                    Managers.Sound.Play("Zombie Bite");
                    attackTargetEntity.OnDamage(_damage, hitPoint, hitNormal, gameObject);

                    _lastAttackedTargets.Add(attackTargetEntity);
                    break;
                }
            }
        }

        if ((State == Define.ZombieState.Idle || State == Define.ZombieState.Patrol) && !HasTarget)
        {
            // 20 ������ �������� ���� ������ ���� �׷�����, ���� ��ġ�� ��� �ݶ��̴��� ������
            // ��, whatIsTarget ���̾ ���� �ݶ��̴��� ���������� ���͸�
            var colliders = Physics.OverlapSphere(transform.position, _sensingRange, _whatIsTarget);

            // ��� �ݶ��̴����� ��ȸ�ϸ鼭, ����ִ� LivingEntity ã��
            foreach (var collider in colliders)
            {
                var livingEntity = collider.GetComponent<LivingEntity>();

                // LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ����ִٸ�,
                if (livingEntity != null && !livingEntity.Dead && livingEntity.GetComponent<PlayerController>().State != Define.PlayerState.Hide)
                {
                    Debug.Log("scream");
                    // ���� ����� �ش� LivingEntity�� ����
                    StartCoroutine("Scream", livingEntity);

                    // �÷��̾� �߰� ���·� ����
                    livingEntity.gameObject.GetComponent<PlayerController>().State = Define.PlayerState.Detec;

                    // for�� ���� ��� ����
                    break;
                }
            }
        }
    }

    IEnumerator Scream(LivingEntity target)
    {
        Managers.Sound.Play($"Zombie Scream{Random.Range(1,3)}");
        State = Define.ZombieState.Scream;
        yield return new WaitForSeconds(2.5f);
        _targetEntity = target;
        State = Define.ZombieState.Tracking;
    }

    // �ֱ������� ������ ����� ��ġ�� ã�� ��θ� ����
    IEnumerator UpdatePath()
    {
        // ����ִ� ���� ���� ����
        while (!Dead)
        {
            // ������ �ް� �ִ� ���� ��� ��ΰ��� X
            if (_stunTime > 0.0f)
                yield return new WaitForSeconds(0.05f);

            if (HasTarget && !IsTargetHiding)
            {
                if (State == Define.ZombieState.Idle || State == Define.ZombieState.Patrol)
                    State = Define.ZombieState.Tracking;

                // ���� ��� ���� : ��θ� �����ϰ� AI �̵��� ��� ����
                _agent.SetDestination(_targetEntity.transform.position);
            }
            else
            {
                if (!HasTarget)
                    _targetEntity = null;

                if (IsTargetHiding)
                {
                    State = Define.ZombieState.Idle;
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    yield return new WaitForSeconds(10.0f);
                    if (IsTargetHiding)
                    {
                        _targetEntity = null;
                        State = Define.ZombieState.Patrol;
                    }
                    else
                        State = Define.ZombieState.Tracking;
                }

                //if (State != Define.ZombieState.Idle && State != Define.ZombieState.Patrol)
                //    State = Define.ZombieState.Idle;

                if (State == Define.ZombieState.Patrol)
                {
                    _agent.SetDestination(_startPosition);

                    if (_agent.remainingDistance <= 1f)
                        State = Define.ZombieState.Idle;
                }

            }

            // 0.05 �� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.05f);
        }
    }

    // �������� �Ծ����� ������ ó��
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, GameObject damager)
    {
        base.OnDamage(damage, hitPoint, hitNormal, damager);
        Managers.Resource.Instantiate("Particle/Flesh Impact", hitPoint, Quaternion.LookRotation(hitNormal), transform);

        if (!Dead)
        {
            Managers.Sound.Play("Zombie Damage");

            State = Define.ZombieState.Damage;
            _stunTime = 2.0f;

            if (_targetEntity == null)
            {
                _targetEntity = damager.GetComponent<LivingEntity>();
                damager.GetComponent<PlayerController>().State = Define.PlayerState.Detec;
            }
        }
    }

    public void BeginAttack()
    {
        if (HasTarget && !IsTargetHiding)
            State = Define.ZombieState.AttackBegin;
    }

    public void EnableAttack()
    {
        if (HasTarget && !IsTargetHiding)
            State = Define.ZombieState.Attacking;

        _lastAttackedTargets.Clear();
    }

    public void DisableAttack()
    {
        if (HasTarget && !IsTargetHiding)
        {
            State = Define.ZombieState.Tracking;
        }
        else if (!IsTargetHiding)
        {
            State = Define.ZombieState.AttackBegin;
        }
    }

    // ��� ó��
    public override void Die()
    {
        // LivingEntity�� Die()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        // �ٸ� AI���� �������� �ʵ��� �ڽ��� ��� �ݶ��̴����� ��Ȱ��ȭ
        GetComponent<Collider>().enabled = false;

        // AI ������ �����ϰ� ����޽� ������Ʈ�� ��Ȱ��ȭ
        //_agent.enabled = false;
        _agent.isStopped = true;

        _sensingRoot.gameObject.SetActive(false);

        // ��� �ִϸ��̼� ���
        _animator.applyRootMotion = true;
        _animator.SetTrigger("Die");

        // ��� ȿ���� ���
        Managers.Sound.Play("Zombie Die");
    }
}
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

    NavMeshAgent _agent; // 경로계산 AI 에이전트
    Animator _animator; // 애니메이터 컴포넌트

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

    LivingEntity _targetEntity; // 추적할 대상
    [SerializeField]
    LayerMask _whatIsTarget; // 추적 대상 레이어


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

    // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(ZombieData data)
    {
        // 체력 설정
        this.StartingHealth = data.health;
        this.Health = data.health;

        // 내비메쉬 에이전트의 이동 속도 설정
        this._runSpeed = data.runSpeed;
        this._patrolSpeed = data.patrolSpeed;

        this._damage = data.damage;
    }

    private void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (Dead) return;

        if (_stunTime > 0.0f) // 데미지 받고 있는 중
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

        // 추적 대상의 존재 여부에 따라 다른 애니메이션을 재생
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
            // 20 유닛의 반지름을 가진 가상의 구를 그렸을때, 구와 겹치는 모든 콜라이더를 가져옴
            // 단, whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
            var colliders = Physics.OverlapSphere(transform.position, _sensingRange, _whatIsTarget);

            // 모든 콜라이더들을 순회하면서, 살아있는 LivingEntity 찾기
            foreach (var collider in colliders)
            {
                var livingEntity = collider.GetComponent<LivingEntity>();

                // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아있다면,
                if (livingEntity != null && !livingEntity.Dead && livingEntity.GetComponent<PlayerController>().State != Define.PlayerState.Hide)
                {
                    Debug.Log("scream");
                    // 추적 대상을 해당 LivingEntity로 설정
                    StartCoroutine("Scream", livingEntity);

                    // 플레이어 발각 상태로 변경
                    livingEntity.gameObject.GetComponent<PlayerController>().State = Define.PlayerState.Detec;

                    // for문 루프 즉시 정지
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

    // 주기적으로 추적할 대상의 위치를 찾아 경로를 갱신
    IEnumerator UpdatePath()
    {
        // 살아있는 동안 무한 루프
        while (!Dead)
        {
            // 데미지 받고 있는 중일 경우 경로갱신 X
            if (_stunTime > 0.0f)
                yield return new WaitForSeconds(0.05f);

            if (HasTarget && !IsTargetHiding)
            {
                if (State == Define.ZombieState.Idle || State == Define.ZombieState.Patrol)
                    State = Define.ZombieState.Tracking;

                // 추적 대상 존재 : 경로를 갱신하고 AI 이동을 계속 진행
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

            // 0.05 초 주기로 처리 반복
            yield return new WaitForSeconds(0.05f);
        }
    }

    // 데미지를 입었을때 실행할 처리
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

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 다른 AI들을 방해하지 않도록 자신의 모든 콜라이더들을 비활성화
        GetComponent<Collider>().enabled = false;

        // AI 추적을 중지하고 내비메쉬 컴포넌트를 비활성화
        //_agent.enabled = false;
        _agent.isStopped = true;

        _sensingRoot.gameObject.SetActive(false);

        // 사망 애니메이션 재생
        _animator.applyRootMotion = true;
        _animator.SetTrigger("Die");

        // 사망 효과음 재생
        Managers.Sound.Play("Zombie Die");
    }
}
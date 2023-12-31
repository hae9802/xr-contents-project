using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnemyScripts
{
    public class NEnemyController : MonoBehaviour
    {
        private Fsm fsm;
        private Fsm fsmLife;

        [Header("체력 조정")]
        [SerializeField] private ReferenceValueT<float> myHp;
        
        [Header("공격 대미지 조정")]
        [SerializeField] private ReferenceValueT<float> myAttackDamage;
        
        [Header("탐지 거리 조정")]
        [SerializeField] private ReferenceValueT<float> myTraceRange;
        
        [Header("공격 거리 조정")]
        [SerializeField] private ReferenceValueT<float> myAttackRange;
        
        [Header("속도 조정")]
        [SerializeField] private ReferenceValueT<float> myMoveSpeed;

        [Header("대기 시간을 조정")] 
        [SerializeField] private ReferenceValueT<float> waitTime;

        [Header("타입 지정(일반 몬스터 None)")] 
        [SerializeField] private ReferenceValueT<EEliteType> myType;
        
        [Header("경직 시간을 조정")] [Range(0.1f,0.5f)]
        [SerializeField] private float hitTime;

        [Header("넉백 거리를 조정")] [Range(0.1f, 3.0f)] 
        [SerializeField] private float knockbackPower;

        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isAlive;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isNowAttack;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isJumping;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isGround;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> canJumpNextNode;
        [HideInInspector] [SerializeField] private ReferenceValueT<ENode> myNode;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isTimerWait;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isTimerEnded;

        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isHit;
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isHitOn;
        [HideInInspector] [SerializeField] private ReferenceValueT<float> playerDamage;
        [HideInInspector] [SerializeField] private Transform playerTransform;
        
        [HideInInspector] [SerializeField] private ReferenceValueT<bool> isHitPlayer;
        
        [SerializeField] private List<GameObject> timers;

        private bool isCoroutineOn;
        

        private Blackboard b;
        private SkeletonAnimation anim;

        private void Awake()
        {
            fsm = new Fsm();
            b = new Blackboard();
            anim = gameObject.GetComponent<SkeletonAnimation>();

            playerTransform = GameObject.Find("Player").transform;

            isHitPlayer.Value = false;

            isAlive.Value = true;
            myNode.Value = ENode.Idle;

            isTimerWait.Value = false;
            isTimerEnded.Value = false;

            isHit.Value = false;
            isHitOn.Value = false;
            playerDamage.Value = 0.0f;

            b.AddData("isTimerWait", isTimerWait);
            b.AddData("isTimerEnded", isTimerEnded);
            b.AddData("waitTime", waitTime);
            
            b.AddData("isHitPlayer", isHitPlayer);

            b.AddData("myNode", myNode);
            b.AddData("isAlive", isAlive);
            b.AddData("myHp", myHp);
            b.AddData("myTransform", transform);
            b.AddData("myAttackDamage", myAttackDamage);
            b.AddData("myTraceRange", myTraceRange);
            b.AddData("myAttackRange", myAttackRange);
            b.AddData("playerTransform", playerTransform);
            b.AddData("myMoveSpeed", myMoveSpeed);
            b.AddData("isNowAttack", isNowAttack);
            b.AddData("myType", myType);
            b.AddData("isJumping", isJumping);
            b.AddData("canJumpNextNode", canJumpNextNode);
            b.AddData("isGround", isGround);
            
            b.AddData("knockbackPower", knockbackPower);
            b.AddData("hitTime", hitTime);
            b.AddData("isHit", isHit);
            b.AddData("isHitOn", isHitOn);
            
            b.AddData("playerDamage", playerDamage);
        }

        void Start()
        {
            isCoroutineOn = false;
            
            var wait = new WaitNode();
            var trace = new NormalTraceNode();
            var attack = new NormalAttackNode();
            var jump = new JumpNode();

            wait.enterPlayer = trace;
            
            trace.playerEnter = attack;
            trace.playerExit = wait;
            trace.enterJump = jump;
            
            jump.endJump = trace;
            
            attack.outOfAttackRange = wait;

            fsmLife = new Fsm();
            var alive = new AliveNode();
            var dead = new DeadNode();
            alive.dead = dead;

            fsmLife.Init(b, alive);
            fsm.Init(b, wait);
        }

        private IEnumerator GuardNonDestroy()
        {
            isCoroutineOn = true;
            yield return new WaitForSeconds(2.0f);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (CameraController.Inst.IsNowCutScene) return;

            if (myHp <= 0 && !isCoroutineOn)
            {
                StartCoroutine(GuardNonDestroy());
            }

            if (isNowAttack)
            {
                if (anim.AnimationState.GetCurrent(0).IsComplete && !PlayerManager.Instance.isInvincibility &&
                    !isHitPlayer.Value)
                {
                    var myPos = transform.position;
                    var dir = (playerTransform.position - myPos).normalized;
                    var ePos = new Vector3(dir.x > 0 ? myPos.x + myAttackRange.Value * 1.5f : myPos.x - myAttackRange * 1.5f, myPos.y, 0);
                    EffectController.Inst.PlayEffect(ePos, "NormalMonsterAttack");
                    isHitPlayer.Value = false;
                    PlayerManager.Instance.PlayerDiscountHp(myAttackDamage, transform.position.x);
                    GameManager.Inst.HitPlayer();
                }
            }

            if (isAlive.Value)
            {
                if (isHit) return;

                if (PlayerManager.Instance.GetIsPlayerDead()) return;
                Flip();
                fsm.Update();
                fsmLife.Update();
            }
            else
            {
                transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                transform.GetComponent<Collider2D>().enabled = false;
                
                if (DOTween.IsTweening(this))
                {
                    DOTween.Kill(this);
                }

                if (anim.AnimationName == "Monster_Dead" && anim.AnimationState.GetCurrent(0).IsComplete)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.transform.CompareTag("Ground"))
            {
                isGround.Value = true;
            }
        }
        
        private void Flip()
        {
            var playerTransform = b.GetData<Transform>("playerTransform");
            float dir = playerTransform.position.x - transform.position.x;

            transform.rotation = dir > 0 ? new Quaternion(0, 180, 0, 0) : new Quaternion(0, 0, 0, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, myAttackRange.Value);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, myTraceRange.Value);
        }

        public void TimerSwitch()
        {
            foreach (var obj in timers)
            {
                obj.SetActive(true);
            }
        }

        public Blackboard Data()
        {
            return b;
        }

        public void DiscountHp(float damage)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    SoundManager.Inst.Play("NormalMonsterHit1");
                    break;
                case 1:
                    SoundManager.Inst.Play("NormalMonsterHit2");
                    break;
                case 2:
                    SoundManager.Inst.Play("NormalMonsterHit3");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            b.GetData<ReferenceValueT<ENode>>("myNode").Value = ENode.Hit;

            isHit.Value = true;

            myHp.Value -= damage;
            LogPrintSystem.SystemLogPrint(transform, $"{damage} From Player -> remain {myHp.Value}",
                ELogType.EnemyAI);
            Sequence sequence = DOTween.Sequence();

            // 플레이어와 자신의 포지션을 빼준다 -> 정규화 해준다 -> 속도를 곱한다
            // 자신의 위치와 구한 벡터를 더해준다
            var myPos = transform.position;
            var playerPos = GameObject.Find("Player").transform.position;

            var dirVector = (myPos - playerPos).normalized;

            myPos += dirVector * knockbackPower;

            transform.DOMoveX(myPos.x, hitTime).OnComplete(() =>
            {
                isHit.Value = false;
            }).SetId(this);
        }
    }
}
using System;
using System.Collections.Generic;
using DG.Tweening;
using EnemyScripts;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalTraceNode : TraceNode
{
    public INode playerEnter;
    public INode playerExit;

    public override INode Execute(Blackboard blackboard)
    {
        var type = Trace(blackboard);
        
        var myNode = blackboard.GetData<ReferenceValueT<ENode>>("myNode");

        var myTransform = blackboard.GetData<Transform>("myTransform");
        var playerTransform = blackboard.GetData<Transform>("playerTransform");
        var myPos = myTransform.position;
        var playerPos = playerTransform.position;
        
        var myMoveSpeed = blackboard.GetData<ReferenceValueT<float>>("myMoveSpeed");
        
        var isTimerEnded = blackboard.GetData<ReferenceValueT<bool>>("isTimerEnded");
        var waitTime = blackboard.GetData<ReferenceValueT<float>>("waitTime");
        var isTimerWait = blackboard.GetData<ReferenceValueT<bool>>("isTimerWait");
        var myPosToCamera = Camera.main.WorldToViewportPoint(myPos);
        var myRd = myTransform.GetComponent<Rigidbody2D>();

        if (myPosToCamera.x > 0.1f && myPosToCamera.x < 0.9f)
        {
            if (!isTimerEnded.Value)
            {
                myNode.Value = ENode.SpecialAttackReady;

                myRd.constraints = RigidbodyConstraints2D.FreezePositionX;
                
                if (!isTimerEnded.Value)
                {
                    var timer = myTransform.GetComponentInChildren<WeakTimeController>(true);

                    if (!isTimerWait.Value)
                    {
                        myTransform.GetComponent<NEnemyController>().TimerSwitch();
                        timer.Init(waitTime);
                        isTimerWait.Value = true;
                        return Fsm.GuardNullNode(this, this);
                    }

                    if (!timer.IsEnded) return Fsm.GuardNullNode(this, this);

                    if (!isTimerEnded.Value)
                    {
                        isTimerEnded.Value = true;
                    }

                    timer.Checked();
                }
            }
        }

        myRd.constraints = RigidbodyConstraints2D.None;
        myRd.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        myNode.Value = ENode.Trace;

        var moveDir = (playerPos - myPos).normalized;
        
        if (moveDir.x > 0f)
        {
            moveDir.x = 1.0f;
            var movePos = new Vector2(moveDir.x * myMoveSpeed.Value, myRd.velocity.y);
            myRd.velocity = movePos;
        }
        else
        {
            moveDir.x = -1.0f;
            var movePos = new Vector2(moveDir.x * myMoveSpeed, myRd.velocity.y);
            myRd.velocity = movePos;
        }

        switch (type)
        {
            case ETraceState.PlayerEnter:
                return Fsm.GuardNullNode(this, playerEnter);
            case ETraceState.PlayerExit:
                return Fsm.GuardNullNode(this, playerExit);
            case ETraceState.PlayerTrace:
                return Fsm.GuardNullNode(this, this);
            case ETraceState.NeedJump:
                return Fsm.GuardNullNode(this, enterJump);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class NormalAttackNode : INode
{
    public INode outOfAttackRange;

    public INode Execute(Blackboard blackboard)
    {
        blackboard.GetData<ReferenceValueT<ENode>>("myNode").Value = ENode.NormalAttack;
        var myType = blackboard.GetData<ReferenceValueT<EEliteType>>("myType").Value;

        var isNowAttack = blackboard.GetData<ReferenceValueT<bool>>("isNowAttack");

        var anim = blackboard.GetData<Transform>("myTransform").GetComponent<SkeletonAnimation>();

        // Attack On
        var myTransform = blackboard.GetData<Transform>("myTransform");
        var playerTransform = blackboard.GetData<Transform>("playerTransform");

        var myPos = myTransform.position;

        // 거리 계산을 위한 변수
        var d1 = playerTransform.GetComponent<PlayerManager>().MyRadius;
        var d2 = blackboard.GetData<ReferenceValueT<float>>("myAttackRange").Value;
        var distance = (myPos - playerTransform.position).magnitude;

        var player = playerTransform.GetComponent<PlayerManager>();

        var attackDamage = blackboard.GetData<ReferenceValueT<float>>("myAttackDamage").Value;

        var sequence = DOTween.Sequence();

        if (d1 + d2 < distance)
        {
            return Fsm.GuardNullNode(this, outOfAttackRange);
        }

        isNowAttack.Value = true;

        if (myType == EEliteType.None)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    SoundManager.Inst.Play("NormalMonsterAtk1");
                    break;
                case 1:
                    SoundManager.Inst.Play("NormalMonsterAtk2");
                    break;
                case 2:
                    SoundManager.Inst.Play("NormalMonsterAtk3");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    SoundManager.Inst.Play("RushMonsterAtk1");
                    break;
                case 1:
                    SoundManager.Inst.Play("RushMonsterAtk2");
                    break;
                case 2:
                    SoundManager.Inst.Play("RushMonsterAtk3");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        sequence.SetDelay(1.5f).OnComplete(() => { isNowAttack.Value = false; }).SetId(this);

        return Fsm.GuardNullNode(this, outOfAttackRange);
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using EnemyScripts;


[Serializable]
public class ReferenceValueT<T> where T : struct
{
    public T Value;

    public static implicit operator T(ReferenceValueT<T> v)
    {
        return v.Value;
    }
}

public class Blackboard
{
    private Dictionary<string, object> table = new Dictionary<string, object>();

    public void AddData(string key, object obj)
    {
        if (string.IsNullOrEmpty(key))
            throw new Exception("Error");
        table.Add(key, obj);
    }

    public object GetData(string key) => table[key];

    public T GetData<T>(string key)
    {
        if (!table.ContainsKey(key))
            throw new Exception("Error");
        return (T)table[key];
    }

    public bool TryGetData<T>(string key, out T data) where T : class
    {

        if (!table.ContainsKey(key))
        {
            data = null;
            return false;
        }

        data = (T)table[key];
        return true;

    }

    public bool TryGetDataStruct<T>(string key, out T data) where T : struct
    {
        if (!table.ContainsKey(key))
        {
            data = new T();
            return false;
        }

        if (table[key] is not ReferenceValueT<T> rv) throw new Exception();

        data = rv.Value;
        return true;
    }
}

public class Fsm
{
    private Blackboard blackboard;
    private INode currentNode;

    public void Init(Blackboard blackboard, INode defaultNode)
    {
        currentNode = defaultNode;
        this.blackboard = blackboard;
    }

    public void Update()
    {
        currentNode = currentNode.Execute(blackboard);
    }

    public static INode GuardNullNode(INode current, INode next)
    {
        if (next == null)
        {
            Debug.LogError(current.GetType());
            Debug.Assert(false);
        }

        return next;
    }
}

public enum ENode
{
    Idle,
    Trace,
    NormalAttack,
    Jump,
    SpecialAttackReady,
    SpecialAttack,
    Groggy,
    Dead,
    Hit
}

public interface INode
{
    public INode Execute(Blackboard blackboard);
}

public class WaitNode : INode
{
    public INode enterPlayer;

    public INode Execute(Blackboard blackboard)
    {
        var myTransform = blackboard.GetData<Transform>("myTransform");
        var playerTransform = blackboard.GetData<Transform>("playerTransform");
        var isGround = blackboard.GetData<ReferenceValueT<bool>>("isGround");
        var myNode = blackboard.GetData<ReferenceValueT<ENode>>("myNode");
        var isNowAttack = blackboard.GetData<ReferenceValueT<bool>>("isNowAttack");
        var myRd = myTransform.GetComponent<Rigidbody2D>();

        myNode.Value = ENode.Idle;
        
        
        if (!isGround.Value) return Fsm.GuardNullNode(this, this);
        
        myRd.constraints = RigidbodyConstraints2D.FreezePositionX;
        
        float d1 = playerTransform.GetComponent<PlayerManager>().MyRadius;
        float d2 = blackboard.GetData<ReferenceValueT<float>>("myTraceRange").Value;

        float distance = (myTransform.position - playerTransform.position).magnitude;

        if (isNowAttack.Value)
            return Fsm.GuardNullNode(this, this);

        myRd.constraints = RigidbodyConstraints2D.None;
        myRd.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        return Fsm.GuardNullNode(this, d1 + d2 >= distance ? enterPlayer : this);
    }
}

public enum ETraceState
{
    PlayerEnter,
    PlayerExit,
    PlayerTrace,
    PlayerEnterRush,
    NeedJump
}

public abstract class TraceNode : INode
{
    public INode enterJump;
    
    public ETraceState Trace(Blackboard blackboard)
    {
        Transform myTransform = blackboard.GetData<Transform>("myTransform");
        Transform playerTransform = blackboard.GetData<Transform>("playerTransform");

        float playerRange = playerTransform.GetComponent<PlayerManager>().MyRadius;
        float myAttackRange = blackboard.GetData<ReferenceValueT<float>>("myAttackRange").Value;

        EEliteType myType = blackboard.GetData<ReferenceValueT<EEliteType>>("myType").Value;

        // Distance of Player to Monster
        float distance = (myTransform.position - playerTransform.position).magnitude;
        float traceRange = blackboard.GetData<ReferenceValueT<float>>("myTraceRange").Value;

        // isJumping in True -> Loop this node when End of jump
        float distanceForJump = Mathf.Abs(myTransform.position.x - playerTransform.position.x);
        var isJumping = blackboard.GetData<ReferenceValueT<bool>>("isJumping");
        if (isJumping.Value)
        {
            return ETraceState.PlayerTrace;
        }

        // Distance of Player to Monster is Same -> Check Y Position -> need jump return
        if (distanceForJump <= myAttackRange && myType != EEliteType.Bomb)
        {
            float playerYPos = playerTransform.position.y;
            float myYPos = myTransform.position.y;

            float yPosCalc = Mathf.Abs(playerYPos - myYPos);

            if (yPosCalc >= 2.0f)
                return ETraceState.NeedJump;
        }

        // Player Out of Range
        if (traceRange + playerRange <= distance)
            return ETraceState.PlayerExit;

        // Trace Logic
        if (myType != EEliteType.Rush)
        {
            // Check Attack Range
            return playerRange + myAttackRange >= distance ? ETraceState.PlayerEnter : ETraceState.PlayerTrace; 
        }

        // Rush Monster Normal Attack Range Check
        if (playerRange + myAttackRange >= distance)
            return ETraceState.PlayerEnter;
        
        // From here Only Use Rush Monster
        var hasRemainAttackTime = blackboard.GetData<ReferenceValueT<bool>>("hasRemainAttackTime");
        var isOverRush = blackboard.GetData<ReferenceValueT<bool>>("isOverRush");
        
        // When Rush Attack Not in Cooldown
        if (!hasRemainAttackTime.Value)
        {
            float myRushRange = blackboard.GetData<ReferenceValueT<float>>("myRushRange").Value;
            float myOverRushRange = blackboard.GetData<ReferenceValueT<float>>("myOverRushRange").Value;

            // Check Rush Range
            if (playerRange + myRushRange >= distance)
            {
                // Check Over Rush Range
                if (playerRange + myOverRushRange >= distance)
                    isOverRush.Value = true;

                // Enable Rush Attack
                if (!isOverRush.Value)
                {
                    var camPos = Camera.main.WorldToViewportPoint(myTransform.position);
                    if (camPos.x < 0.9f && camPos.x > 0.1f)
                        return ETraceState.PlayerEnterRush;
                    return ETraceState.PlayerTrace;
                }
            }
        }

        // Rush Attack in Cooldown
        // Check Attack Range for Rush Monster
        return ETraceState.PlayerTrace;
    }

    public abstract INode Execute(Blackboard blackboard);
}

public class JumpNode : INode
{
    public INode endJump;
    
    public INode Execute(Blackboard blackboard)
    {
        // Enemy가 플레이어를 제데로 따라 갈 수 있도록 y값을 판단
        // 상향점프와 하향점프 모두 필요함
        blackboard.GetData<ReferenceValueT<ENode>>("myNode").Value = ENode.Jump;

        var myTransform = blackboard.GetData<Transform>("myTransform");
        var playerTransform = blackboard.GetData<Transform>("playerTransform");
        var isJumping = blackboard.GetData<ReferenceValueT<bool>>("isJumping");

        if (playerTransform.GetComponent<PlayerManager>().GetIsJumping())
        {
            return Fsm.GuardNullNode(this, endJump);
        }

        Vector3 jumpPos = new Vector3(myTransform.position.x,
            playerTransform.position.y, 0);
        
        isJumping.Value = true;
        myTransform.DOJump(jumpPos, 3f, 1, 1f).OnComplete(() =>
        {
            isJumping.Value = false;
        });

        return Fsm.GuardNullNode(this, endJump);
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class FollowerAI : SerializedMonoBehaviour
{
    public Transform target;
    NavMeshAgent agent;
    Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            if (agent.remainingDistance < 2.5f)
            {
                agent.speed = 2f;
            }
            else
            {
                agent.speed = 3.5f;
            }
        }
        animator.SetFloat(Animator.StringToHash("Speed"), agent.velocity.magnitude);
    }

    void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            EnvironmentalAudioManager.Instance.PlaySFX("footstep", transform, true);
        }
    }
}

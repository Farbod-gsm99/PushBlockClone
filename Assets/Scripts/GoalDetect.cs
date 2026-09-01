using Unity.MLAgents;
using UnityEngine;

public class GoalDetect : MonoBehaviour
{

    [HideInInspector] public PushAgentBasic agent;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goal"))
        {
            agent.ScoredAGoal();
        }
    }
}

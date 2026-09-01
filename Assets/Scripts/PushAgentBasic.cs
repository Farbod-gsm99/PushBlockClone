using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.AppUI.Core;

public class PushAgentBasic : Agent
{
    public GameObject ground;

    public GameObject block;

    public GameObject area;

    public GameObject goal;

    [HideInInspector] public GoalDetect goalDetect;

    Rigidbody m_AgentRB;    //cached on initialization
    Rigidbody m_BlockRB;    //cached on initialization
    Material m_GroundMaterial;  //cached on Awake()
    Renderer m_GroundRenderer;

    PushBlockSettings m_PushBlockSettings;
    EnvironmentParameters m_ResetParams;


    [HideInInspector] public Bounds areaBounds;


    protected override void Awake()
    {
        base.Awake();
        m_PushBlockSettings = FindAnyObjectByType<PushBlockSettings>();
    }

    public override void Initialize()
    {
        goalDetect = block.GetComponent<GoalDetect>();
        goalDetect.agent = this;

        m_AgentRB = GetComponent<Rigidbody>();
        m_BlockRB = block.GetComponent<Rigidbody>();
        areaBounds = ground.GetComponent<Collider>().bounds;
        m_GroundRenderer = ground.GetComponent<Renderer>();
        m_GroundMaterial = m_GroundRenderer.material;
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();

    }

    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier);
            var randomPosZ = Random.Range(-areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier);

            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 0.25f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(1.0f, 0.2f, 1.0f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    public void ScoredAGoal()
    {
        AddReward(5.0f);

        EndEpisode();

        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = m_GroundMaterial;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        AddReward(-1.0f / MaxStep);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRB.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed, ForceMode.VelocityChange);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3 (0f, rotationAngle, 0f));

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        m_AgentRB.linearVelocity = Vector3.zero;
        m_AgentRB.angularVelocity = Vector3.zero;

        SetResetParameters();
    }

    public void ResetBlock()
    {
        block.transform.position = GetRandomSpawnPos();
        m_BlockRB.linearVelocity = Vector3.zero;
        m_BlockRB.angularVelocity = Vector3.zero;
    }

    void SetResetParameters()
    {
        SetGroundMaterialFriction();
        SetBlockProperties();
    }

    public void SetGroundMaterialFriction()
    {
        var groundCollider = ground.GetComponent<Collider>();
        groundCollider.material.dynamicFriction = m_ResetParams.GetWithDefault("dynamic_friction", 0);
        groundCollider.material.staticFriction = m_ResetParams.GetWithDefault("static_friction", 0);
    }

    public void SetBlockProperties()
    {
        var scale = m_ResetParams.GetWithDefault("block_scale", 1);
        m_BlockRB.transform.localScale = new Vector3(scale, 0.2f, scale);
        m_BlockRB.linearDamping = m_ResetParams.GetWithDefault("block_drag", 0.5f);
    }

}

using UnityEngine;
using Unity.MLAgents;

public class PushAgentBasic : Agent
{
    public GameObject ground;

    public GameObject block;

    public GameObject area;

    public GameObject goal;

    Rigidbody m_AgentRB;    //cached on initialization
    Rigidbody m_BlockRB;    //cached on initialization
    Material m_GroundMaterial;  //cached on Awake()


    [HideInInspector] public Bounds areabounds;


}

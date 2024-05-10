using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
    public Transform m_Goal;

    public float m_MaxSpeed;
    public float m_MaxTime;

    private Vector3 m_AgentLastPlace;
    private float m_StartingDistance;

    private float m_StrtingTime;
        
    public override void OnEpisodeBegin()
    {
        // Reset the environment

        var agentPos = GetRandomPosition();
        var goalPos = GetRandomPosition();

        transform.localPosition = agentPos;
        m_Goal.localPosition = goalPos;

        m_AgentLastPlace = agentPos;
        m_StartingDistance = (agentPos - goalPos).magnitude;

        m_StrtingTime = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Input Additional data
        sensor.AddObservation((m_Goal.localPosition - transform.position).normalized);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Do action
        var speedX = actionBuffers.ContinuousActions[0];
        var speedZ = actionBuffers.ContinuousActions[1];

        var deltaPos = new Vector3(speedX, 0f, speedZ) * m_MaxSpeed * Time.deltaTime;
        transform.localPosition += deltaPos;

        var distance = (transform.localPosition - m_Goal.localPosition).magnitude;

        // Evaluate Result
        if (distance < 1f)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (Time.time - m_StrtingTime > m_MaxTime)
        {
            EndEpisode();
        }
        else
        {
            var lastDistance = (m_Goal.localPosition - m_AgentLastPlace).magnitude;
            
            var distanceDelta = lastDistance - distance;
            var score = distanceDelta / m_StartingDistance;
            
            AddReward(score);
        }

        m_AgentLastPlace = transform.localPosition;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
    }
}

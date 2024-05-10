using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TTTAgent : Agent
{
    private TTTBoard _board;

    public int m_ID;

    private bool m_IsWaitingForInput;
    private bool m_IsWaitingForDecision;
    private int m_Input;

    public bool m_IsHumanControlled;
    
    public void SetBoard(TTTBoard board)
    {
        _board = board;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log($"Agent {m_ID} has started new episode");
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var state = _board.GetState(m_ID);
        string obs = "";
        foreach (var item in state)
        {
            obs += item.ToString() + " ";
            sensor.AddObservation(item);
        }
        Debug.Log($"Agent {m_ID} has collected observation {obs}");
    }

    public IEnumerator DoStep()
    {
        yield return null;
        m_IsWaitingForDecision = true;
        
        if (m_IsHumanControlled)
        {
            StartCoroutine(WaitForInput());
        }
        else
        {
            RequestDecision();
        }
        
        while (m_IsWaitingForDecision)
        {
            yield return null;
        }
    }

    public void OnBoardClick(int id)
    {
        m_Input = id;
        m_IsWaitingForInput = false;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.DiscreteActions;
        var decisionId = actions[0];
        var res = _board.DoStep(decisionId);
        Debug.Log($"Agent {m_ID} has decided {decisionId} with result {res}");
        var score = _board.GetScore(_board.GetSide(m_ID));
        SetRewards(score, false);
        
        m_IsWaitingForDecision = false;
    }

    IEnumerator WaitForInput()
    {
        var stepIsDone = false;
        while (!stepIsDone)
        {
            m_IsWaitingForInput = true;
            while (m_IsWaitingForInput)  yield return null;
            var res = _board.DoStep(m_Input);
            Debug.Log($"Persone {m_ID} has decided {m_Input} with result {res}");

            if (res > -1) stepIsDone = true;
        }
        
        m_IsWaitingForDecision = false;
    }

    public void SetRewards(float reward, bool endEpisode)
    {
        Debug.Log($"Agent {m_ID} has rewarded {reward} with ending episode {endEpisode}");
        SetReward(reward);
        if(endEpisode) EndEpisode();
    }
    
    public void AddRewards(float reward, bool endEpisode)
    {
        Debug.Log($"Agent {m_ID} has added reward {reward} with ending episode {endEpisode}");
        AddReward(reward);
        if(endEpisode) EndEpisode();
    }
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        string log = "Anabled Actions ";
        var map = _board.GetState(m_ID);
        for (int i = 0; i < map.Length; i++)
        {
            bool state = map[i] == 0;
            actionMask.SetActionEnabled(0, i, state);
            log += $" {state}";
        }
        Debug.Log($"Agent {m_ID}  Enabled Actions: {log}");
    }
    
}

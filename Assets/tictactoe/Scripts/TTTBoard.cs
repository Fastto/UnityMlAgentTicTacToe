using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TTTBoard : MonoBehaviour
{
    public GameObject m_XPrefab;
    public GameObject m_OPrefab;

    public List<TTTAgent> m_Players;
    public List<TTTButton> m_Buttons;

    private bool m_GameIsFinished;
    private int[] m_Map;
    private List<GameObject> m_Figures;

    private bool m_FirstIsFirst = false;
    
    public Action<int> m_OnClick;

    private List<Vector3> m_Coordinates;

    private int m_CurrentPlayerID;

    private int m_LastActionResult;

    public TextMeshProUGUI p1won;
    public TextMeshProUGUI p1mistake;
    public TextMeshProUGUI p2won;
    public TextMeshProUGUI p2mistake;
    public TextMeshProUGUI nobodywon;

    private void Start()
    {
        m_Figures = new List<GameObject>();
        m_Coordinates = new List<Vector3>();
        foreach (var b in m_Buttons)
        {
            b.m_OnClick += i => m_OnClick?.Invoke(i);
            m_Coordinates.Add(b.transform.localPosition);
        }

        foreach (var p in m_Players)
        {
            p.SetBoard(this);
            m_OnClick += p.OnBoardClick;
        }

        StartCoroutine(GameCycle());
    }

    IEnumerator GameCycle()
    {
        yield return null;
        while (true)
        {
            yield return StartCoroutine(EpisodeBegin());
            yield return StartCoroutine(EpisodeBody());
            yield return StartCoroutine(EpisodeEnd());
        }
    }

    IEnumerator EpisodeBegin()
    {
        yield return null;
        m_GameIsFinished = false;
        ClearBoard();
        ConfigPlayers();
    }

    IEnumerator EpisodeBody()
    {
        yield return null;
        m_CurrentPlayerID = m_FirstIsFirst ? 0 : 1;
        while (!m_GameIsFinished)
        {
            yield return StartCoroutine(m_Players[m_CurrentPlayerID].DoStep());

            if (m_LastActionResult == -1)
            {
                m_Players[0].SetRewards(m_CurrentPlayerID == 0 ? -1 : 0, true);
                m_Players[1].SetRewards(m_CurrentPlayerID == 1 ? -1 : 0, true);

                if (m_CurrentPlayerID == 0)
                {
                    int newval = int.Parse(p1mistake.text) + 1;
                    p1mistake.text = newval.ToString();
                }
                else
                {
                    int newval = int.Parse(p2mistake.text) + 1;
                    p2mistake.text = newval.ToString();
                }
                
                m_GameIsFinished = true;
                continue;
            }
            
            //check step result
            if (IsXWon())
            {
                m_Players[0].SetRewards(m_FirstIsFirst ? 1 : -1, true);
                m_Players[1].SetRewards(m_FirstIsFirst ? -1 : 1, true);

                if (m_FirstIsFirst)
                {
                    int newval = int.Parse(p1won.text) + 1;
                    p1won.text = newval.ToString();
                }
                else
                {
                    int newval = int.Parse(p2won.text) + 1;
                    p2won.text = newval.ToString();
                }
                
                m_GameIsFinished = true;
                continue;
            }

            if (IsYWon())
            {
                m_Players[0].SetRewards(m_FirstIsFirst ? -1 : 1, true);
                m_Players[1].SetRewards(m_FirstIsFirst ? 1 : -1, true);
                
                if (!m_FirstIsFirst)
                {
                    int newval = int.Parse(p1won.text) + 1;
                    p1won.text = newval.ToString();
                }
                else
                {
                    int newval = int.Parse(p2won.text) + 1;
                    p2won.text = newval.ToString();
                }

                
                m_GameIsFinished = true;
                continue;
            }

            if (IsFinishedWithoutWinner())
            {
                m_Players[0].SetRewards(0, true);
                m_Players[1].SetRewards(0, true);
                
                int newval = int.Parse(nobodywon.text) + 1;
                nobodywon.text = newval.ToString();
                
                m_GameIsFinished = true;
                continue;
            }

            m_CurrentPlayerID = m_CurrentPlayerID == 0 ? 1 : 0;
            yield return null;
        }
    }

    public int GetSide(int playerId)
    {
        var side = (m_FirstIsFirst && playerId == 0) || (!m_FirstIsFirst && playerId == 1) ? 1 : -1;
        return side;
    }
    
    IEnumerator EpisodeEnd()
    {
        yield return null;
    }

    private void ClearBoard()
    {
        m_Map = new int[9];
        for (int i = 0; i < m_Figures.Count; i++)
        {
            Destroy(m_Figures[i]);
        }
        m_Figures.Clear();
    }
    
    private void ConfigPlayers()
    {
        m_FirstIsFirst = !m_FirstIsFirst;
    }

    public int DoStep(int id)
    {
        m_LastActionResult = 0;
        if (m_Map[id] != 0)
        {
            m_LastActionResult = -1;
            return m_LastActionResult;
        }

        bool isX = (m_FirstIsFirst && m_CurrentPlayerID == 0) 
                   || (!m_FirstIsFirst && m_CurrentPlayerID == 1);
        
        var go = Instantiate(isX ? m_XPrefab : m_OPrefab, m_Coordinates[id], Quaternion.identity, transform);
        go.transform.localPosition = m_Coordinates[id];
        m_Figures.Add(go);
        m_Map[id] = isX ? 1 : -1;
        
        return m_LastActionResult;
    }

    public int[] GetState(int playerId)
    {
        //all figures of the requested player equals 1
        //all figures of the opposit player equals -1
        //all empty fiels equals 0

        int reverse = 1;
        int[] map = new int[9];

        if (m_FirstIsFirst && playerId == 1 
            || !m_FirstIsFirst && playerId == 0)
        {
            reverse = -1;
        }
        
        for (int i = 0; i < m_Map.Length; i++)
        {
            map[i] = m_Map[i] * reverse;
        }
        
        return map;
    }

    private bool IsXWon()
    {
        bool[] arr = new bool[9];
        for (int i = 0; i < m_Map.Length; i++)
        {
            arr[i] = m_Map[i] == 1 ;
        }

        return IsWon(arr);
    }
    
    private bool IsYWon()
    {
        bool[] arr = new bool[9];
        for (int i = 0; i < m_Map.Length; i++)
        {
            arr[i] = m_Map[i] == -1;
        }
        
        return IsWon(arr);
    }

    private bool IsWon(bool[] map)
    {
        return (map[0] && map[1] && map[2])
                     || (map[3] && map[4] && map[5])
                     || (map[6] && map[7] && map[8])
                     || (map[0] && map[3] && map[6])
                     || (map[1] && map[4] && map[7])
                     || (map[2] && map[5] && map[8])
                     || (map[0] && map[4] && map[8])
                     || (map[2] && map[4] && map[6]);
    }

    private bool IsFinishedWithoutWinner()
    {
        return !HasEmptyCells() && !IsXWon() && !IsYWon();
    }

    private bool HasEmptyCells()
    {
        bool hasEmptyCells = false;
        for (int i = 0; i < m_Map.Length; i++)
        {
            if (m_Map[i] == 0)
            {
                hasEmptyCells = true;
                break;
            }
        }

        return hasEmptyCells;
    }

    public float GetScore(int side)
    {
        bool[] arr = new bool[9];
        for (int i = 0; i < m_Map.Length; i++)
        {
            arr[i] = m_Map[i] == side || m_Map[i] == 0;
        }
        var myScore = GetScore(arr);
        
        arr = new bool[9];
        side = side * -1;
        for (int i = 0; i < m_Map.Length; i++)
        {
            arr[i] = m_Map[i] == side || m_Map[i] == 0;
        }
        var opponentScore = GetScore(arr);

        if (myScore + opponentScore == 0)
            return 0;

        var score = (float)myScore / ((float)myScore + (float)opponentScore);
        score *= 2f;
        score -= 1f;
        return score;
    }

    private int GetScore(bool[] map)
    {
        int score = 0;
        score += map[0] && map[1] && map[2] ? 1 : 0;
        score += map[3] && map[4] && map[5] ? 1 : 0;
        score += map[6] && map[7] && map[8] ? 1 : 0;
        score += map[0] && map[3] && map[6] ? 1 : 0;
        score += map[1] && map[4] && map[7] ? 1 : 0;
        score += map[2] && map[5] && map[8] ? 1 : 0;
        score += map[0] && map[4] && map[8] ? 1 : 0;
        score += map[2] && map[4] && map[6] ? 1 : 0;

        return score;
    }
}

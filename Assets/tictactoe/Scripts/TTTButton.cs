using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTTButton : MonoBehaviour
{
    public int m_ID;
    public Action<int> m_OnClick;
    void OnMouseDown()
    {
        m_OnClick?.Invoke(m_ID);
    }
    
}

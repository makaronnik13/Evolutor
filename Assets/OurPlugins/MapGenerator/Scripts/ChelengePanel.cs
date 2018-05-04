﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tribus
{
public class ChelengePanel : MonoBehaviour {

    private GameObject _visual;
    private GameObject visual
    {
        get
        {
            if (!_visual)
            {
                _visual = transform.GetChild(0).gameObject;
            }
            return _visual;
        }
    }

    // Use this for initialization
    void Start()
    {
        FindObjectOfType<RoomActivator>().OnChellengeIn += ShowPanel;
    }

    private void ShowPanel(Chalenge chellenge)
    {
        visual.SetActive(true);
    }

    public void HidePanel()
    {
        visual.SetActive(false);
    }
}
}

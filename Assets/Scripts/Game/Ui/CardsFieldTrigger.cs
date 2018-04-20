﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardsFieldTrigger : Singleton<CardsFieldTrigger>, IPointerEnterHandler, IPointerExitHandler
{
	public CardVisual activeCardVisual;

	public void OnPointerEnter (PointerEventData eventData)
	{
		if(!eventData.pointerDrag)
		{
			return;
		}
		activeCardVisual = eventData.pointerDrag.GetComponent<CardVisual> ();

        if (activeCardVisual)
		{
            activeCardVisual.SetState(CardVisual.CardState.ChosingAim);
        }
	}

	public void OnPointerExit (PointerEventData eventData)
	{
		if (activeCardVisual && (activeCardVisual.State == CardVisual.CardState.Dragging || activeCardVisual.State == CardVisual.CardState.ChosingAim) && !CardsManager.Instance.ChooseManager.Choosing) 
		{
            activeCardVisual.SetState(CardVisual.CardState.Dragging);
			activeCardVisual = null;
		}
	}
}

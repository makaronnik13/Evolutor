﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardsManager : Singleton<CardsManager> {

	public enum ChooseType
	{
		Simple,
		Drag
	}

	public ChooseType chooseType = ChooseType.Simple;

    public GameObject CardPrefab;
    public Transform dropTransform, pileTransform, handTransform, activationSlotTransform, chooseCardField;

	public CardsLayout HandCardsLayout;
	public ChoseCardsLayout ChoseCardsLayout;

	public Action<CardVisual> OnCardTaken = (CardVisual visual)=>{};
	public Action<CardVisual> OnCardDroped = (CardVisual visual)=>{};
	public Action<CardVisual> OnCardTakenInChooseField = (CardVisual visual)=>{};

	private Action<List<CardVisual>> onChoseCardFieldClosed;
	private List<CardVisual> cardsInChoseCardField = new List<CardVisual>();
	private List<CardVisual> chosenCards
	{
		get
		{
			return ChoseCardsLayout.Instance.chosedCards;
		}
	}

	public bool CardDragging
	{
		get
		{
			foreach(CardVisual cv in HandCardsLayout.Cards)
			{
				if(cv.State == CardVisual.CardState.Dragging || cv.State == CardVisual.CardState.ChosingAim)
				{
					return true;
				}
			}
			return false;
		}
	}

    private Canvas _playerCanvas;
	public Canvas playerCanvas
    {
        get
        {
            if (!_playerCanvas)
            {
                _playerCanvas = GetComponentInParent<Canvas>();
            }
            return _playerCanvas;
        }
    }

 

    public void GetCard(Card card)
    {
	    GameObject newCard = Instantiate (CardPrefab);
	    OnCardTaken.Invoke (newCard.GetComponent<CardVisual> ());
	    newCard.GetComponent<CardVisual> ().Init (card);
	    newCard.transform.SetParent (pileTransform);
	    newCard.transform.localPosition = Vector3.zero;
	    newCard.transform.localRotation = Quaternion.identity;
	    newCard.transform.localScale = Vector3.one;
	    newCard.transform.SetParent (handTransform);
	    HandCardsLayout.CardsReposition ();
    }

    public Vector3 GetPosition(CardVisual cardVisual, bool hovered = false)
    {
		return HandCardsLayout.GetPosition(cardVisual, hovered);
    }

    public Quaternion GetRotation(CardVisual cardVisual, bool hovered = false)
    {
		return HandCardsLayout.GetRotation(cardVisual, hovered);
    }

	public Vector3 GetChoosePosition(CardVisual cardVisual, bool hovered = false)
	{
		return ChoseCardsLayout.GetPosition(cardVisual, hovered);
	}

	public Quaternion GetChooseRotation(CardVisual cardVisual, bool hovered = false)
	{
		return ChoseCardsLayout.GetRotation(cardVisual, hovered);
	}

    public void DropCard(CardVisual cardVisual)
    {
        NetworkCardGameManager.sInstance.DropCard(cardVisual.CardAsset);
            OnCardDroped.Invoke (cardVisual);		
            Destroy(cardVisual.gameObject);
    }

	public void MoveCardTo(Transform card, Transform aim, Action callback = null)
	{
		MoveCardTo(card, aim, (CardVisual visual)=>{callback.Invoke();});
	}

	public void MoveCardTo(Transform card, Transform aim, Action<CardVisual> callback = null)
	{
		StartCoroutine (MoveCard(card, aim, callback));
	}

	IEnumerator MoveCard(Transform card, Transform aim, Action<CardVisual> callback = null)
    {
        float time = 0;
        while (card.position!=aim.position)
        {
            card.position = Vector3.Lerp(card.position, aim.position, time);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (callback!=null)
        {
			callback.Invoke(card.GetComponent<CardVisual>());
        }
    }

	public void FillChooseCardField(List<Card> cards, int max, Action<List<CardVisual>> callback = null, bool simpleCardChoose = true)
	{
		if (simpleCardChoose) 
		{
			chooseType = ChooseType.Simple;
		} else 
		{
			chooseType = ChooseType.Drag;
		}
			

		ChoseCardsLayout.Instance.Choosing = true;
		onChoseCardFieldClosed = callback;

		foreach(Card c in cards)
		{
			GameObject newCard = Instantiate (CardPrefab);
			OnCardTakenInChooseField.Invoke (newCard.GetComponent<CardVisual> ());
			newCard.GetComponent<CardVisual> ().Init (c);
			newCard.GetComponent<CardVisual> ().State = CardVisual.CardState.Choosing;
			cardsInChoseCardField.Add (newCard.GetComponent<CardVisual> ());
			newCard.transform.SetParent (chooseCardField);
			newCard.transform.localPosition = Vector3.zero;
			newCard.transform.localRotation = Quaternion.identity;
			newCard.transform.localScale = Vector3.one;
			ChoseCardsLayout.CardsReposition ();
		}
		ChoseCardsLayout.Instance.SetMax (max);
		ChoseCardsLayout.Instance.CardsReposition ();
	}

	public void HideChooseCardField()
	{
		ChoseCardsLayout.Instance.Choosing = false;
		if(onChoseCardFieldClosed!=null)
		{
			Action<List<CardVisual>> lastCallback = onChoseCardFieldClosed;
			onChoseCardFieldClosed = null;
			lastCallback(chosenCards);
		}
	}
}
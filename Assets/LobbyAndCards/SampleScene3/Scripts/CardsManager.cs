﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardsManager : Singleton<CardsManager> {

    public GameObject CardPrefab;
    public Transform dropTransform, pileTransform, handTransform;

	private List<CardVisual> cardsInHand = new List<CardVisual>();
	public int CardsCount
	{
		get
		{
			cardsInHand.RemoveAll (c=>c == null);
			return cardsInHand.Count;
		}
	}

    private Queue<Card> pile = new Queue<Card>();
    private List<Card> drop = new List<Card>();
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
		
	public void GeneratePile(List<Card> cards)
    {
		List<Card> sorted = cards.OrderBy(a => Guid.NewGuid()).ToList();
        pile = new Queue<Card>(sorted);
    }

    [ContextMenu("Get card")]
    public void GetCard()
    {
        GameObject newCard = Instantiate(CardPrefab);
        newCard.GetComponent<CardVisual>().Init(pile.Dequeue());
		cardsInHand.Add (newCard.GetComponent<CardVisual>());
        newCard.transform.SetParent(pileTransform);
        newCard.transform.localPosition = Vector3.zero;
        newCard.transform.localScale = Vector3.one;
        if (pile.Count == 0)
        {
            Resuffle();
        }

		newCard.transform.SetParent(handTransform);
    }

    public void DropCard(CardVisual cardVisual)
    {
            drop.Add(cardVisual.CardAsset);
            Destroy(cardVisual.gameObject);
    }

    private void Resuffle()
    {
        List<Card> sorted = drop.OrderBy(a => Guid.NewGuid()).ToList();
        pile = new Queue<Card>(sorted);
        drop.Clear();
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
}
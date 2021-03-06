﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class EditingDeckView : MonoBehaviour {

	public TMP_InputField DeckName;
	public Transform Dock;
	public GameObject CardPrefab;
	private List<DeckCardPanel> cardPanels = new List<DeckCardPanel> ();
	private DeckStruct deck;

	public void Init (DeckStruct ds) 
	{
		foreach(DeckCardPanel lcp in cardPanels)
		{
            Lean.Pool.LeanPool.Despawn(lcp.gameObject);
		}
		cardPanels.Clear ();

		deck = ds;
		DeckName.text = ds.DeckName;
		foreach(DeckCardPanel dcp in cardPanels)
		{
            Lean.Pool.LeanPool.Despawn(dcp.gameObject);
		}

		foreach(string c in ds.Cards)
		{
			AddCard (c);
		}
	}

	public void RemoveCard(Card card)
	{
		DeckCardPanel dcp = cardPanels.FirstOrDefault (cardPanel=>cardPanel.Card == card);
		if(dcp)
		{
			dcp.Remove (()=>
			{
				cardPanels.Remove(dcp);
                Lean.Pool.LeanPool.Despawn(dcp.gameObject);
			});
		}
	}

	public void AddCard(string c)
	{
		DeckCardPanel dcp = cardPanels.FirstOrDefault (cardPanel=>cardPanel.Card.name == c);
		if (dcp) 
		{
			dcp.Add ();
		} 
		else 
		{
			dcp = Lean.Pool.LeanPool.Spawn(CardPrefab).GetComponent<DeckCardPanel>();
			dcp.Init (DefaultResourcesManager.GetCardById(c));
			dcp.transform.SetParent (Dock);
			dcp.transform.localScale = Vector3.one;
			cardPanels.Add (dcp);
		}
	}

	public void Rename()
	{
		deck.DeckName = DeckName.text;
		GetComponentInParent<DeckEditPanel> ().SaveDeck ();
	}
}

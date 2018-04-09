﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DefaultResourcesManager
{
	private static string AtlasPath = "DefaultResources/PortraitsAtlas";
	private static string NamesAssetPath = "DefaultResources/PlayerNames";
	private static string ColorsAssetPath = "DefaultResources/PlayerColors";
	private static string DeckAssetPath = "DefaultResources/StartingDeck";
	private static string AllCardsAssetPath = "DefaultResources/AllCards";

	private static Card[] allCards;
	public static Card[] AllCards
	{
		get
		{
			if (allCards == null) 
			{
				allCards = Resources.Load<Deck> (AllCardsAssetPath).DeckStruct.Cards.ToArray();
			}
			return allCards;
		}
	}

	private static DeckStruct startingDeck;
	public static DeckStruct StartingDeck
	{
		get
		{
			if (startingDeck == null) 
			{
				startingDeck = Resources.Load<Deck> (DeckAssetPath).DeckStruct;
			}

			DeckStruct newDeck = new DeckStruct ();
			newDeck.DeckName = startingDeck.DeckName;
			newDeck.Cards = new List<Card>(startingDeck.Cards);
			return newDeck;
		}
	}

	private static string[] names = new string[0];
	public static string[] Names
	{
		get
		{
			if(names.Length == 0)
			{
				names = Resources.Load<StringsCollection> (NamesAssetPath).Names.ToArray();
			}
			return names;
		}
	}

	private static Color[] colors = new Color[0];
	public static Color[] Colors
	{
		get
		{
			if(colors.Length == 0)
			{
				colors = Resources.Load<ColorsCollection> (ColorsAssetPath).Colors.ToArray();
			}
			return colors;
		}
	}
		
	private static Sprite[] avatars = new Sprite[0];
	public static Sprite[] Avatars
	{
		get
		{
			if(avatars.Length == 0)
			{
				avatars = Resources.LoadAll<Sprite>(AtlasPath);
			}
			return avatars;
		}
	}

	public static Sprite GetRandomAvatar()
	{
		return Avatars[Random.Range(0, Avatars.Length-1)];
	}

	public static string GetRandomName()
	{
		return Names[Random.Range(0, Names.Length-1)];
	}

	public static Color GetRandomColor()
	{
		return Colors[Random.Range(0, Colors.Length-1)];
	}
}
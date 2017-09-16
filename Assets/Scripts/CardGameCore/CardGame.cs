﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public delegate void LoadJTokenDelegate(JToken jToken,string defaultValue);

[JsonObject(MemberSerialization.OptIn)]
public class CardGame
{
    public const string AllSetsFileName = "AllSets.json";
    public const string AllCardsFileName = "AllCards.json";
    public const string BackgroundImageFileName = "Background";
    public const string CardBackImageFileName = "CardBack";
    public const string DefaultCardImageURLFormat = "{0}";
    public const int DefaultCopiesOfCardPerDeck = 4;
    public const int DefaultDeckCardStackCount = 15;
    public const string DefaultDeckFileType = "txt";
    public const string DefaultImageFileType = "png";
    public const string DefaultSet = "_CGSDEFAULT_";
    public const string SetCardsIdentifier = "cards";

    public string FilePathBase {
        get { return CardGameManager.GamesFilePathBase + "/" + Name; }
    }

    public string ConfigFilePath {
        get { return FilePathBase + "/" + Name + ".json"; }
    }

    public string DecksFilePath {
        get  { return FilePathBase + "/decks"; }
    }

    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public string AllCardsURL { get; set; }

    [JsonProperty]
    public bool AllCardsZipped { get; set; }

    [JsonProperty]
    public string AllSetsURL { get; set; }

    [JsonProperty]
    public bool AllSetsZipped { get; set; }

    [JsonProperty]
    public bool AutoUpdate { get; set; }

    [JsonProperty]
    public string AutoUpdateURL { get; set; }

    [JsonProperty]
    public string BackgroundImageFileType { get; set; }

    [JsonProperty]
    public string BackgroundImageURL { get; set; }

    [JsonProperty]
    public string CardBackImageFileType { get; set; }

    [JsonProperty]
    public string CardBackImageURL { get; set; }

    [JsonProperty]
    public string CardIdIdentifier { get; set; }

    [JsonProperty]
    public string CardImageFileType { get; set; }

    [JsonProperty]
    public string CardImageURLBase { get; set; }

    [JsonProperty]
    public string CardImageURLFormat { get; set; }

    [JsonProperty]
    public string CardNameIdentifier { get; set; }

    [JsonProperty]
    public string CardSetIdentifier { get; set; }

    [JsonProperty]
    public string CardPrimaryProperty { get; set; }

    [JsonProperty]
    public List<PropertyDef> CardProperties { get; set; }

    [JsonProperty]
    public int CopiesOfCardPerDeck { get; set; }

    [JsonProperty]
    public int DeckCardStackCount { get; set; }

    [JsonProperty]
    public string DeckFileType { get; set; }

    [JsonProperty]
    public string SetCodeIdentifier { get; set; }

    [JsonProperty]
    public string SetNameIdentifier { get; set; }

    private List<Set> _sets;
    private List<Card> _cards;
    private Sprite _backgroundImageSprite;
    private Sprite _cardBackImageSprite;
    private bool _isLoaded;
    private string _error;

    public CardGame(string name, string url)
    {
        Name = name;
        AutoUpdateURL = url;

        BackgroundImageFileType = DefaultImageFileType;
        CardBackImageFileType = DefaultImageFileType;
        CardImageURLFormat = DefaultCardImageURLFormat;
        CardImageFileType = DefaultImageFileType;
        CardIdIdentifier = "id";
        CardNameIdentifier = "name";
        CardSetIdentifier = "set";
        CopiesOfCardPerDeck = DefaultCopiesOfCardPerDeck;
        DeckCardStackCount = DefaultDeckCardStackCount;
        DeckFileType = DefaultDeckFileType;
        SetCodeIdentifier = "code";
        SetNameIdentifier = "name";
    }

    public IEnumerator Load()
    {
        if (!string.IsNullOrEmpty(AutoUpdateURL) && (AutoUpdate || !File.Exists(ConfigFilePath)))
            yield return UnityExtensionMethods.SaveURLToFile(AutoUpdateURL, ConfigFilePath);
        try { 
            JsonConvert.PopulateObject(File.ReadAllText(ConfigFilePath), this);
        } catch (Exception e) {
            Debug.LogError("Failed to load card game! Error: " + e.Message + e.StackTrace);
            _error = e.Message;
            yield break;
        }

        string setsFile = FilePathBase + "/" + AllSetsFileName;
        if (!string.IsNullOrEmpty(AllSetsURL) && (AutoUpdate || !File.Exists(setsFile))) {
            yield return UnityExtensionMethods.SaveURLToFile(AllSetsURL, AllSetsZipped ? setsFile + ".zip" : setsFile);
            if (AllSetsZipped)
                UnityExtensionMethods.ExtractZip(setsFile + ".zip", FilePathBase);
        }
        string cardsFile = FilePathBase + "/" + AllCardsFileName;
        if (!string.IsNullOrEmpty(AllCardsURL) && (AutoUpdate || !File.Exists(cardsFile))) {
            yield return UnityExtensionMethods.SaveURLToFile(AllCardsURL, AllCardsZipped ? cardsFile + ".zip" : cardsFile);
            if (AllCardsZipped)
                UnityExtensionMethods.ExtractZip(cardsFile + ".zip", FilePathBase);
        }
        try {
            LoadJSONFromFile(setsFile, LoadSetFromJToken);
            LoadJSONFromFile(cardsFile, LoadCardFromJToken);
        } catch (Exception e) {
            Debug.LogError("Failed to load card game data! Error: " + e.Message + e.StackTrace);
            _error = e.Message;
            yield break;
        }

        Sprite backgroundSprite = null;
        yield return UnityExtensionMethods.RunOutputCoroutine<Sprite>(UnityExtensionMethods.CreateAndOutputSpriteFromImageFile(FilePathBase + "/" + BackgroundImageFileName + "." + BackgroundImageFileType, BackgroundImageURL), (output) => backgroundSprite = output);
        if (backgroundSprite != null)
            _backgroundImageSprite = backgroundSprite;
        
        Sprite cardBackSprite = null;
        yield return UnityExtensionMethods.RunOutputCoroutine<Sprite>(UnityExtensionMethods.CreateAndOutputSpriteFromImageFile(FilePathBase + "/" + CardBackImageFileName + "." + CardBackImageFileType, CardBackImageURL), (output) => cardBackSprite = output);
        if (cardBackSprite != null)
            _cardBackImageSprite = cardBackSprite;
        
        _isLoaded = true;
    }

    public void LoadJSONFromFile(string file, LoadJTokenDelegate load)
    {
        if (!File.Exists(file))
            return;
        
        JToken root = JToken.Parse(File.ReadAllText(file));
        IJEnumerable<JToken> jTokenEnumeration = root as JArray;
        if (jTokenEnumeration == null)
            jTokenEnumeration = (root as JObject).PropertyValues();
        foreach (JToken jToken in jTokenEnumeration)
            load(jToken, DefaultSet);
    }

    public void LoadSetFromJToken(JToken setJToken, string defaultSet)
    {
        if (setJToken == null)
            return;
        
        string setCode = setJToken.Value<string>(SetCodeIdentifier) ?? defaultSet;
        string setName = setJToken.Value<string>(SetNameIdentifier) ?? defaultSet;
        if (!string.IsNullOrEmpty(setCode) && !string.IsNullOrEmpty(setName))
            Sets.Add(new Set(setCode, setName));
        JArray cards = setJToken.Value<JArray>(SetCardsIdentifier);
        if (cards != null)
            foreach (JToken jToken in cards)
                LoadCardFromJToken(jToken, setCode);
    }

    public void LoadCardFromJToken(JToken cardJToken, string defaultSet)
    {
        if (cardJToken == null)
            return;

        string cardId = cardJToken.Value<string>(CardIdIdentifier) ?? string.Empty;
        string cardName = cardJToken.Value<string>(CardNameIdentifier) ?? string.Empty;
        string cardSet = cardJToken.Value<string>(CardSetIdentifier) ?? defaultSet;
        Dictionary<string, PropertySet> cardProps = new Dictionary<string, PropertySet>();
        foreach (PropertyDef prop in CardProperties) {
            cardProps [prop.Name] = new PropertySet() {
                Key = prop,
                Value = new PropertyDefValue() { Value = cardJToken.Value<string>(prop.Name) }
            };
        }
        if (!string.IsNullOrEmpty(cardId)) {
            Cards.Add(new Card(cardId, cardName, cardSet, cardProps));
            bool setExists = cardSet == defaultSet;
            for (int i = 0; !setExists && i < Sets.Count; i++)
                if (Sets [i].Code == cardSet)
                    setExists = true;
            if (!setExists)
                Sets.Add(new Set(cardSet, cardSet));
        }
    }

    public IEnumerable<Card> FilterCards(string id, string name, string setCode, Dictionary<string, string> properties)
    {
        if (id == null)
            id = string.Empty;
        if (name == null)
            name = string.Empty;
        if (setCode == null)
            setCode = string.Empty;
        if (properties == null)
            properties = new Dictionary<string, string>();

        foreach (Card card in Cards) {
            if (card.Id.ToLower().Contains(id.ToLower())
                && card.Name.ToLower().Contains(name.ToLower())
                && card.SetCode.ToLower().Contains(setCode.ToLower())) {
                bool propsMatch = true;
                foreach (KeyValuePair<string, string> entry in properties)
                    if (!(card.Properties [entry.Key].Value.Value).ToLower().Contains(entry.Value.ToLower()))
                        propsMatch = false;
                if (propsMatch)
                    yield return card;
            }
        }
    }

    public List<Set> Sets {
        get {
            if (_sets == null)
                _sets = new List<Set>();
            return _sets;
        }
    }

    public List<Card> Cards {
        get {
            if (_cards == null)
                _cards = new List<Card>();
            return _cards;
        }
    }

    public Sprite BackgroundImageSprite {
        get {
            if (_backgroundImageSprite == null)
                _backgroundImageSprite = Resources.Load<Sprite>(BackgroundImageFileName);
            return _backgroundImageSprite;
        }
    }

    public Sprite CardBackImageSprite {
        get {
            if (_cardBackImageSprite == null)
                _cardBackImageSprite = Resources.Load<Sprite>(CardBackImageFileName);
            return _cardBackImageSprite;
        }
    }

    public bool IsLoaded {
        get {
            return _isLoaded;
        }
    }

    public string Error {
        get {
            return _error;
        }
    }
}

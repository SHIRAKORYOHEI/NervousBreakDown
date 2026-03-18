using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    public Transform board;
    Sprite[] cardSprites;
    
    void Start()
    {
        cardSprites = Resources.LoadAll<Sprite>("Cards");
        
        List<int> ids = new List<int>();
        for (int i = 0; i < 40; i++) ids.Add(i);

        // Fisher-Yates shuffle
        for (int i = ids.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (ids[i], ids[r]) = (ids[r], ids[i]);
        }

        // Instantiate cards
        foreach (int id in ids)
        {
            Card card = Instantiate(cardPrefab, board, false).GetComponent<Card>();
            card.id = id;
            card.frontSprite = cardSprites[id];
        }
    }
}

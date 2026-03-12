using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    public Transform board;
    List<Card> cards = new List<Card>();
    List<int> cardsValue = new List<int>();
    
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j < 11; j++)
            {
                cardsValue.Add(j);
            }
        }
        Shuffle();
        CardInstantiate();
    }

    void Update()
    {
        
    }
    
    void CardInstantiate()
    {
        for (int i = 0; i < cardsValue.Count; i++)
        {
            Card card = Instantiate(cardPrefab, board).GetComponent<Card>();
            card.value = cardsValue[i];
            cards.Add(card);
        }
    }
    
    void Shuffle()
    {
        int n = cardsValue.Count;

        while (n > 1)
        {
            n--;
            
            int k = Random.Range(0, n + 1);
            (cardsValue[k], cardsValue[n]) = (cardsValue[n], cardsValue[k]);
        }
    }
}

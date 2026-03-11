using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    public Transform board;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    void CardInstantiate()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject card = Instantiate(cardPrefab, board);
                card.transform.SetAsLastSibling();
            }
        }
    }
}

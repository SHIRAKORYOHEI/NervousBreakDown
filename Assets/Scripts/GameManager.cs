using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }
    
    public void SelectCard(Card card)
    {
        Debug.Log(card.number);
        Debug.Log(card.suit);
    }
}

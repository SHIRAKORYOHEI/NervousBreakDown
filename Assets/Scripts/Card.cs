using UnityEngine;

public class Card : MonoBehaviour
{
    public int value;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnCardClicked()
    {
        GameManager.Instance.SelectCard(this);
    }
}

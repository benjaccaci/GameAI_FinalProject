using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public int startingMoney = 0;
    public TMP_Text moneyUI;
    private int currentMoney;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentMoney = startingMoney;
        
        if (moneyUI != null)
        {
            moneyUI.text = currentMoney.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMoney(int amount) {
        currentMoney += amount;  
        if (moneyUI != null) {
            moneyUI.text = currentMoney.ToString();
        }  
    }
}

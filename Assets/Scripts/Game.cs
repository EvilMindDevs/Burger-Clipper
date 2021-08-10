using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [Space(10)]

    // variables
    public double currentBurger = 0;
    public int soldBurger = 0;
    public int ingredients = 10000;
    public double money = 0;
    public int price = 0;
    public float demandRate = 0f;
    public float burgerInSeconds = 0f;
    public bool isAutoBuyerOn = false;

    [Space(10)]

    public int apperentice = 0;
    public int master = 0;
    public int machine = 0;
    public int apperenticeRate = 1;
    public int masterRate = 2;
    public int machineRate = 5;


    [Space(10)]

    // Generators
    public int apperenticeCost = 1000;
    public int masterCost = 2000;
    public int machineCost = 5000;

    [Space(10)]

    // metrics
    public int totalBurgerCount = 0;
    public int burgerCountInOneUpdate = 0;
    public float lastProduceRateTs = 0f;
    public int ingredientCost = 300;
    public int ingredientAmountForBurger = 100;
    public int buyIngredientAmount= 10000;
    public float ingredientsCostLastUpdateTime = 0f;
    public float generatorsLastUpdateTime = 0f;

    [Space(10)]

    // Texts
    public TMP_Text BurgerCountText;
    public TMP_Text BurgerInFridgeText;
    public TMP_Text IngredientsText;
    public Text IngredientsPriceText;
    public Text ApperenticeBuyButton;
    public Text MasterBuyButton;
    public Text MachineBuyButton;
    public TMP_Text PublicDemandText;
    public TMP_Text MoneyText;
    public TMP_Text PriceText;
    public TMP_Text BurgerInSecondsText;
    public TMP_Text ApperenticeText;
    public TMP_Text MasterText;
    public TMP_Text BurgerMachineText;
    public TMP_Text AutobuyerText;
    //public TMP_Text BusinnessText;
    //public TMP_Text ProductionText;

    // In-App Messaging
    public HMSAppMessagingManager messagingManager;

    /*
        1 burger minimum price is 1 dollar, demand is 100%
        1 burger maximum price is 50 dollar, demand is 0%
    */

    private void Start()
    {
        updateCounterText();
        InvokeRepeating("UpdateGameTime", 0, 0.1f);
        lastProduceRateTs = Time.time;
        ingredientsCostLastUpdateTime = Time.time;
        MasterText.GetComponentInChildren<Button>().onClick.AddListener(() => BuyGenerator(GeneratorType.Master));
        ApperenticeText.GetComponentInChildren<Button>().onClick.AddListener(() => BuyGenerator(GeneratorType.Apprentice));
        BurgerMachineText.GetComponentInChildren<Button>().onClick.AddListener(() => BuyGenerator(GeneratorType.Machine));
        generatorsLastUpdateTime = Time.time;
        AutobuyerText.gameObject.SetActive(false);
    }

    public enum GeneratorType { Master, Apprentice, Machine}

    public void BuyGenerator(GeneratorType type)
    {
        switch (type)
        {
            case GeneratorType.Apprentice:
                if (money < apperenticeCost) return;
                apperentice++;
                money -= apperenticeCost;
                apperenticeCost += apperenticeCost / 10;
                break;
            case GeneratorType.Master:
                if (money < masterCost) return;
                master++;
                money -= masterCost;
                masterCost += masterCost / 10;
                break;
            case GeneratorType.Machine:
                if (money < machineCost) return;
                machine++;
                money -= machineCost;
                machineCost += machineCost / 10;
                break;
        }
    }

    public void UpdateGameTime()
    {
        if (isAutoBuyerOn) AutoBuyerBuyIngredients();
        UpdateGeneratorsProduction();
        UpdateIngredientsCost();
        UpdateDemand();
        if(currentBurger > 0)
        {
            PurchaseBurger();
        }
        updateCounterText();
    }

    public void AutoBuyerBuyIngredients()
    {
        if (ingredients < apperentice * apperenticeRate * ingredientAmountForBurger ||
            ingredients < master * masterRate * ingredientAmountForBurger ||
            ingredients < machine * machineRate * ingredientAmountForBurger)
        {
            Buyingredients();
        }
    }

    public void UpdateGeneratorsProduction()
    {
        // apperentice can produce 1 burger in 3 seconds
        // master can produce 2 burger in 3 seconds
        // machine can produce 5 burger in 3 seconds
        if (Time.time - generatorsLastUpdateTime > 3f)
        {
            if (apperentice >= 1 && ingredients > ingredientAmountForBurger * apperentice * apperenticeRate)
            {
                makeBurger(apperenticeRate * apperentice);
            }
            if (master >= 1 && ingredients > ingredientAmountForBurger * master * masterRate)
            {
                makeBurger(masterRate * master);
            }
            if (machine >= 1 && ingredients > ingredientAmountForBurger * machine * machineRate)
            {
                makeBurger(machineRate * machine);
            }
            generatorsLastUpdateTime = Time.time;
        }
    }

    public void UpdateIngredientsCost()
    {
        if (Time.time - ingredientsCostLastUpdateTime > 10)
        {
            ingredientCost = Mathf.FloorToInt(Random.Range(200f, 500f));
            ingredientsCostLastUpdateTime = Time.time;
        }
    }

    public void UpdateDemand()
    {
        if (Time.time - lastProduceRateTs > 5f)
        {
            lastProduceRateTs = Time.time; // reset the timer
            burgerInSeconds = burgerCountInOneUpdate / 5f; // calculate the burgerinseconds
            burgerCountInOneUpdate = 0;
        }

        demandRate = 100f - (price / 50f) * 100f;
    }

    public void PurchaseBurger()
    {
        // this random is chance to someone to buy
        if (Random.Range(0f, 100f) < demandRate)
        {
            currentBurger -= 1;
            money += price;
        }
    }

    public void makeBurger(int rate = 1)
    {
        if (ingredients < ingredientAmountForBurger * rate)
        {
            return;
        }

        currentBurger += rate;
        totalBurgerCount += rate;
        if (totalBurgerCount > 1000)
        {
            AutobuyerText.gameObject.SetActive(true);
        }
        burgerCountInOneUpdate += rate;
        ingredients -= ingredientAmountForBurger * rate;
    }

    public void Buyingredients()
    {
        if(money < ingredientCost)
        {
            return;
        }

        ingredients += buyIngredientAmount;
        buyIngredientAmount += (buyIngredientAmount * 5 / 100);
        money -= ingredientCost;
        updateCounterText();
    }

    public void BuyAutoBuyer()
    {
        if (money <= 10000) return;

        AutobuyerText.GetComponentInChildren<Text>().text = "Sold";
        AutobuyerText.GetComponentInChildren<Button>().interactable = false;

        isAutoBuyerOn = true;
    }

    public void RaisePrice()
    {
        // if demand is 2, It cannot be lower than it
        if(demandRate < 2)
        {
            return;
        }
        price += 1;
        updateCounterText();

    }

    public void DropPrice()
    {
        if(price < 1)
        {
            return;
        }
        price -= 1;
        updateCounterText();

    }

    public void updateCounterText()
    {
        BurgerCountText.text = "Burger: " + totalBurgerCount.ToString();
        BurgerInFridgeText.text = "Burger In Fridge: " + currentBurger.ToString();
        IngredientsText.text = "Ingredients: " + ingredients + "gr";
        IngredientsPriceText.text = "$" + ingredientCost;
        PublicDemandText.text = "Public Demand: " + demandRate + "%";
        MoneyText.text = "Money: " + "$" + money;
        PriceText.text = "Price: $" + price;
        BurgerInSecondsText.text = "Burger / Seconds: " + burgerInSeconds;

        ApperenticeText.text = "Apperentice: " + apperentice;
        MasterText.text = "Master: " + master;
        BurgerMachineText.text = "Burger Machine: " + machine;

        ApperenticeBuyButton.text = "$" + apperenticeCost;
        MasterBuyButton.text = "$" + masterCost;
        MachineBuyButton.text = "$" + machineCost;
    }
}

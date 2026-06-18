using System.Text;
using TMPro;
using UnityEngine;

public class PerformanceScreen : MonoBehaviour
{
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private TextMeshProUGUI profitText;
    [SerializeField] private TextMeshProUGUI gradeText;

    public void ShowResults(Order order, PlateScorer plate, float finalProfit, string grade)
    {
        resultsPanel.SetActive(true);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("ORDER RESULTS");
        sb.AppendLine();

        // Ingredient validation
        foreach (OrderItem required in order.requiredItems)
        {
            int found = 0;

            foreach (FoodStats food in plate.foodsOnPlate)
            {
                if (food.foodType == required.type)
                    found++;
            }

            if (found == required.amount)
            {
                sb.AppendLine($"✓ {required.type} x{required.amount}");
            }
            else if (found < required.amount)
            {
                sb.AppendLine($"✗ {required.type} x{required.amount} (Missing)");
            }
            else
            {
                sb.AppendLine($"✗ {required.type} x{required.amount} (Extra)");
            }
        }

        sb.AppendLine();
        sb.AppendLine("COOKING QUALITY");
        sb.AppendLine();

        foreach (FoodStats food in plate.foodsOnPlate)
        {
            string quality;

            if (food.CookRatio < 0.9f)
                quality = "Undercooked";
            else if (food.CookRatio <= 1.2f)
                quality = "Perfect";
            else if (food.CookRatio <= 1.5f)
                quality = "Overcooked";
            else
                quality = "Burnt";

            sb.AppendLine($"{food.foodType} - {quality}");
        }

        resultsText.text = sb.ToString();
        profitText.text = $"Profit Earned: ${Mathf.RoundToInt(finalProfit)}";
        gradeText.text = $"Grade: {grade}";
    }
}

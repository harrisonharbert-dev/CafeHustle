using TMPro;
using UnityEngine;
using System.Text;

public class OrderInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderText;

    private void Update()
    {
        UpdateOrderDisplay();
    }

    private void UpdateOrderDisplay()
    {
        Order currentOrder = GameManager.Instance.currentOrder;

        if (currentOrder == null)
        {
            orderText.text = "No Active Order";
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("INGREDIENTS");
        sb.AppendLine();

        foreach (OrderItem item in currentOrder.requiredItems)
        {
            sb.AppendLine($"{item.type} x{item.amount}");
        }

        sb.AppendLine();

        orderText.text = sb.ToString();
    }
}
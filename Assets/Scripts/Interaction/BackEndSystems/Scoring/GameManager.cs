using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Order currentOrder;
    public PlateScorer plate;

    public PerformanceScreen endGameUI;

    private void Awake()
    {
        Instance = this;
    }

    public void ServeFood()
    {
        float score = plate.ScorePlate();

        string grade = CalculateGrade(score);

        endGameUI.ShowResults(currentOrder, plate, score, grade);
    }
    private string CalculateGrade(float score)
    {
        if (score >= 95f)
            return "S";
        else if (score >= 85f)
            return "A";
        else if (score >= 70f)
            return "B";
        else if (score >= 50f)
            return "C";
        else
            return "F";
    }
}
using UnityEngine;
using TMPro;

public class TrainingHUD : MonoBehaviour
{
    public MaxAgent maxAgent;

    public TMP_Text episodeText;
    public TMP_Text summaryText;
    public TMP_Text swingHitText;

    private void Update()
    {
        if (maxAgent == null)
            return;

        if (episodeText != null)
        {
            episodeText.text =
                "EPISODE " + maxAgent.EpisodeNumber;
        }

        if (summaryText != null)
        {
            string actionName = GetActionName(maxAgent.LastAction);

            summaryText.text =
                "MAX TRAINING\n\n" +

                "Episode: " + maxAgent.EpisodeNumber +

                "\nStep: " + maxAgent.StepCount +
                " / " + maxAgent.MaxStep +

                "\n\nReward: " +
                maxAgent.GetCumulativeReward().ToString("F4") +

                "\nDistance: " +maxAgent.CurrentDistance.ToString("F2") +

                "\n\nHits: " + maxAgent.HitCount +

                "\n\nPPO TRAINING" +

                "\nTime Scale: " +
                Time.timeScale.ToString("F1") + "x"+

                "\nAction: " + actionName;
        }

        if (swingHitText != null)
        {
            swingHitText.text =
                "SWING HIT: " + maxAgent.SwingHitCount;
        }
    }

    private string GetActionName(int action)
    {
        switch (action)
        {
            case 0: return "IDLE";
            case 1: return "WALK FORWARD";
            case 2: return "TURN LEFT";
            case 3: return "TURN RIGHT";
            case 4: return "SWING AXE";
            default: return "UNKNOWN";
        }
    }
}
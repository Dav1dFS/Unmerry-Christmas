using UnityEngine;

public class BackYardFixups : MonoBehaviour
{
    private void Awake()
    {
        SetupDrawingBook();
        FixCardboardBoxRewards();
    }

    private void SetupDrawingBook()
    {
        var drawingBook = GameObject.Find("DrawingBook");
        if (drawingBook == null)
        {
            Debug.LogWarning("[BackYardFixups] DrawingBook not found");
            return;
        }

        // Ensure tag and layer
        drawingBook.tag = "Collectable";
        drawingBook.layer = LayerMask.NameToLayer("Pickable");

        // Ensure collider
        var col = drawingBook.GetComponent<Collider>();
        if (col == null)
        {
            col = drawingBook.AddComponent<BoxCollider>();
        }
        col.isTrigger = false;

        // Ensure DrawingPageCollectable component
        if (drawingBook.GetComponent<DrawingPageCollectable>() == null)
        {
            drawingBook.AddComponent<DrawingPageCollectable>();
        }

    }

    private void FixCardboardBoxRewards()
    {
        var boxes = FindObjectsOfType<MovingBox>();
        int boxesWithReward = 0;

        foreach (var box in boxes)
        {
            // Check if this box is configured with a reward
            var rewardField = box.GetType().GetField("_rewardPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rewardField != null)
            {
                var reward = rewardField.GetValue(box) as GameObject;
                if (reward != null)
                {
                    boxesWithReward++;
                    if (boxesWithReward > 1)
                    {
                        // Only the last box should have the reward, disable others
                        rewardField.SetValue(box, null);
                    }
                }
            }
        }
    }
}

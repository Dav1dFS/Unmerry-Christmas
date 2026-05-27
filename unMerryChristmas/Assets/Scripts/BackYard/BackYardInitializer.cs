using UnityEngine;

public class BackYardInitializer : MonoBehaviour
{
    private void Awake()
    {
        SetupTokens();
    }

    private void SetupTokens()
    {
        var pushingAT = GameObject.Find("PushingAT");
        var throwingAT = GameObject.Find("ThrowingAT");
        var ziplineAT = GameObject.Find("ZiplineAT");

        if (pushingAT != null) SetupToken(pushingAT, BackYardTaskTracker.TaskId.GnomeParade, PlayerAbility.Pushing);
        if (throwingAT != null) SetupToken(throwingAT, BackYardTaskTracker.TaskId.BurstPipe, PlayerAbility.Throwing);
        if (ziplineAT != null) SetupToken(ziplineAT, BackYardTaskTracker.TaskId.TheHighWall, PlayerAbility.Ziplining);
    }

    private void SetupToken(GameObject token, BackYardTaskTracker.TaskId requiredTask, PlayerAbility ability)
    {
        // Ensure Rigidbody
        var rb = token.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = token.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
        }

        // Ensure Collider (non-trigger)
        var col = token.GetComponent<Collider>();
        if (col == null)
        {
            col = token.AddComponent<SphereCollider>();
        }
        col.isTrigger = false;

        // Ensure on Pickable layer
        int pickableLayer = LayerMask.NameToLayer("Pickable");
        if (pickableLayer >= 0)
        {
            token.layer = pickableLayer;
        }

        // Ensure PickupObject
        if (token.GetComponent<PickupObject>() == null)
        {
            token.AddComponent<PickupObject>();
        }

        // Ensure TokenEffect for glow
        if (token.GetComponent<TokenEffect>() == null)
        {
            token.AddComponent<TokenEffect>();
        }

        // Ensure AbilityToken with correct ability
        var abilityToken = token.GetComponent<AbilityToken>();
        if (abilityToken == null)
        {
            abilityToken = token.AddComponent<AbilityToken>();
        }
        SetAbilityOnToken(abilityToken, ability);

        // Setup TaskReward
        var taskReward = token.GetComponent<TaskReward>();
        if (taskReward == null)
        {
            taskReward = token.AddComponent<TaskReward>();
        }
        taskReward.SetRequiredTask(requiredTask);

        Debug.Log($"[BackYardInitializer] Configured {token.name} with task {requiredTask} and ability {ability}");
    }

    private void SetAbilityOnToken(AbilityToken token, PlayerAbility ability)
    {
        var field = typeof(AbilityToken).GetField("_ability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(token, ability);
        }
    }
}

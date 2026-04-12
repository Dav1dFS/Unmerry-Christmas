using UnityEngine;

public class FatherNpcController : NpcController
{
    protected override void OnEnterBusy(Transform keyItem)
    {
        Debug.Log($"{name}: The Father is using object [{keyItem.name}]");
    }

    protected override void OnEnterWalking(Transform keyItem)
    {
        Debug.Log($"{name}: The Father is walking to object [{keyItem.name}]");
    }

    protected override void OnEnterAlerted()
    {
        Debug.Log($"{name}: The Father is Alerted");
    }

    protected override void OnEnterWatching()
    {
        Debug.Log($"{name}: The Father Saw You");
    }

    protected override void OnEnterDisabled()
    {
        Debug.Log($"{name}: The Father is Disabled");
    }
}
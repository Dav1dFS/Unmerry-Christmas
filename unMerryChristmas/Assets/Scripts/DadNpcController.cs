using UnityEngine;

public class DadNpcController : NpcController
{
    protected override void OnEnterBusy(Transform keyItem)
    {
        Debug.Log($"{name}: Using object [{keyItem.name}]");
    }

    protected override void OnEnterWalking(Transform keyItem)
    {
        Debug.Log($"{name}: Walking to object [{keyItem.name}]");
    }

    protected override void OnEnterAlerted()
    {
        Debug.Log($"{name}: Dad Alerted");
    }

    protected override void OnEnterWatching()
    {
        Debug.Log($"{name}: Dad Saw You");
    }

    protected override void OnEnterDisabled()
    {
        Debug.Log($"{name}: Dad Disabled");
    }
}

using UnityEngine;

public class FatherNpcController : NpcController
{
    protected override void OnEnterBusy(Transform keyItem) { }
    protected override void OnEnterWalking(Transform keyItem) { }
    protected override void OnEnterAlerted() { }
    protected override void OnEnterWatching() { }
    protected override void OnEnterDisabled() { }
}
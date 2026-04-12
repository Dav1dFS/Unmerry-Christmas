using UnityEngine;

public class ThrowableImpact : MonoBehaviour
{
    private bool _thrown = false;

    public void SetThrown() => _thrown = true;

    private void OnCollisionEnter(Collision col)
    {
        Debug.Log($"Impacto com: {col.gameObject.name} | thrown={_thrown}");
        if (!_thrown) return;

        // GetComponentInParent garante que apanha o NpcController mesmo se acertar num filho
        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
        {
            npc.OnHit();
            _thrown = false;
            return;
        }

        // Se acertou em qualquer outra coisa que não seja o player, desativa o flag
        if (!col.gameObject.CompareTag("Player"))
            _thrown = false;
    }


}
using UnityEngine;

public class AbilityToken : MonoBehaviour
{
    [SerializeField] private PlayerAbility _ability;
    public PlayerAbility Ability => _ability;
}

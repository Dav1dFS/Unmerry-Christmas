using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AbilityEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    private static readonly Dictionary<PlayerAbility, string> Names = new()
    {
        { PlayerAbility.Moving,            "Moving"             },
        { PlayerAbility.Running,           "Running"            },
        { PlayerAbility.Jumping,           "Jumping"            },
        { PlayerAbility.Grabbing,          "Grabbing"           },
        { PlayerAbility.Interacting,       "Interacting"        },
        { PlayerAbility.Releasing,         "Releasing"          },
        { PlayerAbility.Pushing,           "Pushing"            },
        { PlayerAbility.Rolling,           "Rolling"            },
        { PlayerAbility.Throwing,          "Throwing"           },
        { PlayerAbility.ChangingCostumes,  "Changing Costumes"  },
        { PlayerAbility.ThrowingCandy,     "Throwing Candy"     },
        { PlayerAbility.Ziplining,         "Ziplining"          },
        { PlayerAbility.AnimatingElves,    "Animating Elves"    },
        { PlayerAbility.Hiding,            "Hiding in Boxes"    },
        { PlayerAbility.ExplosingPresents, "Explosive Presents" },
        { PlayerAbility.SequencedDance,    "Sequenced Dance"    },
    };

    public void Setup(PlayerAbility ability, bool faded)
    {
        if (Names.TryGetValue(ability, out string name))
            _label.text = name;

        GetComponent<CanvasGroup>().alpha = faded ? 0.4f : 1f;
    }
}

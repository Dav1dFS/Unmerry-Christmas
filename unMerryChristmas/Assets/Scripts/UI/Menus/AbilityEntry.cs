using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single Tutorial-page entry for one ability (Level Design Document §3.3).
///
/// Each entry can show:
///   • the ability name (_label, required),
///   • a hand-drawn illustration (_illustration, optional — assign per-ability
///     sprites in the inspector via _illustrations),
///   • the input prompt (_input, optional),
///   • a one-line caption in the elf's voice (_caption, optional).
///
/// Only _label is mandatory; the optional fields are populated only when wired,
/// so the existing text-only prefab keeps working until art is added.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AbilityEntry : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private TMP_Text _label;

    [Header("Optional content (Level Design Document §3.3)")]
    [SerializeField] private TMP_Text _caption;
    [SerializeField] private TMP_Text _input;
    [SerializeField] private Image    _illustration;

    [Tooltip("Optional per-ability illustration sprites. Leave empty until art exists.")]
    [SerializeField] private List<AbilityIllustration> _illustrations = new();

    [System.Serializable]
    public struct AbilityIllustration
    {
        public PlayerAbility ability;
        public Sprite sprite;
    }

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

    // Keyboard input prompts, per Level Design Document §3.3 entry table.
    private static readonly Dictionary<PlayerAbility, string> Inputs = new()
    {
        { PlayerAbility.Moving,            "WASD"                  },
        { PlayerAbility.Running,           "Shift"                 },
        { PlayerAbility.Jumping,           "Space"                 },
        { PlayerAbility.Grabbing,          "E"                     },
        { PlayerAbility.Interacting,       "F"                     },
        { PlayerAbility.Releasing,         "E"                     },
        { PlayerAbility.Pushing,           "Hold E near object"    },
        { PlayerAbility.Rolling,           "R"                     },
        { PlayerAbility.Throwing,          "Q while holding"       },
        { PlayerAbility.ChangingCostumes,  "F near wardrobe"       },
        { PlayerAbility.ThrowingCandy,     "Q"                     },
        { PlayerAbility.Ziplining,         "F near anchor"         },
        { PlayerAbility.AnimatingElves,    "F near elf"            },
        { PlayerAbility.Hiding,            "F near container"      },
        { PlayerAbility.ExplosingPresents, "Hold T, then release"  },
        { PlayerAbility.SequencedDance,    "Arrow Keys (sequence)" },
    };

    // One-line captions in the elf's voice, per Level Design Document §3.3.
    private static readonly Dictionary<PlayerAbility, string> Captions = new()
    {
        { PlayerAbility.Moving,            "Feet first. Always."             },
        { PlayerAbility.Running,           "Faster. They are not looking."   },
        { PlayerAbility.Jumping,           "Up and over. Easy."              },
        { PlayerAbility.Grabbing,          "Hands are the best tools."       },
        { PlayerAbility.Interacting,       "A little nudge goes a long way." },
        { PlayerAbility.Releasing,         "Let go. Timing helps."           },
        { PlayerAbility.Pushing,           "Lean in. It will move eventually." },
        { PlayerAbility.Rolling,           "Tuck in tight and go fast."      },
        { PlayerAbility.Throwing,          "Aim later. Throw now."           },
        { PlayerAbility.ChangingCostumes,  "They never look twice at the help." },
        { PlayerAbility.ThrowingCandy,     "A distraction with a pleasant smell." },
        { PlayerAbility.Ziplining,         "The candy holds. Probably."      },
        { PlayerAbility.AnimatingElves,    "Wake up. We have work to do."    },
        { PlayerAbility.Hiding,            "If they open it, act festive."   },
        { PlayerAbility.ExplosingPresents, "Wrap it tight. Stand back further." },
        { PlayerAbility.SequencedDance,    "Every step counts. Mostly the last one." },
    };

    public void Setup(PlayerAbility ability, bool faded)
    {
        if (_label != null && Names.TryGetValue(ability, out string name))
            _label.text = name;

        if (_input != null && Inputs.TryGetValue(ability, out string input))
            _input.text = input;

        if (_caption != null && Captions.TryGetValue(ability, out string caption))
            _caption.text = caption;

        if (_illustration != null)
        {
            Sprite sprite = GetIllustration(ability);
            _illustration.sprite = sprite;
            _illustration.enabled = sprite != null;
        }

        GetComponent<CanvasGroup>().alpha = faded ? 0.4f : 1f;
    }

    private Sprite GetIllustration(PlayerAbility ability)
    {
        foreach (var entry in _illustrations)
            if (entry.ability == ability) return entry.sprite;
        return null;
    }
}

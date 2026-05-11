using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class AbilityEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text    _nameLabel;
    [SerializeField] private TMP_Text    _captionLabel;
    [SerializeField] private TMP_Text    _inputLabel;
    [SerializeField] private CanvasGroup _group;

    private static readonly Dictionary<PlayerAbility, (string name, string caption, string kb, string ctrl)> Data = new()
    {
        { PlayerAbility.Moving,           ("Moving",            "Feet first. Always.",                      "WASD",               "Left Stick"            ) },
        { PlayerAbility.Running,          ("Running",           "Faster. They are not looking.",            "Shift",              "L2"                    ) },
        { PlayerAbility.Jumping,          ("Jumping",           "Up and over. Easy.",                       "Space",              "Cross"                 ) },
        { PlayerAbility.Grabbing,         ("Grabbing",          "Hands are the best tools.",                "E",                  "Square"                ) },
        { PlayerAbility.Interacting,      ("Interacting",       "A little nudge goes a long way.",          "F",                  "Triangle"              ) },
        { PlayerAbility.Releasing,        ("Releasing",         "Let go. Timing helps.",                    "E",                  "Release Square"        ) },
        { PlayerAbility.Pushing,          ("Pushing",           "Lean in. It will move eventually.",        "Hold E",             "Hold Square"           ) },
        { PlayerAbility.Rolling,          ("Rolling",           "Tuck in tight and go fast.",               "R",                  "R2"                    ) },
        { PlayerAbility.Throwing,         ("Throwing",          "Aim later. Throw now.",                    "Q",                  "Circle"                ) },
        { PlayerAbility.ChangingCostumes, ("Changing Costumes", "They never look twice at the help.",       "F near wardrobe",    "Triangle near wardrobe") },
        { PlayerAbility.ThrowingCandy,    ("Throwing Candy",    "A distraction with a pleasant smell.",     "Q",                  "Circle"                ) },
        { PlayerAbility.Ziplining,        ("Ziplining",         "The candy holds. Probably.",               "F near anchor",      "Triangle near anchor"  ) },
        { PlayerAbility.AnimatingElves,   ("Animating Elves",   "Wake up. We have work to do.",            "F near elf",         "Triangle near elf"     ) },
        { PlayerAbility.Hiding,           ("Hiding in Boxes",   "If they open it, act festive.",           "F near container",   "Triangle near container") },
        { PlayerAbility.ExplosingPresents,("Explosive Presents","Wrap it tight. Stand back further.",      "Hold T, release",    "Hold R1, release"      ) },
        { PlayerAbility.SequencedDance,   ("Sequenced Dance",   "Every step counts. Mostly the last one.", "Arrow Keys",         "D-Pad"                 ) },
    };

    public void Setup(PlayerAbility ability, bool faded)
    {
        if (!Data.TryGetValue(ability, out var d)) return;

        _nameLabel.text    = d.name;
        _captionLabel.text = d.caption;
        _inputLabel.text   = $"{d.kb}  /  {d.ctrl}";

        if (_group != null)
            _group.alpha = faded ? 0.45f : 1f;
    }
}

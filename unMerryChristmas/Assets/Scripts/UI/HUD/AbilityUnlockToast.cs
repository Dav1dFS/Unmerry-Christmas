using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AbilityUnlockToast : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float    _displayDuration = 4f;
    [SerializeField] private float    _fadeDuration    = 0.45f;

    private static readonly Dictionary<PlayerAbility, string> AbilityNames = new()
    {
        { PlayerAbility.Pushing,          "Pushing"           },
        { PlayerAbility.Rolling,          "Rolling"           },
        { PlayerAbility.Throwing,         "Throwing"          },
        { PlayerAbility.ChangingCostumes, "Changing Costumes" },
        { PlayerAbility.ThrowingCandy,    "Throwing Candy"    },
        { PlayerAbility.Ziplining,        "Ziplining"         },
        { PlayerAbility.AnimatingElves,   "Animating Elves"   },
        { PlayerAbility.Hiding,           "Hiding in Boxes"   },
        { PlayerAbility.ExplosingPresents,"Explosive Presents"},
        { PlayerAbility.SequencedDance,   "Sequenced Dance"   },
    };

    private CanvasGroup _group;
    private Coroutine   _current;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show(PlayerAbility ability)
    {
        if (!AbilityNames.TryGetValue(ability, out string name)) return;
        if (_current != null) StopCoroutine(_current);
        _label.text = $"New note added\n<size=70%><i>{name}</i></size>";
        _current = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        gameObject.SetActive(true);
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(_displayDuration);
        yield return Fade(1f, 0f);
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < _fadeDuration)
        {
            _group.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        _group.alpha = to;
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Open via: Tools → BackYard Setup Window
/// Drag your scene objects into the labelled slots, then click Apply for each section.
/// </summary>
public class BackYardSetupWindow : EditorWindow
{
    // ── Scroll ────────────────────────────────────────────────────────────────
    private Vector2 _scroll;

    // ── Task 1 ────────────────────────────────────────────────────────────────
    private GameObject _windowTriggerGO;

    // ── Task 2 ────────────────────────────────────────────────────────────────
    private GameObject _patioTable;
    private GameObject _chairA;
    private GameObject _chairB;

    // ── Task 3 ────────────────────────────────────────────────────────────────
    private GameObject _gnome1, _gnome2, _gnome3;
    private GameObject _zone1,  _zone2,  _zone3;
    private GameObject _compostBinGO;
    private GameObject _compostLid;
    private GameObject _by1Page;

    // ── Task 4 ────────────────────────────────────────────────────────────────
    private GameObject _box1, _box2, _box3;
    private GameObject _box1Closed, _box1Open, _box1Snow;
    private GameObject _box2Closed, _box2Open, _box2Snow;
    private GameObject _box3Closed, _box3Open, _box3Snow;

    // ── Task 5 ────────────────────────────────────────────────────────────────
    private GameObject _lightsStrand;
    private GameObject _lightsLooseEnd;
    private GameObject _tangledBall;

    // ── Task 6 ────────────────────────────────────────────────────────────────
    private GameObject _pipeGO;
    private GameObject _pipeIntact;
    private GameObject _pipeBurst;
    private GameObject _icePatchVisual;

    // ── Task 8 ────────────────────────────────────────────────────────────────
    private GameObject _birdbathGO;
    private GameObject _birdbathIntact;
    private GameObject _birdbathShattered;
    private GameObject _birdbathRubble;

    // ── Task 9 ────────────────────────────────────────────────────────────────
    private GameObject _shedDoorGO;
    private GameObject _padlockGO;
    private GameObject _padlockIntact;
    private GameObject _padlockBroken;
    private GameObject _shelf1, _shelf2, _shelf3;

    // ── Foldouts ─────────────────────────────────────────────────────────────
    private bool _f1 = true, _f2 = true, _f3 = true, _f4 = true,
                 _f5 = true, _f6 = true, _f8 = true, _f9 = true;

    [MenuItem("Tools/BackYard Setup Window")]
    public static void Open() =>
        GetWindow<BackYardSetupWindow>("BackYard Setup").minSize = new Vector2(400, 600);

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Drag scene GameObjects into the slots below, then click Apply for each section. " +
            "Already-configured components are skipped automatically.",
            MessageType.Info);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        // ── Task 1 ────────────────────────────────────────────────────────────
        _f1 = EditorGUILayout.Foldout(_f1, "Task 1 — Window Climb Trigger", true, EditorStyles.foldoutHeader);
        if (_f1)
        {
            EditorGUI.indentLevel++;
            _windowTriggerGO = GOField("Trigger GameObject (at window)", _windowTriggerGO);
            if (GUILayout.Button("Apply Task 1")) ApplyTask1();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 2 ────────────────────────────────────────────────────────────
        _f2 = EditorGUILayout.Foldout(_f2, "Task 2 — Garden Chaos", true, EditorStyles.foldoutHeader);
        if (_f2)
        {
            EditorGUI.indentLevel++;
            _patioTable = GOField("Patio Table",  _patioTable);
            _chairA     = GOField("Chair A",      _chairA);
            _chairB     = GOField("Chair B",      _chairB);
            if (GUILayout.Button("Apply Task 2")) ApplyTask2();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 3 ────────────────────────────────────────────────────────────
        _f3 = EditorGUILayout.Foldout(_f3, "Task 3 — Gnome Parade + Compost Bin (BY-1)", true, EditorStyles.foldoutHeader);
        if (_f3)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Gnomes (tag them as 'Gnome')");
            _gnome1 = GOField("Gnome 1", _gnome1);
            _gnome2 = GOField("Gnome 2", _gnome2);
            _gnome3 = GOField("Gnome 3", _gnome3);
            Space();
            EditorGUILayout.LabelField("Placement marker zones (by the back door)");
            _zone1 = GOField("Zone 1", _zone1);
            _zone2 = GOField("Zone 2", _zone2);
            _zone3 = GOField("Zone 3", _zone3);
            Space();
            EditorGUILayout.LabelField("Compost bin");
            _compostBinGO = GOField("Compost Bin GameObject", _compostBinGO);
            _compostLid   = GOField("Lid child Transform",    _compostLid);
            _by1Page      = GOField("BY-1 drawing page (child of bin)", _by1Page);
            if (GUILayout.Button("Apply Task 3")) ApplyTask3();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 4 ────────────────────────────────────────────────────────────
        _f4 = EditorGUILayout.Foldout(_f4, "Task 4 — Unearth the Past (3 boxes)", true, EditorStyles.foldoutHeader);
        if (_f4)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Box 1");
            _box1       = GOField("Root",          _box1);
            _box1Closed = GOField("Closed visual", _box1Closed);
            _box1Open   = GOField("Open visual",   _box1Open);
            _box1Snow   = GOField("Snow cover (optional)", _box1Snow);
            Space();
            EditorGUILayout.LabelField("Box 2");
            _box2       = GOField("Root",          _box2);
            _box2Closed = GOField("Closed visual", _box2Closed);
            _box2Open   = GOField("Open visual",   _box2Open);
            _box2Snow   = GOField("Snow cover (optional)", _box2Snow);
            Space();
            EditorGUILayout.LabelField("Box 3");
            _box3       = GOField("Root",          _box3);
            _box3Closed = GOField("Closed visual", _box3Closed);
            _box3Open   = GOField("Open visual",   _box3Open);
            _box3Snow   = GOField("Snow cover (optional)", _box3Snow);
            if (GUILayout.Button("Apply Task 4")) ApplyTask4();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 5 ────────────────────────────────────────────────────────────
        _f5 = EditorGUILayout.Foldout(_f5, "Task 5 — Lights Out", true, EditorStyles.foldoutHeader);
        if (_f5)
        {
            EditorGUI.indentLevel++;
            _lightsStrand   = GOField("Lights strand root",       _lightsStrand);
            _lightsLooseEnd = GOField("Loose end child (tip)",    _lightsLooseEnd);
            _tangledBall    = GOField("Tangled ball mesh (child)", _tangledBall);
            if (GUILayout.Button("Apply Task 5")) ApplyTask5();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 6 ────────────────────────────────────────────────────────────
        _f6 = EditorGUILayout.Foldout(_f6, "Task 6 — Burst Pipe", true, EditorStyles.foldoutHeader);
        if (_f6)
        {
            EditorGUI.indentLevel++;
            _pipeGO       = GOField("Pipe root GameObject",    _pipeGO);
            _pipeIntact   = GOField("Intact visual child",     _pipeIntact);
            _pipeBurst    = GOField("Burst visual child",      _pipeBurst);
            _icePatchVisual = GOField("Ice visual child (on IcePatch GO)", _icePatchVisual);
            if (GUILayout.Button("Apply Task 6")) ApplyTask6();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 8 ────────────────────────────────────────────────────────────
        _f8 = EditorGUILayout.Foldout(_f8, "Task 8 — Birdbath Demolition", true, EditorStyles.foldoutHeader);
        if (_f8)
        {
            EditorGUI.indentLevel++;
            _birdbathGO        = GOField("Birdbath root",            _birdbathGO);
            _birdbathIntact    = GOField("Intact visual child",      _birdbathIntact);
            _birdbathShattered = GOField("Shattered basin child",    _birdbathShattered);
            _birdbathRubble    = GOField("Full destroyed child",     _birdbathRubble);
            if (GUILayout.Button("Apply Task 8")) ApplyTask8();
            EditorGUI.indentLevel--;
        }
        Space();

        // ── Task 9 ────────────────────────────────────────────────────────────
        _f9 = EditorGUILayout.Foldout(_f9, "Task 9 — Shed Heist", true, EditorStyles.foldoutHeader);
        if (_f9)
        {
            EditorGUI.indentLevel++;
            _shedDoorGO   = GOField("Shed door (swinging part)", _shedDoorGO);
            _padlockGO    = GOField("Padlock mesh",              _padlockGO);
            _padlockIntact = GOField("Padlock intact visual",    _padlockIntact);
            _padlockBroken = GOField("Padlock broken visual",    _padlockBroken);
            Space();
            _shelf1 = GOField("Shelf 1", _shelf1);
            _shelf2 = GOField("Shelf 2", _shelf2);
            _shelf3 = GOField("Shelf 3", _shelf3);
            EditorGUILayout.HelpBox(
                "After applying, open each ShedShelf in the Inspector and fill _contents and _contentBodies with the items on that shelf.",
                MessageType.Warning);
            if (GUILayout.Button("Apply Task 9")) ApplyTask9();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndScrollView();
    }

    // ── Apply methods ─────────────────────────────────────────────────────────

    void ApplyTask1()
    {
        if (!Require(_windowTriggerGO, "Window Trigger GO")) return;

        EnsureCollider<BoxCollider>(_windowTriggerGO).isTrigger = true;
        var wc = EnsureComponent<WindowClimb>(_windowTriggerGO);
        SetString(wc, "_targetScene",  "KitchenScenario");
        SetString(wc, "_entryPointId", "from_backyard_window");

        Done("Task 1: WindowClimb configured on " + _windowTriggerGO.name);
    }

    void ApplyTask2()
    {
        if (!Require(_patioTable, "Patio Table") ||
            !Require(_chairA, "Chair A") ||
            !Require(_chairB, "Chair B")) return;

        // Table
        var rb = EnsureComponent<Rigidbody>(_patioTable);
        rb.mass        = 20f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;
        var push = EnsureComponent<PushableObject>(_patioTable);
        SetFloat(push, "maxPushDistance", 1.5f);
        SetFloat(push, "_pushForce", 800f);
        var tip = EnsureComponent<TippablePatioTable>(_patioTable);
        SetFloat(tip, "_tipTorque", 120f);
        SetLayer(_patioTable, "Pushable");

        // Wire tracker
        var tracker = FindComponent<GardenChaosTracker>();
        if (tracker != null)
        {
            SetRef(tip, "_gardenChaosTracker", tracker);
            Debug.Log("[BackYardSetup] TippablePatioTable._gardenChaosTracker wired.");
        }

        // Chair A
        SetupChair(_chairA);

        // Chair B
        SetupChair(_chairB);

        // Wire chairs to tracker
        if (tracker != null)
        {
            SetRef(tracker, "_chairA", _chairA.GetComponent<TopplableChair>());
            SetRef(tracker, "_chairB", _chairB.GetComponent<TopplableChair>());
            Debug.Log("[BackYardSetup] GardenChaosTracker chairs wired.");
        }

        Done("Task 2: Garden Chaos components applied.");
    }

    void SetupChair(GameObject chair)
    {
        var rb = EnsureComponent<Rigidbody>(chair);
        rb.mass = 5f;
        rb.constraints = RigidbodyConstraints.None;
        EnsureComponent<PickupObject>(chair);
        EnsureComponent<TopplableChair>(chair);
        SetLayer(chair, "Pickup");
    }

    void ApplyTask3()
    {
        // Gnomes
        foreach (var gnome in new[] { _gnome1, _gnome2, _gnome3 })
        {
            if (gnome == null) continue;
            SetTag(gnome, "Gnome");
            var rb = EnsureComponent<Rigidbody>(gnome);
            rb.mass = 2f;
            rb.isKinematic = false;
            EnsureComponent<PickupObject>(gnome);
            SetLayer(gnome, "Pickup");
        }

        // Placement zones
        foreach (var zone in new[] { _zone1, _zone2, _zone3 })
        {
            if (zone == null) continue;
            var col = EnsureCollider<BoxCollider>(zone);
            col.isTrigger = true;
            col.size = new Vector3(0.8f, 0.5f, 0.8f);
            EnsureComponent<GnomePlacementZone>(zone);
        }

        // Wire tracker
        var tracker = FindComponent<GnomeParadeTracker>();
        if (tracker != null)
        {
            var so = new SerializedObject(tracker);
            var arr = so.FindProperty("_zones");
            int count = 0;
            var zones = new[] { _zone1, _zone2, _zone3 };
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                if (zones[i] != null)
                {
                    var zone = zones[i].GetComponent<GnomePlacementZone>();
                    arr.GetArrayElementAtIndex(i).objectReferenceValue = zone;
                    count++;
                }
            }
            so.ApplyModifiedProperties();
            Debug.Log($"[BackYardSetup] GnomeParadeTracker wired {count}/3 zones.");
        }

        // Compost bin
        if (_compostBinGO != null)
        {
            var bin = EnsureComponent<CompostBin>(_compostBinGO);
            if (_compostLid != null) SetRef(bin, "_lidTransform", _compostLid.transform);
            SetFloat(bin, "_openAngle", 90f);
            SetLayer(_compostBinGO, "Pickup");
            EnsureComponent<Rigidbody>(_compostBinGO).isKinematic = true;

            if (_by1Page != null)
            {
                SetTag(_by1Page, "Collectable");
                SetLayer(_by1Page, "Pickup");
                var col = EnsureCollider<SphereCollider>(_by1Page);
                col.isTrigger = true;
                col.radius = 0.25f;
                EnsureComponent<DrawingPageCollectable>(_by1Page);
                _by1Page.SetActive(false);

                var pageComp = _by1Page.GetComponent<DrawingPageCollectable>();
                SetRef(bin, "_drawingPage", pageComp);
                Debug.Log("[BackYardSetup] CompostBin BY-1 page wired.");
            }
        }

        Done("Task 3: Gnome Parade + Compost Bin applied.");
    }

    void ApplyTask4()
    {
        ApplyBox(_box1, _box1Closed, _box1Open, _box1Snow);
        ApplyBox(_box2, _box2Closed, _box2Open, _box2Snow);
        ApplyBox(_box3, _box3Closed, _box3Open, _box3Snow);

        // Wire tracker
        var tracker = FindComponent<BoxStackTracker>();
        if (tracker != null)
        {
            var so  = new SerializedObject(tracker);
            var arr = so.FindProperty("_boxes");
            arr.arraySize = 3;
            var boxes = new[] { _box1, _box2, _box3 };
            for (int i = 0; i < 3; i++)
            {
                if (boxes[i] != null)
                    arr.GetArrayElementAtIndex(i).objectReferenceValue =
                        boxes[i].GetComponent<MovingBox>();
            }
            so.ApplyModifiedProperties();
            Debug.Log("[BackYardSetup] BoxStackTracker wired.");
        }

        Done("Task 4: Moving boxes applied.");
    }

    void ApplyBox(GameObject root, GameObject closed, GameObject open, GameObject snow)
    {
        if (root == null) return;
        var box = EnsureComponent<MovingBox>(root);
        if (closed != null) SetRef(box, "_closedVisual", closed);
        if (open   != null) { SetRef(box, "_openVisual", open); open.SetActive(false); }
        if (snow   != null) SetRef(box, "_snowCover", snow);
        SetLayer(root, "Pickup");
        if (!root.TryGetComponent<Collider>(out _))
            root.AddComponent<BoxCollider>();
    }

    void ApplyTask5()
    {
        if (!Require(_lightsStrand, "Lights Strand")) return;

        var lights = EnsureComponent<LightsPullable>(_lightsStrand);

        // Rigidbody on root
        var rb = EnsureComponent<Rigidbody>(_lightsStrand);
        rb.isKinematic = true;

        if (_lightsLooseEnd != null)
        {
            var looseRb = EnsureComponent<Rigidbody>(_lightsLooseEnd);
            looseRb.isKinematic = true;
            if (!_lightsLooseEnd.TryGetComponent<Collider>(out _))
            {
                var c = _lightsLooseEnd.AddComponent<BoxCollider>();
                c.size = new Vector3(0.1f, 0.1f, 0.1f);
            }
            var pickup = EnsureComponent<PickupObject>(_lightsLooseEnd);
            SetLayer(_lightsLooseEnd, "Pickup");
            SetRef(lights, "_looseEnd", pickup);
        }

        SetRef(lights, "_lightsRigidbody", rb);
        SetFloat(lights, "_pullSpeedThreshold", 4f);
        SetFloat(lights, "_tearThreshold", 1.5f);

        if (_tangledBall != null)
        {
            _tangledBall.SetActive(false);
            SetRef(lights, "_tangledVisual", _tangledBall);
        }

        Done("Task 5: LightsPullable applied.");
    }

    void ApplyTask6()
    {
        if (!Require(_pipeGO, "Pipe GO")) return;

        var pipe = EnsureComponent<BreakablePipe>(_pipeGO);
        if (_pipeIntact != null) SetRef(pipe, "_intactVisual", _pipeIntact);
        if (_pipeBurst  != null) { SetRef(pipe, "_burstVisual", _pipeBurst); _pipeBurst.SetActive(false); }

        var icePatch = FindComponent<IcePatch>();
        if (icePatch != null)
        {
            SetRef(pipe, "_icePatch", icePatch);
            Debug.Log("[BackYardSetup] BreakablePipe._icePatch auto-wired.");
        }

        if (_icePatchVisual != null)
        {
            _icePatchVisual.SetActive(false);
            SetRef(icePatch, "_iceVisual", _icePatchVisual);

            var matPath = "Assets/Materials/IceSurface.physicsMaterial";
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(matPath);
            if (mat != null) SetRef(icePatch, "_iceMaterial", mat);
        }

        if (!_pipeGO.TryGetComponent<Collider>(out _))
            _pipeGO.AddComponent<MeshCollider>();

        Done("Task 6: Burst Pipe applied.");
    }

    void ApplyTask8()
    {
        if (!Require(_birdbathGO, "Birdbath GO")) return;

        var bath = EnsureComponent<BreakableBirdbath>(_birdbathGO);
        if (_birdbathIntact    != null) SetRef(bath, "_intactVisual",         _birdbathIntact);
        if (_birdbathShattered != null) { SetRef(bath, "_shatteredBasinVisual", _birdbathShattered); _birdbathShattered.SetActive(false); }
        if (_birdbathRubble    != null) { SetRef(bath, "_fullDestroyedVisual",  _birdbathRubble);    _birdbathRubble.SetActive(false); }

        if (!_birdbathGO.TryGetComponent<Collider>(out _))
            _birdbathGO.AddComponent<MeshCollider>();

        Done("Task 8: Birdbath Demolition applied.");
    }

    void ApplyTask9()
    {
        // Shed door
        if (_shedDoorGO != null)
        {
            var door = EnsureComponent<ShedDoor>(_shedDoorGO);
            SetFloat(door, "_openAngle", 110f);
            SetFloat(door, "_openSpeed", 3f);

            // Create DoorPivot if missing
            var pivot = _shedDoorGO.transform.Find("DoorPivot");
            if (pivot == null)
            {
                var pivotGO = new GameObject("DoorPivot");
                Undo.RegisterCreatedObjectUndo(pivotGO, "Create DoorPivot");
                pivotGO.transform.SetParent(_shedDoorGO.transform, false);
                pivot = pivotGO.transform;
                Debug.Log("[BackYardSetup] Created DoorPivot — move it to the hinge edge of the door.");
            }
            SetRef(door, "_doorPivot", pivot);
            SetLayer(_shedDoorGO, "Pickup");
            if (!_shedDoorGO.TryGetComponent<Collider>(out _))
                _shedDoorGO.AddComponent<BoxCollider>();
        }

        // Padlock
        if (_padlockGO != null)
        {
            var padlock = EnsureComponent<Padlock>(_padlockGO);
            if (_padlockIntact != null) SetRef(padlock, "_intactVisual", _padlockIntact);
            if (_padlockBroken != null) { SetRef(padlock, "_brokenVisual", _padlockBroken); _padlockBroken.SetActive(false); }
            if (_shedDoorGO != null)
            {
                var door = _shedDoorGO.GetComponent<ShedDoor>();
                if (door != null) SetRef(padlock, "_shedDoor", door);
            }
            if (!_padlockGO.TryGetComponent<Collider>(out _))
                _padlockGO.AddComponent<BoxCollider>();
        }

        // Shelves
        foreach (var shelf in new[] { _shelf1, _shelf2, _shelf3 })
        {
            if (shelf == null) continue;
            var s = EnsureComponent<ShedShelf>(shelf);
            SetVec3(s, "_scatterImpulse", new Vector3(0f, 2f, 2f));
            SetLayer(shelf, "Pickup");
            if (!shelf.TryGetComponent<Collider>(out _))
                shelf.AddComponent<BoxCollider>();
            Debug.LogWarning($"[BackYardSetup] {shelf.name}: ShedShelf added — fill _contents and _contentBodies manually in the Inspector.");
        }

        Done("Task 9: Shed Heist applied (shelf contents still need manual wiring).");
    }

    // ── Shared utilities ──────────────────────────────────────────────────────

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        if (!go.TryGetComponent<T>(out var c))
        {
            c = Undo.AddComponent<T>(go);
        }
        EditorUtility.SetDirty(go);
        return c;
    }

    static T EnsureCollider<T>(GameObject go) where T : Collider
    {
        if (!go.TryGetComponent<T>(out var c))
            c = Undo.AddComponent<T>(go);
        return c;
    }

    static T FindComponent<T>() where T : Component
        => Object.FindFirstObjectByType<T>();

    static void SetString(Component c, string field, string value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.stringValue = value; so.ApplyModifiedProperties(); }
    }

    static void SetFloat(Component c, string field, float value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.floatValue = value; so.ApplyModifiedProperties(); }
    }

    static void SetRef(Component c, string field, Object value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedProperties(); }
    }

    static void SetVec3(Component c, string field, Vector3 value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.vector3Value = value; so.ApplyModifiedProperties(); }
    }

    static void SetTag(GameObject go, string tag)
    {
        try { go.tag = tag; }
        catch { Debug.LogWarning($"[BackYardSetup] Tag '{tag}' does not exist — create it in Project Settings → Tags & Layers first."); }
    }

    static void SetLayer(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
            Debug.LogWarning($"[BackYardSetup] Layer '{layerName}' not found — create it in Project Settings → Tags & Layers.");
        else
            go.layer = layer;
    }

    static bool Require(GameObject go, string label)
    {
        if (go != null) return true;
        EditorUtility.DisplayDialog("Missing field", $"Please assign: {label}", "OK");
        return false;
    }

    static void Done(string msg)
    {
        Debug.Log("[BackYardSetup] " + msg);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    static GameObject GOField(string label, GameObject current) =>
        (GameObject)EditorGUILayout.ObjectField(label, current, typeof(GameObject), true);

    static void Space() => EditorGUILayout.Space(6);
}

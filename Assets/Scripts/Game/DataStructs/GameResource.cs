﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Grids/Resource")]
public class GameResource : ScriptableObject {
	public int Priority;
    public string resName;
    [AssetsOnly, InlineEditor(InlineEditorModes.LargePreview)]
    public Sprite sprite;
    public bool showInPanel;
}

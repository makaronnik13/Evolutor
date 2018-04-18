#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinEditor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//#define PREFAB_DEBUG

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Reflection;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [CanEditMultipleObjects]
    [InitializeOnLoad]
    public class OdinEditor : Editor
    {
        private static Func<MonoBehaviour, bool> AudioUtil_HaveAudioCallback;
        private static Func<MonoBehaviour, int> AudioUtil_GetCustomFilterChannelCount;
        private static Type AudioFilterGUIType;
        private static Action<object, MonoBehaviour> DrawAudioFilterGUI;
        private static bool HasReflectedAudioFilter;
        private static bool Initialized = false;

        private object audioFilterGUIInstance;
        //private bool hasSeenLayout = false; // See OnInspectorGUI in this type for reason as to removal

        private static readonly GUIContent networkChannelLabel = new GUIContent("Network Channel", "QoS channel used for updates. Use the [NetworkSettings] class attribute to change this.");
        private static readonly GUIContent networkSendIntervalLabel = new GUIContent("Network Send Interval", "Maximum update rate in seconds. Use the [NetworkSettings] class attribute to change this, or implement GetNetworkSendInterval");

        [NonSerialized]
        private PropertyTree tree;

        private static void EnsureInitialized()
        {
            if (!Initialized)
            {
                Initialized = true;

                try
                {
                    string haveAudioCallbackName = UnityVersion.IsVersionOrGreater(5, 6) ? "HasAudioCallback" : "HaveAudioCallback";

                    AudioUtil_HaveAudioCallback = (Func<MonoBehaviour, bool>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, bool>), typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod(haveAudioCallbackName, BindingFlags.Public | BindingFlags.Static));
                    AudioUtil_GetCustomFilterChannelCount = (Func<MonoBehaviour, int>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, int>), typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("GetCustomFilterChannelCount", BindingFlags.Public | BindingFlags.Static));
                    AudioFilterGUIType = typeof(Editor).Assembly.GetType("UnityEditor.AudioFilterGUI");
                    DrawAudioFilterGUI = EmitUtilities.CreateWeakInstanceMethodCaller<MonoBehaviour>(AudioFilterGUIType.GetMethod("DrawAudioFilterGUI", BindingFlags.Public | BindingFlags.Instance));
                    HasReflectedAudioFilter = true;
                }
                catch (Exception)
                {
                    Debug.LogWarning("The internal Unity class AudioFilterGUI has been changed; cannot properly mock a generic Unity inspector. This probably won't be very noticeable.");
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public PropertyTree Tree
        {
            get
            {
                if (this.tree == null)
                {
                    try
                    {
                        this.tree = PropertyTree.Create(this.serializedObject);
                    }
                    catch (ArgumentException) { }
                }

                return this.tree;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.tree = null;
            EnsureInitialized();

            EditorWindow inspector = this.GetInspectorWindow();

            if (inspector != null)
            {
                inspector.wantsMouseMove = InspectorConfig.Instance.ProcessMouseMoveInInspector;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual void OnDisable()
        {
            EnsureInitialized();
            EditorWindow inspector = this.GetInspectorWindow();

            if (inspector != null)
            {
                inspector.wantsMouseMove = false;
            }
        }

        /// <summary>
        /// Draws the default Odin inspector.
        /// </summary>
        public new void DrawDefaultInspector()
        {
            this.OnInspectorGUI();
        }

        /// <summary>
        /// Draws the default Unity inspector.
        /// </summary>
        public void DrawUnityInspector()
        {
            base.DrawDefaultInspector();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected EditorWindow GetInspectorWindow()
        {
            Type inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");

            try
            {
                var window = GUIHelper.CurrentWindow;

                if (window != null && window.GetType() != inspectorWindowType)
                {
                    return null;
                }

                return window;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EnsureInitialized();

            if (this.Tree == null)
            {
                base.OnInspectorGUI();
                return;
            }
            else if (this.Tree.RootPropertyCount == 0)
            {
                var assemblyTypeFlag = AssemblyUtilities.GetAssemblyTypeFlag(this.target.GetType().Assembly);

                if (assemblyTypeFlag == AssemblyTypeFlags.UnityTypes || assemblyTypeFlag == AssemblyTypeFlags.UnityEditorTypes)
                {
                    this.MockUnityGenericInspector();
                    return;
                }
            }

#if PREFAB_DEBUG
            this.PrefabModificationsDebug();
#endif

            if (Event.current.type == EventType.Layout)
            {
                this.tree.DrawMonoScriptObjectField =
                    this.tree.UnitySerializedObject != null &&
                    this.tree.TargetType != null &&
                    GeneralDrawerConfig.Instance.ShowMonoScriptInEditor &&
                    !this.tree.TargetType.IsDefined(typeof(HideMonoScriptAttribute), true);
            }

            if (this.tree.DrawMonoScriptObjectField == false)
            {
                //EditorGUILayout.Space();
            }

            this.tree.Draw(true);

            NetworkBehaviour networkBehaviour = this.target as NetworkBehaviour;

            if (networkBehaviour != null)
            {
                EditorGUILayout.LabelField(networkChannelLabel, GUIHelper.TempContent(networkBehaviour.GetNetworkChannel().ToString()));
                EditorGUILayout.LabelField(networkSendIntervalLabel, GUIHelper.TempContent(networkBehaviour.GetNetworkSendInterval().ToString()));
            }

            this.RepaintIfRequested();
            GUIHelper.CurrentWindow.wantsMouseMove = true;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected void MockUnityGenericInspector()
        {
            if (!this.IsMissingMonoBehaviourTarget() || !this.MissingMonoBehaviourGUI())
            {
                base.OnInspectorGUI();

                if (HasReflectedAudioFilter && this.target is MonoBehaviour)
                {
                    if (AudioUtil_HaveAudioCallback(this.target as MonoBehaviour) && AudioUtil_GetCustomFilterChannelCount(this.target as MonoBehaviour) > 0)
                    {
                        if (this.audioFilterGUIInstance == null)
                        {
                            this.audioFilterGUIInstance = Activator.CreateInstance(AudioFilterGUIType);
                        }

                        DrawAudioFilterGUI(this.audioFilterGUIInstance, this.target as MonoBehaviour);
                    }
                }
            }
        }

        private bool IsMissingMonoBehaviourTarget()
        {
            return this.target.GetType() == typeof(MonoBehaviour) || this.target.GetType() == typeof(ScriptableObject);
        }

        private bool MissingMonoBehaviourGUI()
        {
            this.serializedObject.Update();

            SerializedProperty serializedProperty = this.serializedObject.FindProperty("m_Script");
            bool result;

            if (serializedProperty == null)
            {
                result = false;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedProperty, new GUILayoutOption[0]);
                MonoScript monoScript = serializedProperty.objectReferenceValue as MonoScript;

                bool invalid = true;

                if (monoScript != null)
                {
                    invalid = false;
                }

                if (invalid)
                {
                    SirenixEditorGUI.WarningMessageBox("The associated script can not be loaded.\nPlease fix any compile errors\nand assign a valid script.", true);
                }
                if (this.serializedObject.ApplyModifiedProperties())
                {
                    ActiveEditorTracker.sharedTracker.ForceRebuild();
                }

                result = true;
            }
            return result;
        }

#if PREFAB_DEBUG

        // The below is occasionally used debug code to view the custom prefab
        // modifications that are on the currently selected object
        private System.Collections.Generic.List<Action> actions = new System.Collections.Generic.List<Action>();

        private void PrefabModificationsDebug()
        {
            if (Event.current.type == EventType.Layout)
            {
                this.actions.Clear();

                var supporter = this.target as Serialization.ISupportsPrefabSerialization;
                if (supporter != null)
                {
                    if (supporter.SerializationData.PrefabModifications == null)
                    {
                        this.actions.Add(() => GUILayout.Label("Prefab Modifications: NULL"));
                    }
                    else
                    {
                        var mods = supporter.SerializationData.PrefabModifications.ToArray();

                        this.actions.Add(() =>
                        {
                            GUILayout.Label("Prefab Modifications: " + mods.Length);

                            for (int i = 0; i < mods.Length; i++)
                            {
                                GUILayout.Label(mods[i]);
                            }
                        });
                    }
                }

                var prefabType = PrefabUtility.GetPrefabType(this.target);

                if (prefabType == PrefabType.PrefabInstance)
                {
                    var mods = PrefabUtility.GetPropertyModifications(this.target);

                    this.actions.Add(() =>
                    {
                        GUILayout.Label("Unity Prefab Mods: " + mods.Length);

                        foreach (var mod in mods)
                        {
                            GUILayout.Label("    " + mod.propertyPath + ": " + mod.value);
                        }
                    });
                }
            }

            for (int i = 0; i < this.actions.Count; i++)
            {
                this.actions[i]();
            }
        }

#endif
    }
}
#endif
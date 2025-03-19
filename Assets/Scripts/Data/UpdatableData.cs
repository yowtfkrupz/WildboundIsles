using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    private IUpdater updater;

    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        updater = new EditorUpdater();
#else
        updater = new BuildUpdater();
#endif
    }

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            updater.RegisterUpdateCallback(NotifyOfUpdatedValues);
        }
    }

    public void NotifyOfUpdatedValues()
    {
        updater.UnregisterUpdateCallback(NotifyOfUpdatedValues);
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

    private interface IUpdater
    {
        void RegisterUpdateCallback(System.Action callback);
        void UnregisterUpdateCallback(System.Action callback);
    }

#if UNITY_EDITOR
    private class EditorUpdater : IUpdater
    {
        public void RegisterUpdateCallback(System.Action callback)
        {
            UnityEditor.EditorApplication.update += () => callback();
        }

        public void UnregisterUpdateCallback(System.Action callback)
        {
            UnityEditor.EditorApplication.update -= () => callback();
        }
    }
#endif

    private class BuildUpdater : IUpdater
    {
        public void RegisterUpdateCallback(System.Action callback) { }
        public void UnregisterUpdateCallback(System.Action callback) { }
    }
}
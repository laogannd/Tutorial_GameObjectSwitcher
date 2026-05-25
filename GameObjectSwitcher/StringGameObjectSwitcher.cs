using System;
using System.Collections.Generic;

using Obvious.Soap;
using Sirenix.OdinInspector;
using UnityEngine;

// 监听全局 String 事件，根据预设的 key-GameObjects 字典切换显示
[AddComponentMenu("Soap/StringGameObjectSwitcher")]
public class StringGameObjectSwitcher : MonoBehaviour
{
    [Serializable]
    public class Target
    {
        [HorizontalGroup("Row"), LabelWidth(50)]
        [LabelText("物体")]
        public GameObject GameObject;

        [HorizontalGroup("Row"), LabelWidth(50)]
        [LabelText("显示")]
        public bool Show = true;
    }

    [Serializable]
    public class Entry
    {
        [LabelText("键")]
        public string Key;

        [LabelText("目标物体")]
        public Target[] Targets = Array.Empty<Target>();
    }

    [TitleGroup("事件")]
    [SerializeField, Required("必须引用 ScriptableEventString 资产")]
    [LabelText("全局 String 事件")]
    private ScriptableEventString _event;

    [TitleGroup("配置")]
    [SerializeField, LabelText("必须匹配才隐藏")]
    [Tooltip("开启后，只有成功找到键值对时才会隐藏当前已显示的物体；未找到则保持现状")]
    private bool _requireMatchToHide = true;

    [TitleGroup("配置")]
    [SerializeField, LabelText("映射表")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    private List<Entry> _entries = new();

    private Dictionary<string, Target[]> _map;
    private Target[] _currentTargets;

    private void Awake()
    {
        BuildMap();
    }

    private void OnEnable()
    {
        _event.OnRaised += OnStringEvent;
    }

    private void OnDisable()
    {
        _event.OnRaised -= OnStringEvent;
    }

    private void OnStringEvent(string key)
    {
        if (!_map.TryGetValue(key, out var targets))
        {
            if (!_requireMatchToHide) RevertCurrent();
            return;
        }

        RevertCurrent();
        ApplyTargets(targets);
        _currentTargets = targets;
    }

    private void RevertCurrent()
    {
        if (_currentTargets == null) return;
        for (int i = 0; i < _currentTargets.Length; i++)
        {
            var t = _currentTargets[i];
            if (t.GameObject != null)
                t.GameObject.SetActive(!t.Show);
        }
        _currentTargets = null;
    }

    private void ApplyTargets(Target[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            if (t.GameObject != null)
                t.GameObject.SetActive(t.Show);
        }
    }

    private void BuildMap()
    {
        _map = new Dictionary<string, Target[]>(_entries.Count);
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (string.IsNullOrEmpty(entry.Key)) continue;
            _map[entry.Key] = entry.Targets;
        }
    }
}

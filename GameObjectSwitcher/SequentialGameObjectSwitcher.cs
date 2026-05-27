using System;
using System.Collections.Generic;

using Obvious.Soap;
using Sirenix.OdinInspector;
using UnityEngine;

// 监听全局 String 事件，每次收到相同 key 时按顺序推进到下一步执行物体显隐
[AddComponentMenu("Soap/SequentialGameObjectSwitcher")]
public class SequentialGameObjectSwitcher : MonoBehaviour
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
    public class Step
    {
        [LabelText("目标物体")]
        public Target[] Targets = Array.Empty<Target>();
    }

    [Serializable]
    public class Entry
    {
        [LabelText("键")]
        public string Key;

        [LabelText("步骤序列")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        public Step[] Steps = Array.Empty<Step>();
    }

    [TitleGroup("事件")]
    [SerializeField, Required("必须引用 ScriptableEventString 资产")]
    [LabelText("全局 String 事件")]
    private ScriptableEventString _event;

    [TitleGroup("配置")]
    [SerializeField, LabelText("循环")]
    [Tooltip("到达最后一步后是否循环回第一步，关闭则停留在最后一步")]
    private bool _loop;

    [TitleGroup("配置")]
    [SerializeField, LabelText("映射表")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    private List<Entry> _entries = new();

    private Dictionary<string, Entry> _map;
    private Dictionary<string, int> _stepIndices;
    private Dictionary<string, Target[]> _lastApplied;

    private void Awake()
    {
        BuildMap();
        _stepIndices = new Dictionary<string, int>(_map.Count);
        _lastApplied = new Dictionary<string, Target[]>(_map.Count);
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
        if (!_map.TryGetValue(key, out var entry)) return;
        if (entry.Steps.Length == 0) return;

        _stepIndices.TryGetValue(key, out int currentIndex);

        // 还原上一步
        if (_lastApplied.TryGetValue(key, out var lastTargets))
            RevertTargets(lastTargets);

        // 执行当前步骤
        var step = entry.Steps[currentIndex];
        ApplyTargets(step.Targets);
        _lastApplied[key] = step.Targets;

        // 推进索引
        int nextIndex = currentIndex + 1;
        if (nextIndex >= entry.Steps.Length)
            nextIndex = _loop ? 0 : currentIndex;
        _stepIndices[key] = nextIndex;
    }

    private void RevertTargets(Target[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            if (t.GameObject != null)
                t.GameObject.SetActive(!t.Show);
        }
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
        _map = new Dictionary<string, Entry>(_entries.Count);
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (string.IsNullOrEmpty(entry.Key)) continue;
            _map[entry.Key] = entry;
        }
    }
}

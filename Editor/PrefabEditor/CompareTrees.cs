using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Plugins;
using Plugins.models;
using Plugins.models.orm;

namespace Editor.PrefabEditor
{
    public class TreeNode
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public int? ParentValue { get; set; }
        public HashSet<int> ChildrenValues { get; } = new();
    }

    public class ChildrenMismatch
    {
        public int Value { get; set; }
        public int[] OnlyInTransform { get; set; }
        public int[] OnlyInInfo { get; set; }
    }

    public class CompareTrees
    {
        private readonly Transform _root;

        private readonly Dictionary<int, TreeNode> _transformNodes = new();
        private readonly Dictionary<int, List<string>> _transformValuePaths = new();
        private readonly Dictionary<int, List<string>> _transformDuplicates = new();

        private readonly Dictionary<int, TreeNode> _infoNodes = new();
        private readonly Dictionary<int, HashSet<int>> _infoParentValues = new();
        private readonly Dictionary<int, HashSet<int>> _infoMultiParents = new();
        private readonly Queue<Info> _infoQueue = new();
        private readonly HashSet<int> _queuedInfoValues = new();

        private int[] _onlyInTransform = Array.Empty<int>();
        private int[] _onlyInInfo = Array.Empty<int>();
        private int[] _both = Array.Empty<int>();
        private int[] _nameMismatches = Array.Empty<int>();
        private int[] _parentMismatches = Array.Empty<int>();
        private ChildrenMismatch[] _childrenMismatches = Array.Empty<ChildrenMismatch>();
        private int _updatedNameCount;
        private int[] _nameUpdateFailures = Array.Empty<int>();


        // 运行完整比较流程：构建两棵树、计算差异、输出结果。
        public static void Run( Transform root)
        {
            var instance = new CompareTrees(root);
            instance.BuildTransformTree();
            instance.BuildInfoTree();
            instance.CalculateDifferences();
            instance.UpdateInfoNames();
            instance.LogResult();
        }

        // 构造函数：保存本次要对比的 Transform 根节点。
        private CompareTrees(Transform root)
        {
            _root = root;
        }
        
        // 从 Transform 层级解析 BodyStruct，并建立模型树的 value 索引。
        private void BuildTransformTree()
        {
            foreach (var transform in PrefabCollection.ForEachChildren(_root))
            {
                if (!BodyStruct.GetFromPrefab(transform.name, out var body))
                {
                    continue;
                }

                AddTransformPath(body.value, transform);
                AddTransformNode(body, transform);
            }

            LinkTransformChildren();
            CacheTransformDuplicates();
        }

        // 记录同一个 value 在 Transform 树中的所有路径，用于后续发现重复 value。
        private void AddTransformPath(int value, Transform transform)
        {
            if (!_transformValuePaths.TryGetValue(value, out var sameValuePaths))
            {
                sameValuePaths = new List<string>();
                _transformValuePaths.Add(value, sameValuePaths);
            }

            sameValuePaths.Add(GetTransformPath(transform));
        }

        // 将一个可解析的 Transform 节点加入模型索引，重复 value 只保留首个节点。
        private void AddTransformNode(BodyStruct body, Transform transform)
        {
            if (_transformNodes.ContainsKey(body.value))
            {
                return;
            }

            _transformNodes.Add(
                body.value,
                new TreeNode
                {
                    Value = body.value,
                    Name = body.name,
                    ParentValue = FindParentValue(transform)
                }
            );
        }

        // 根据每个 Transform 节点的父级 value，反向补齐父节点的 ChildrenValues。
        private void LinkTransformChildren()
        {
            foreach (var node in _transformNodes.Values)
            {
                if (node.ParentValue.HasValue &&
                    _transformNodes.TryGetValue(node.ParentValue.Value, out var parent))
                {
                    parent.ChildrenValues.Add(node.Value);
                }
            }
        }

        // 从路径缓存中筛出 Transform 里的重复 value。
        private void CacheTransformDuplicates()
        {
            foreach (
                var item in _transformValuePaths.Where(
                    static item => item.Value.Count > 1)
                )
            {
                _transformDuplicates.Add(item.Key, item.Value);
            }
        }

        // 以模型树中出现过的 value 为入口，查询 info 表并构建数据库树索引。
        private void BuildInfoTree()
        {
            EnqueueKnownInfoNodes();

            while (_infoQueue.Count > 0)
            {
                var parentInfo = _infoQueue.Dequeue();
                AddInfoChildren(parentInfo);
            }

            CacheInfoMultiParents();
        }

        // 查询模型中每个 value 对应的 info，并作为数据库树遍历入口。
        private void EnqueueKnownInfoNodes()
        {
            foreach (var transformNode in _transformNodes.Values)
            {
                if (!AnatomyDatabase.FindBodyFromValue(transformNode.Value, out var info))
                {
                    continue;
                }

                AddOrUpdateInfoNode(info, null);
                EnqueueInfo(info);
            }
        }

        // 查询一个 info 节点的直接子级，并把子级加入数据库树索引和遍历队列。
        private void AddInfoChildren(Info parentInfo)
        {
            if (!_infoNodes.TryGetValue(parentInfo.Value, out var parentNode))
            {
                return;
            }

            foreach (var childInfo in AnatomyDatabase.FindAllChildren(parentInfo))
            {
                parentNode.ChildrenValues.Add(childInfo.Value);
                AddInfoParentValue(childInfo.Value, childInfo.Pval);
                AddOrUpdateInfoNode(childInfo, childInfo.Pval);
                EnqueueInfo(childInfo);
            }
        }

        // 新增或补全一个 info 节点；同 value 已存在时只补名称和父级。
        private void AddOrUpdateInfoNode(Info info, int? parentValue)
        {
            if (!_infoNodes.TryGetValue(info.Value, out var node))
            {
                _infoNodes.Add(
                    info.Value,
                    new TreeNode
                    {
                        Value = info.Value,
                        Name = info.Name,
                        ParentValue = parentValue
                    }
                );
                return;
            }

            if (string.IsNullOrEmpty(node.Name))
            {
                node.Name = info.Name;
            }

            if (!node.ParentValue.HasValue && parentValue.HasValue)
            {
                node.ParentValue = parentValue;
            }
        }

        // 记录 info 表中某个 value 出现过的父级 pval，用于发现多父级异常。
        private void AddInfoParentValue(int value, int parentValue)
        {
            if (!_infoParentValues.TryGetValue(value, out var parents))
            {
                parents = new HashSet<int>();
                _infoParentValues.Add(value, parents);
            }

            parents.Add(parentValue);
        }

        // 将未遍历过的 info 节点加入队列，避免循环或重复查询。
        private void EnqueueInfo(Info info)
        {
            if (_queuedInfoValues.Add(info.Value))
            {
                _infoQueue.Enqueue(info);
            }
        }

        // 从 info 父级缓存中筛出同一 value 对应多个 pval 的异常。
        private void CacheInfoMultiParents()
        {
            foreach (
                var item in _infoParentValues.Where(
                    static item => item.Value.Count > 1)
                )
            {
                _infoMultiParents.Add(item.Key, item.Value);
            }
        }

        // 基于两边的 value 集合和父子关系，计算所有差异集合。
        private void CalculateDifferences()
        {
            var transformValues = _transformNodes.Keys.ToHashSet();
            var infoValues = _infoNodes.Keys.ToHashSet();

            _onlyInTransform = transformValues.Except(infoValues).OrderBy(static value => value).ToArray();
            _onlyInInfo = infoValues.Except(transformValues).OrderBy(static value => value).ToArray();
            _both = transformValues.Intersect(infoValues).OrderBy(static value => value).ToArray();

            CalculateNameMismatches();
            CalculateParentMismatches();
            CalculateChildrenMismatches();
        }

        // 计算两边 value 相同、但 name 不完全相等的节点。
        private void CalculateNameMismatches()
        {
            _nameMismatches = _both
                .Where(value => !string.Equals(
                    _transformNodes[value].Name,
                    _infoNodes[value].Name,
                    StringComparison.Ordinal))
                .ToArray();
        }

        // 将 name 不一致节点的 Transform 名称写回 Info 表，并记录更新结果。
        private void UpdateInfoNames()
        {
            var failures = new List<int>();

            foreach (var value in _nameMismatches)
            {
                var transformNode = _transformNodes[value];
                var info = new Info
                {
                    Value = value,
                    Name = transformNode.Name
                };

                if (AnatomyDatabase.UpdateName(info))
                {
                    _updatedNameCount++;
                }
                else
                {
                    failures.Add(value);
                }
            }

            _nameUpdateFailures = failures.ToArray();
        }

        // 计算两边都存在、但父级 value 与 info.pval 不一致的节点。
        private void CalculateParentMismatches()
        {
            _parentMismatches = _both
                .Where(value => _infoNodes[value].ParentValue.HasValue)
                .Where(value => _transformNodes[value].ParentValue != _infoNodes[value].ParentValue)
                .ToArray();
        }

        // 计算两边都存在、但直接子级集合不一致的节点。
        private void CalculateChildrenMismatches()
        {
            _childrenMismatches = _both
                .Select(value => new ChildrenMismatch
                {
                    Value = value,
                    OnlyInTransform = _transformNodes[value].ChildrenValues
                        .Except(_infoNodes[value].ChildrenValues)
                        .OrderBy(static child => child)
                        .ToArray(),
                    OnlyInInfo = _infoNodes[value].ChildrenValues
                        .Except(_transformNodes[value].ChildrenValues)
                        .OrderBy(static child => child)
                        .ToArray()
                })
                .Where(static item => item.OnlyInTransform.Length > 0 || item.OnlyInInfo.Length > 0)
                .ToArray();
        }

        // 输出比较摘要和每一类差异明细。
        private void LogResult()
        {
            Debug.Log(
                $"结构比较完成：Transform节点={_transformNodes.Count}, Info节点={_infoNodes.Count}, " +
                $"模型独有={_onlyInTransform.Length}, 数据库独有={_onlyInInfo.Length}, " +
                $"名称不一致={_nameMismatches.Length}, 名称更新成功={_updatedNameCount}, " +
                $"名称更新失败={_nameUpdateFailures.Length}, " +
                $"父级不一致={_parentMismatches.Length}, 子级不一致={_childrenMismatches.Length}, " +
                $"Transform重复value={_transformDuplicates.Count}, Info多父级value={_infoMultiParents.Count}"
            );

            LogOnlyInTransform();
            LogOnlyInInfo();
            LogTransformDuplicates();
            LogInfoMultiParents();
            LogNameMismatches();
            LogParentMismatches();
            LogChildrenMismatches();
        }

        // 逐个输出模型中存在、Info 表中不存在的节点。
        private void LogOnlyInTransform()
        {
            foreach (var value in _onlyInTransform)
            {
                Debug.LogWarning($"模型中存在，Info表不存在：{FormatNode(_transformNodes[value])}");
            }
        }

        // 逐个输出 Info 表中存在、模型中不存在的节点。
        private void LogOnlyInInfo()
        {
            foreach (var value in _onlyInInfo)
            {
                Debug.LogWarning($"Info表存在，模型中不存在：{FormatNode(_infoNodes[value])}");
            }
        }

        // 逐个输出 Transform 树中重复 value 的路径。
        private void LogTransformDuplicates()
        {
            foreach (var item in _transformDuplicates)
            {
                Debug.LogWarning($"Transform value重复：value={item.Key}，路径=[{string.Join(" | ", item.Value)}]");
            }
        }

        // 逐个输出 Info 表中同一 value 对应多个父级的异常。
        private void LogInfoMultiParents()
        {
            foreach (var item in _infoMultiParents)
            {
                var node = FormatInfoNodeValue(item.Key);
                Debug.LogWarning($"Info value存在多个父级：{node}");

                foreach (var parentValue in item.Value)
                {
                    Debug.LogWarning(
                        $"Info多父级节点：子节点={node}，父节点={FormatInfoNodeValue(parentValue)}"
                    );
                }
            }
        }

        // 逐个输出 value 相同但 name 不相等的节点、修改前的两边数据及更新结果。
        private void LogNameMismatches()
        {
            foreach (var value in _nameMismatches)
            {
                var updateSucceeded = !_nameUpdateFailures.Contains(value);
                Debug.LogWarning(
                    $"名称不一致：Transform={FormatNode(_transformNodes[value])}，" +
                    $"Info修改前={FormatNode(_infoNodes[value])}，" +
                    $"Info名称更新={(updateSucceeded ? "成功" : "失败")}"
                );
            }
        }

        // 逐个输出父级不一致的节点，以及两边记录的父级。
        private void LogParentMismatches()
        {
            foreach (var value in _parentMismatches)
            {
                var transformNode = _transformNodes[value];
                var infoNode = _infoNodes[value];
                Debug.LogWarning(
                    $"父级不一致：{FormatNode(transformNode)}，" +
                    $"Transform父级={FormatTransformNodeValue(transformNode.ParentValue)}, " +
                    $"Info父级={FormatInfoNodeValue(infoNode.ParentValue)}"
                );
            }
        }

        // 逐个输出子级集合不一致的父节点和每一个缺失/多出的子节点。
        private void LogChildrenMismatches()
        {
            foreach (var mismatch in _childrenMismatches)
            {
                Debug.LogWarning($"子级不一致父节点：{FormatNode(_transformNodes[mismatch.Value])}");

                foreach (var childValue in mismatch.OnlyInTransform)
                {
                    Debug.LogWarning(
                        $"子级不一致，仅模型有：父节点={FormatNode(_transformNodes[mismatch.Value])}，" +
                        $"子节点={FormatTransformNodeValue(childValue)}"
                    );
                }

                foreach (var childValue in mismatch.OnlyInInfo)
                {
                    Debug.LogWarning(
                        $"子级不一致，仅Info有：父节点={FormatNode(_infoNodes[mismatch.Value])}，" +
                        $"子节点={FormatInfoNodeValue(childValue)}"
                    );
                }
            }
        }

        // 从当前 Transform 开始向上查找最近一个可解析 BodyStruct 的父级 value。
        private static int? FindParentValue(Transform transform)
        {
            var parent = transform.parent;
            while (parent != null)
            {
                if (BodyStruct.GetFromPrefab(parent.name, out var parentBody))
                {
                    return parentBody.value;
                }

                parent = parent.parent;
            }

            return null;
        }

        // 拼接 Transform 在场景层级中的完整路径，用于定位重复 value。
        private static string GetTransformPath(Transform transform)
        {
            var path = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

        // 将节点格式化为 name~value，统一所有差异日志的展示。
        private static string FormatNode(TreeNode node)
        {
            return $"{node.Name}~{node.Value}";
        }

        // 按 Transform 索引格式化 value，索引不存在时退回纯 value。
        private string FormatTransformNodeValue(int? value)
        {
            return FormatNodeValue(value, _transformNodes);
        }

        // 按 Info 索引格式化 value，索引不存在时退回纯 value。
        private string FormatInfoNodeValue(int? value)
        {
            return FormatNodeValue(value, _infoNodes);
        }

        // 在指定索引里把 value 解析为 name~value，空父级显示为 null。
        private static string FormatNodeValue(int? value, Dictionary<int, TreeNode> nodes)
        {
            if (!value.HasValue)
            {
                return "null";
            }

            return nodes.TryGetValue(value.Value, out var node)
                ? FormatNode(node)
                : value.Value.ToString();
        }
    }
}

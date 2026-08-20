using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.orm.Models;

namespace Plugins.orm.Servers
{
    public class BonemarksServer : AnatomyDatabase
    {
        // 标签组
        public IReadOnlyList<Bonemarks> BonemarksList => _bonemarks;
        private List<Bonemarks> _bonemarks;
        // 预删除项目缓存
        private readonly List<int> _bonemarkIdsDeleted;

        //骨骼模型
        public int Gender { get; }
        public int[] Family { get; private set; }
        public int OrderNum { get; private set; }

        // sql通配符
        private string Placeholders => string.Join(",", Enumerable.Repeat("?", Family.Length));

        public BonemarksServer(int gender)
        {
            if (gender is not 0 and not 1)
            {
                throw new ArgumentOutOfRangeException(nameof(gender), gender, "性别字段只能为 0 或 1。");
            }

            Gender = gender;
            _bonemarks = new List<Bonemarks>();
            _bonemarkIdsDeleted = new List<int>();
        }

        /// <summary>
        /// 缓存一个标志数据
        /// </summary>
        /// <param name="newMark">新标签</param>
        /// <param name="index">被替换的项目位于Bonemarks的序号，为-1则执行新建</param>
        /// <returns>更新的标签位于Bonemarks的序号</returns>
        public bool SaveUpdateMark(Bonemarks newMark, ref int index)
        {
            if (index < 0)
            {
                _bonemarks.Add(newMark);
                index = _bonemarks.Count - 1;
                return true;
            }

            var oldMark = _bonemarks[index];
            if (newMark.BePainting)
            {
                if (oldMark.Type != newMark.Type || oldMark.Value != newMark.Value)
                    return false;

                oldMark.Color = newMark.Color;
                oldMark.Uv = newMark.Uv;
                oldMark.Name = newMark.Name;
                oldMark.Position = newMark.Position;
                oldMark.Rotation = newMark.Rotation;
            }
            else if (newMark.BeForamen)
            {
                if (oldMark.Type != newMark.Type)
                    return false;

                oldMark.PlaneValue = newMark.PlaneValue;
                oldMark.Name = newMark.Name;
                oldMark.Position = newMark.Position;
                oldMark.Rotation = newMark.Rotation;
            }

            return false;
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="index">被删除的项目的角标</param>
        public void DeleteMark(int index)
        {
            var idOfDel = _bonemarks[index].Id;
            if (idOfDel > 0)
            {
                _bonemarkIdsDeleted.Add(idOfDel);
            }
            _bonemarks.RemoveAt(index);
        }

        /// <summary>
        /// 传入多个value和标志类型，查询并返回全部骨性标志
        /// </summary>
        /// <param name="markType">标志类型</param>
        /// <returns>标志对象集合</returns>
        public void FindAllBonemarks(int markType)
        {
            if (Family == null || Family.Length == 0)
                throw new ArgumentNullException(nameof(Family), "当前无骨骼模型");

            // 参数 - 合并骨骼类型和骨骼模型族群
            var arguments = new object[Family.Length + 1];
            arguments[0] = markType;
            Array.Copy(Family, 0, arguments, 1, Family.Length);

            _bonemarks = DB.Query<Bonemarks>(
                $"SELECT * FROM bone_marks WHERE type=? AND value IN ({Placeholders})",
                arguments
            );
        }

        /// <summary>
        /// 保存 Bonemarks 集合中的全部对象。Id 为 0 时新增，否则按 Id 更新。
        /// </summary>
        public void SaveAllBonemarks()
        {
            if (_bonemarks == null)
                return;

            DB.RunInTransaction(
                () => {
                    foreach (var bonemark in _bonemarks)
                    {
                        if (bonemark == null)
                            throw new InvalidOperationException("骨性标志集合中存在空对象。");

                        if (bonemark.Id == 0)
                            DB.Insert(bonemark);
                        else
                            DB.Update(bonemark);
                    }

                    foreach (var id in _bonemarkIdsDeleted)
                    {
                        DB.Delete<Bonemarks>(id);
                    }
                }
            );

            _bonemarkIdsDeleted.Clear();
        }

        /// <summary>
        /// 获取一个新的骨骼组合
        /// </summary>
        /// <returns>该坐标是否存在数据</returns>
        public bool TryGenerateBonemarkView(int nextIndex)
        {
            var minValue = 1000000 + Gender * 10000;
            var maxValue = minValue + 10000;
            var allFamily = DB.QueryScalars<string>(
                "SELECT DISTINCT family FROM info WHERE value>=? AND value<? ORDER BY value",
                minValue, maxValue
            );

            if (nextIndex < 0 || nextIndex >= allFamily.Count)
            {
                return false;
            }

            var familyText = allFamily[nextIndex];
            if (string.IsNullOrWhiteSpace(familyText))
            {
                return false;
            }

            Family = familyText.Split(';').Select(int.Parse).ToArray();
            OrderNum = nextIndex;
            _bonemarks.Clear();
            return true;
        }

        /// <summary>
        /// 获取当前family关联的孔洞value
        /// </summary>
        /// <returns>输出孔洞模型value</returns>
        public int[] GenerateBonemarkForamens()
        {
            if (Family.Length == 0)
            {
                return Array.Empty<int>();
            }

            var planeValues = DB.QueryScalars<int>(
                "SELECT plane_value FROM bone_marks " +
                $"WHERE plane_value IS NOT NULL AND type=1 AND value IN ({Placeholders})",
                Family.Select(e => (object)e).ToArray()
            );

            return planeValues.ToArray();
        }
    }
}

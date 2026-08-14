using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.orm.Models;

namespace Plugins.orm.Servers
{
    public class BonemarksServer : AnatomyDatabase
    {
        public int Gender;
        public int[] Family;
        public List<Bonemarks> Bonemarks;
        public int OrderNum;

        public BonemarksServer(int gender)
        {
            if (gender is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(gender), gender, "性别字段只能为 0 或 1。");

            Gender = gender;
            OrderNum = 0;
        }

        public int SavingMark(Bonemarks newMark, int index=-1)
        {
            if (index == -1)
            {
                Bonemarks.Add(newMark);
                return Bonemarks.Count - 1;
            }

            newMark.Name = Bonemarks[index].Name;
            Bonemarks[index] = newMark;
            return index;
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

            // 占位符
            var placeholders = string.Join(",", Enumerable.Repeat("?", Family.Length));

            // 参数
            var arguments = new object[Family.Length + 1];
            arguments[0] = markType;
            for (var i = 0; i < Family.Length; i++)
                arguments[i + 1] = Family[i];

            Bonemarks = DB.Query<Bonemarks>(
                $"SELECT * FROM bone_marks WHERE type=? AND value IN ({placeholders})",
                arguments
            );
        }

        /// <summary>
        /// 保存 Bonemarks 集合中的全部对象。Id 为 0 时新增，否则按 Id 更新。
        /// </summary>
        public void SaveAllBonemarks()
        {
            if (Bonemarks == null)
                throw new ArgumentNullException(nameof(Bonemarks), "骨性标志集合为空。");

            DB.RunInTransaction(
                () => {
                    foreach (var bonemark in Bonemarks)
                    {
                        if (bonemark == null)
                            throw new InvalidOperationException("骨性标志集合中存在空对象。");

                        if (bonemark.Id == 0)
                            DB.Insert(bonemark);
                        else
                            DB.Update(bonemark);
                    }
                }
            );
        }

        public void GenerateBonemarkView()
        {
            var minValue = 1000000 + Gender * 10000;
            var maxValue = minValue + 10000;
            var families = DB.QueryScalars<string>(
                "SELECT DISTINCT family FROM info WHERE value>=? AND value<? ORDER BY value",
                minValue, maxValue
            );

            if (OrderNum < 0 || OrderNum >= families.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(OrderNum), OrderNum,
                    $"排序位置必须在 0 到 {families.Count - 1} 之间。"
                );
            }

            var familyText = families[OrderNum];
            if (string.IsNullOrWhiteSpace(familyText))
            {
                throw new InvalidOperationException($"排序位置 {OrderNum} 对应的 family 为空。");
            }

            Family = familyText.Split(';').Select(value => int.Parse(value.Trim())).ToArray();
        }
    }
}

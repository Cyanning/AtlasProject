using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Plugins.models.orm;

namespace Plugins
{
    public sealed class AnatomyDatabase : IDisposable
    {
        private readonly SQLiteConnection _db = new("D:/AnatomyLibrary/AnatomyBeta.db");

        public void Dispose()
        {
            _db.Dispose();
        }

        public bool FindBodyFromValue(int value, out Info body)
        {
            var bodys = _db.Query<Info>("SELECT value, name FROM info WHERE value=?", value);

            if (bodys.Count == 0)
            {
                body = null;
                return false;
            }

            body = bodys[0];
            return true;
        }

        public Info[] FindAllChildren(Info parent)
        {
            var result = _db.Query<Info>("SELECT value, name, pval FROM info WHERE pval=?", parent.Value);
            return result.ToArray();
        }

        // 根据 info.value 更新对应记录的 name，只修改名称字段。
        public bool UpdateName(Info info)
        {
            var affectedRows = _db.Execute("UPDATE info SET name=? WHERE value=?", info.Name, info.Value);
            return affectedRows > 0;
        }

        /// <summary>
        /// 传入多个value和标志类型，查询并返回全部骨性标志
        /// </summary>
        /// <param name="values">模型value的集合</param>
        /// <param name="markType">标志类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">传入的值为空</exception>
        public Bonemarks[] FindAllBonemarks(IEnumerable<int> values, int markType)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var valueArray = values.Distinct().ToArray();
            if (valueArray.Length == 0)
                return Array.Empty<Bonemarks>();

            // 占位符
            var placeholders = string.Join(",", Enumerable.Repeat("?", valueArray.Length));

            // 参数
            var arguments = new int[valueArray.Length + 1];
            arguments[0] = markType;
            for (var i = 0; i < valueArray.Length; i++)
                arguments[i + 1] = valueArray[i];

            return _db.Query<Bonemarks>(
                $"SELECT * FROM Bonemarks WHERE type=? AND value IN ({placeholders})", arguments
            ).ToArray();
        }
    }
}

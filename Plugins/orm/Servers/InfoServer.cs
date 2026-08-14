using Plugins.orm.Models;

namespace Plugins.orm.Servers
{
    public sealed class InfoServer : AnatomyDatabase
    {
        public bool FindBodyFromValue(int value, out Info body)
        {
            var bodys = DB.Query<Info>("SELECT value, name FROM info WHERE value=?", value);

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
            var result = DB.Query<Info>("SELECT value, name, pval FROM info WHERE pval=?", parent.Value);
            return result.ToArray();
        }

        // 根据 info.value 更新对应记录的 name，只修改名称字段。
        public bool UpdateName(Info info)
        {
            var affectedRows = DB.Execute("UPDATE info SET name=? WHERE value=?", info.Name, info.Value);
            return affectedRows > 0;
        }
    }
}

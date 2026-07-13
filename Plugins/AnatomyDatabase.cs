using SQLite;
using Plugins.models.orm;

namespace Plugins
{
    public static class AnatomyDatabase
    {
        private const string DatabasePath = "D:/AnatomyLibrary/AnatomyBeta.db";

        public static bool FindBodyFromValue(int value, out Info body)
        {
            using var db = new SQLiteConnection(DatabasePath);
            var bodys = db.Query<Info>("SELECT value, name FROM info WHERE value=?", value);

            if (bodys.Count == 0)
            {
                body = null;
                return false;
            }

            body = bodys[0];
            return true;
        }

        public static Info[] FindAllChildren(Info parent)
        {
            using var db = new SQLiteConnection(DatabasePath);
            var result = db.Query<Info>("SELECT value, name, pval FROM info WHERE pval=?", parent.Value);
            return result.ToArray();
        }

        // 根据 info.value 更新对应记录的 name，只修改名称字段。
        public static bool UpdateName(Info info)
        {
            using var db = new SQLiteConnection(DatabasePath);
            var affectedRows = db.Execute("UPDATE info SET name=? WHERE value=?", info.Name, info.Value);
            return affectedRows > 0;
        }
    }
}

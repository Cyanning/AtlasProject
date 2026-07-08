using SQLite;
using Plugins.C_.models.orm;

namespace Plugins.C_
{
    public static class AnatomyDatabase
    {
        public static bool FindBodyFromValue(int value, out Info body)
        {
            using var db = new SQLiteConnection("D:/AnatomyLibrary/AnatomyBeta.db");

            var sql = $"SELECT value, name FROM info WHERE value={value}";
            var bodys = db.Query<Info>(sql);

            if (bodys.Count != 1)
            {
                body = null;
                return false;
            }

            body = bodys[0];
            return true;
        }
    }
}

using System;
using SQLite;


namespace Plugins.orm.Servers
{
    public class AnatomyDatabase : IDisposable
    {
        protected readonly SQLiteConnection DB = new("D:/AnatomyLibrary/AnatomyBeta.db");

        public void Dispose()
        {
            DB.Dispose();
        }
    }
}

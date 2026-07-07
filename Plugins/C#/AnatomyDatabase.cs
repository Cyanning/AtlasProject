using System.Collections.Generic;
using System.Data;
using SQLite;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class AnatomyDatabase
    {
        public BodyStruct FindBodyFromValue(int value)
        {
            using (var db = new SQLiteConnection("URI=file:D:/AnatomyLibrary/AnatomyBeta.db"))
            {
                List<BodyStruct> bodys = db.Query<BodyStruct>();
            }
        }
    }
}

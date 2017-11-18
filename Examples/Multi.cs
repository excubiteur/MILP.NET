using System;

using MILP.NET;
using static MILP.NET.Model;

namespace Examples
{
    class Multi
    {
        static public void Run()
        {
            var m = new Model();

            var ORIG = m.CreateSet();
            var DEST = m.CreateSet();
            var PROD = m.CreateSet();

            var supply = m.CreateParam(ORIG, PROD);
            var demand = m.CreateParam(DEST, PROD);

            var limit = m.CreateParam(ORIG, DEST);

            var cost = m.CreateParam(ORIG, DEST, PROD);

            //var Trans = m.CreateVar(ORIG, DEST, PROD);

        }
    }    
}

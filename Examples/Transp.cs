using System;

using MILP.NET;
using static MILP.NET.Model;

namespace Examples
{
    class Transp
    {
        static public void Run()
        {
            var m = new Model();

            var ORIG = m.CreateSet().Data(new string[] {
                "GARY", "CLEV", "PITT" });

            var DEST = m.CreateSet().Data(new string[] {
                "FRA","DET","LAN","WIN","STL","FRE","LAF" });

            var supply = m.CreateParam(ORIG).Data(new double[] {
                1400, 2600, 2900 });

            var demand = m.CreateParam(DEST).Data(new double[] {
                900, 1200, 600, 400, 1700, 1100, 1000 });

            var cost = m.CreateParam(ORIG,DEST).Data(new double[,] {
                {   39,   14,  11,   14,   16,   82,    8},
                {   27,    9,   12,    9,   26,   95,   17},
                {   24,   14,   17,   13,   28,   99,   20}
            });

            var Trans = m.CreateVar(ORIG, DEST).LowerBound(0);

            m.SealData();

            m.Minimize(Sum(ORIG,DEST, (i,j) => cost[i,j]*Trans[i,j]));

            m.SubjectTo(ORIG, (i) =>
                Sum(DEST, (j) => Trans[i,j]) == supply[i]
            );

            m.SubjectTo(DEST, (j) =>
                Sum(ORIG, (i) => Trans[i, j]) == demand[j]
            );

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue + 0.5) == 196200);
                solver.GetValues(Trans, (name1, name2, value) => {
                    Console.WriteLine(name1 + "," + name2 + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue + 0.5) == 196200);
                solver.GetValues(Trans, (name1, name2, value) => {
                    Console.WriteLine(name1 + "," + name2 + " = " + value.ToString());
                });
            }
        }
    }
}

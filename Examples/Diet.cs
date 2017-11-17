using System;

using MILP.NET;
using static MILP.NET.Model;

namespace Examples
{
    class Diet
    {
        public static void Run()
        {
            var m = new Model();

            var NUTR = m.CreateSet().Data(new string[] {
                "A", "B1", "B2", "C" });

            var FOOD = m.CreateSet().Data(new string[] {
                "BEEF", "CHK", "FISH", "HAM", "MCH", "MTL", "SPG", "TUR" });

            var cost = m.CreateParam(FOOD).Data(new double[] {
                3.19, 2.59, 2.29, 2.89, 1.89, 1.99, 1.99, 2.49 });

            var f_min = m.CreateParam(FOOD).Data(new double[] {
                0,0,0,0,0,0,0,0 });

            var f_max = m.CreateParam(FOOD).Data(new double[] {
                100,100,100,100,100,100,100,100 });

            var n_min = m.CreateParam(NUTR).Data(new double[] {
                700,700,700,700 });

            var n_max = m.CreateParam(NUTR).Data(new double[] {
                10000,10000,10000,10000 });

            var amt = m.CreateParam(NUTR, FOOD).Data(new double[,] {
                {60,8,8,40, 15, 70, 25, 60},
                {20,0,10,40,35,30,50,20},
                {10,20,15,35,15,15,25,15},
                {15,20,10,10,15,15,15,10}  }   );

            var Buy = m.CreateVar(FOOD).
                LowerBound((j) => f_min[j]).
                UpperBound((j) => f_max[j]);

            m.SealData();

            m.Minimize(Sum(FOOD, (j) => cost[j] * Buy[j]));

            m.SubjectTo(NUTR, (i) => 
                        n_min[i] <= Sum(FOOD, (j) => amt[i,j]*Buy[j]) <= n_max[i]
            );

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10 + 0.5) == 882);
                solver.GetValues(Buy, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10 + 0.5) == 882);
                solver.GetValues(Buy, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }
        }
    }
}

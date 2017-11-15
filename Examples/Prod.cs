using System;
using System.Collections.Generic;

using MILP.NET;
using static MILP.NET.Model;

namespace Examples
{
    class Prod
    {
        private static void Run(
            IList<string> P_data,
            IList<double> a_data,
            IList<double> c_data,
            IList<double> u_data
            )
        {
            var m = new Model();

            var P = m.CreateSet();

            var a = m.CreateParam(P);
            double b = 40;
            var c = m.CreateParam(P);
            var u = m.CreateParam(P);

            var X = m.CreateVar(P);


            {
                foreach (var p in P_data)
                {
                    P.Add(p);
                }

                int data_index = 0;
                foreach (var p in P_data)
                {
                    a.Add(p, a_data[data_index]);
                    c.Add(p, c_data[data_index]);
                    u.Add(p, u_data[data_index]);
                    ++data_index;
                }
                m.SealData();
            }


            m.Maximize(
                Sum(P, (j) => c[j] * X[j])
                );

            m.SubjectTo(
                Sum(P, (j) => 1 / a[j] * X[j]) <= b
                );

            m.SubjectTo(P,
                (j) => 0 <= X[j] <= u[j]
                );

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                solver.GetValues(X, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                solver.GetValues(X, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }
        }

        public static void Run()
        {
            var P_data = new List<string> { "bands", "coils" };
            var a_data = new List<double> { 200, 140 };
            var c_data = new List<double> { 25, 30 };
            var u_data = new List<double> { 6000, 4000 };
            Run(P_data, a_data, c_data, u_data);
        }
    }
}

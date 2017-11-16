using System;
using System.Collections.Generic;

using MILP.NET;
using static MILP.NET.Model;

namespace Examples
{
    class Steel3
    {
        private static void Run(
            IList<string> PROD_data,
            IList<double> rate_data,
            IList<double> profit_data,
            IList<double> commit_data,
            IList<double> market_data
            )
        {
            var m = new Model();

            var PROD = m.CreateSet();

            var rate = m.CreateParam(PROD);
            double avail = 40;

            var profit = m.CreateParam(PROD);
            var commit = m.CreateParam(PROD);
            var market = m.CreateParam(PROD);

            var Make = m.CreateVar(PROD).
                LowerBound( (p) => commit[p]).
                UpperBound((p) => market[p]);

            {
                foreach (var p in PROD_data)
                {
                    PROD.Add(p);
                }

                int data_index = 0;
                foreach (var p in PROD_data)
                {
                    rate.Add(p, rate_data[data_index]);
                    profit.Add(p, profit_data[data_index]);
                    market.Add(p, market_data[data_index]);
                    commit.Add(p, commit_data[data_index]);
                    ++data_index;
                }
                m.SealData();
            }

            m.Maximize(
                Sum(PROD, (p) =>
                    profit[p] * Make[p]));

            m.SubjectTo(
                Sum(PROD, (p) => (1 / rate[p]) * Make[p]) <= avail);

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10000 + 0.5) == 1948285714);
                solver.GetValues(Make, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10000 + 0.5) == 1948285714);
                solver.GetValues(Make, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

        }

        public static void Run()
        {
            var PROD_data = new List<string> { "bands", "coils", "plates" };
            var rate_data = new List<double> { 200, 140, 160 };
            var profit_data = new List<double> { 25, 30, 29 };
            var market_data = new List<double> { 6000, 4000, 3500 };
            var commit_data = new List<double> { 1000, 500, 750 };
            Run(PROD_data, rate_data, profit_data, commit_data, market_data);
        }
    }
}

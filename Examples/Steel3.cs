using System;
using System.Collections.Generic;

using MILP.NET;
using static MILP.NET.Model;

// AMPL model to translate
// From the book: "AMPL: A Modeling Language for Mathematical Programming" 
// http://ampl.com/resources/the-ampl-book/
// steel3.mod (Chapter 1)
/*
set PROD;  # products

param rate {PROD} > 0;     # produced tons per hour
param avail >= 0;          # hours available in week
param profit {PROD};       # profit per ton

param commit {PROD} >= 0;  # lower limit on tons sold in week
param market {PROD} >= 0;  # upper limit on tons sold in week

var Make {p in PROD} >= commit[p], <= market[p]; # tons produced

maximize Total_Profit: sum {p in PROD} profit[p] * Make[p];

# Objective: total profits from all products

subject to Time: sum {p in PROD} (1/rate[p]) * Make[p] <= avail;

# Constraint: total of hours used by all
# products may not exceed hours available

*/

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

            m.Maximize("Total_Profit",
                Sum(PROD, (p) =>
                    profit[p] * Make[p]));

            m.SubjectTo("Time",
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

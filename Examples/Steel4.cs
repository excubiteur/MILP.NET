using System;
using System.Collections.Generic;

using MILP.NET;
using static MILP.NET.Model;

// AMPL model to translate
// From the book: "AMPL: A Modeling Language for Mathematical Programming" 
// http://ampl.com/resources/the-ampl-book/
// steel4.mod (Chapter 1)
/*
set PROD;   # products
set STAGE;  # stages

param rate {PROD,STAGE} > 0; # tons per hour in each stage
param avail {STAGE} >= 0;    # hours available/week in each stage
param profit {PROD};         # profit per ton

param commit {PROD} >= 0;    # lower limit on tons sold in week
param market {PROD} >= 0;    # upper limit on tons sold in week

var Make {p in PROD} >= commit[p], <= market[p]; # tons produced

maximize Total_Profit: sum {p in PROD} profit[p] * Make[p];

# Objective: total profits from all products

subject to Time {s in STAGE}:
sum {p in PROD} (1/rate[p,s]) * Make[p] <= avail[s];

# In each stage: total of hours used by all
# products may not exceed hours available


*/

namespace Examples
{
    class Steel4
    {
        private static void Run(
            IList<string> PROD_data,
            IList<string> STAGE_data,
            IList<IList<double>> rate_data,
            IList<double> avail_data,
            IList<double> profit_data,
            IList<double> commit_data,
            IList<double> market_data
            )
        {
            var m = new Model();

            var PROD = m.CreateSet();
            var STAGE = m.CreateSet();

            var rate = m.CreateParam(PROD,STAGE);
            var avail = m.CreateParam(STAGE);

            var profit = m.CreateParam(PROD);
            var commit = m.CreateParam(PROD);
            var market = m.CreateParam(PROD);

            var Make = m.CreateVar(PROD).
                LowerBound((p) => commit[p]).
                UpperBound((p) => market[p]);

            {
                foreach (var p in PROD_data)
                {
                    PROD.Add(p);
                }

                foreach (var s in STAGE_data)
                {
                    STAGE.Add(s);
                }

                int data_index = 0;
                foreach (var p in PROD_data)
                {
                    int data_index2 = 0;
                    foreach (var s in STAGE_data)
                    {
                        rate.Add(p, s, rate_data[data_index][data_index2++]);
                    }
                    profit.Add(p, profit_data[data_index]);
                    market.Add(p, market_data[data_index]);
                    commit.Add(p, commit_data[data_index]);
                    ++data_index;
                }

                data_index = 0;
                foreach (var s in STAGE_data)
                {
                    avail.Add(s, avail_data[data_index++]);
                }

                m.SealData();
            }

            m.Maximize("Total_Profit",
                Sum(PROD, (p) =>
                    profit[p] * Make[p]));

            m.SubjectTo("Time",STAGE, (s) =>
                Sum(PROD, (p) => (1 / rate[p,s]) * Make[p]) <= avail[s]
                );

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10000 + 0.5) == 1900714286);
                solver.GetValues(Make, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue * 10000 + 0.5) == 1900714286);
                solver.GetValues(Make, (name, value) => {
                    Console.WriteLine(name + " = " + value.ToString());
                });
            }

        }

        public static void Run()
        {
            var PROD_data = new List<string> { "bands", "coils", "plates" };
            var STAGE_data = new List<string> { "reheat", "roll" };
            var profit_data = new List<double> { 25, 30, 29 };
            var market_data = new List<double> { 6000, 4000, 3500 };
            var commit_data = new List<double> { 1000, 500, 750 };
            var avail_data = new List<double> { 35, 40 };
            var rate_data = new List<IList<double>> {
                new List<double> { 200,200},
                new List<double> { 200, 140},
                new List<double> { 200, 160}
            };
            Run(PROD_data, STAGE_data, rate_data, avail_data, profit_data, commit_data, market_data);
        }
    }
}

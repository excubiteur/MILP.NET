using System;
using System.Collections.Generic;
using System.Linq;

using MILP.NET;
using static MILP.NET.Model;


namespace Examples
{
    class Multi
    {
        static void Run(
            IList<string> ORIG_data,
            IList<string> DEST_data,
            IList<string> PROD_data,
            IList<IList<double>> supply_data,
            IList<IList<double>> demand_data,
            double limit_data,
            IList<IList<IList<double>>> cost_data
            )
        {
            var m = new Model();

            var ORIG = m.CreateSet();
            var DEST = m.CreateSet();
            var PROD = m.CreateSet();

            var supply = m.CreateParam(ORIG, PROD);
            var demand = m.CreateParam(DEST, PROD);

            var limit = m.CreateParam(ORIG, DEST);

            var cost = m.CreateParam(ORIG, DEST, PROD);

            var Trans = m.CreateVar(ORIG, DEST, PROD).LowerBound(0);


            //////////////////////////////////////////////////////////
            // Start data

            foreach (var o in ORIG_data)
                ORIG.Add(o);

            foreach (var d in DEST_data)
                DEST.Add(d);

            foreach (var p in PROD_data)
                PROD.Add(p);

            foreach (var (o, data_index) in ORIG_data.Select((s, i) => (s, i)))
            {
                foreach (var (p, data_index2) in PROD_data.Select((s, i) => (s, i)))
                {
                    supply.Add(o, p, supply_data[data_index2][data_index]);
                }
            }

            foreach (var (d, data_index) in DEST_data.Select((s, i) => (s, i)))
            {
                foreach (var (p, data_index2) in PROD_data.Select((s, i) => (s, i)))
                {
                    demand.Add(d, p, demand_data[data_index2][data_index]);
                }
            }

            foreach (var (o, data_index) in ORIG_data.Select((s, i) => (s, i)))
            {
                foreach (var (d, data_index2) in DEST_data.Select((s, i) => (s, i)))
                {
                    foreach (var (p, data_index3) in PROD_data.Select((s, i) => (s, i)))
                    {
                        double value = cost_data[data_index3][data_index][data_index2];
                        cost.Add(o, d, p, value);
                    }
                }
            }

            limit.SetDefault(limit_data);
            m.SealData();
            // End data
            //////////////////////////////////////////////////////////

            m.Minimize("Total_Cost",
                Sum(ORIG, DEST, PROD,
                    (i, j, p) => cost[i, j, p] * Trans[i, j, p])
                );

            m.SubjectTo("Supply", ORIG, PROD, (i, p) =>
                    Sum(DEST, (j) => Trans[i, j, p]) == supply[i, p]
                    );

            m.SubjectTo("Demand", DEST, PROD, (j, p) =>
                    Sum(ORIG, (i) => Trans[i, j, p]) == demand[j, p]
                    );

            m.SubjectTo("Limit", ORIG, DEST, (i, j) =>
                    Sum(PROD, (p) => Trans[i, j, p]) <= limit[i, j]
                    );

            using (var solver = new glpk(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue + 0.5) == 199500);
                solver.GetValues(Trans, (name1, name2, name3, value) =>
                {
                    Console.WriteLine(name1 + "," + name2 + "," + name3 + " = " + value.ToString());
                });
            }

            using (var solver = new lp_solve(m))
            {
                solver.solve();
                Console.WriteLine("objective = " + solver.ObjectiveValue.ToString());
                System.Diagnostics.Debug.Assert((long)(solver.ObjectiveValue + 0.5) == 199500);
                solver.GetValues(Trans, (name1, name2, name3, value) =>
                {
                    Console.WriteLine(name1 + "," + name2 + "," + name3 + " = " + value.ToString());
                });
            }

        }
        
        static public void Run()
        {
            var ORIG_data = new List<string>{ "GARY", "CLEV", "PITT"};
            var DEST_data = new List<string>{ "FRA","DET","LAN","WIN","STL","FRE","LAF" };
            var PROD_data = new List<string>{ "bands", "coils", "plates" };

            var supply_data = new List<IList<double>>{
                new List<double>{ 400, 700,    800 },
                new List<double>{ 800, 1600,   1800 },
                new List<double>{ 200, 300,  300}
            };

            var demand_data = new List<IList<double>>{
                new List<double>{ 300,     300 ,  100   , 75 ,  650,   225 ,  250},
                new List<double>{ 500,     750,   400 ,  250  , 950 ,  850 ,  500},
                new List<double>{ 100 ,    100 ,    0 ,   50 ,  200 ,  100 ,  250}
            };


            var cost_data = new List<IList<IList<double>>>{
                new List<IList<double>>{
                    new List<double>{ 30 ,  10 ,   8 ,  10 ,  11 ,  71 ,   6},
                    new List<double>{ 22 ,   7 ,  10 ,   7 ,  21  , 82 ,  13},
                    new List<double>{ 19  , 11  , 12 ,  10 ,  25 ,  83 ,  15}
                },
                new List<IList<double>>{
                    new List<double>{ 39 ,  14  , 11 ,  14 ,  16 ,  82 ,   8},
                    new List<double>{ 27 ,   9  , 12 ,   9  , 26,   95,   17},
                    new List<double>{ 24 ,  14  , 17 ,  13 ,  28  , 99 ,  20}
                },
                new List<IList<double>>{
                    new List<double>{ 41 ,  15 ,  12 ,  16,   17 ,  86 ,   8},
                    new List<double>{ 29 ,   9  , 13  ,  9 ,  28 ,  99 ,  18},
                    new List<double>{ 26 ,  14  , 17 ,  13 ,  31 , 104 ,  20}
                }
            };

            Run(ORIG_data, DEST_data, PROD_data, supply_data, demand_data, 625, cost_data);
        }        



    }
}

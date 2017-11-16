# MILP.NET
An AMPL-like C# interface to glpk and lp_solve

Trying to to the same thing for C#. See milpcpp repo. Same goals but for C#.

Models will look like

```
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
            
            // Code for loading data here
            
            m.Maximize(
              Sum(PROD, (p) => profit[p] * Make[p]));
                
            m.SubjectTo(STAGE, (s) =>                
                  Sum(PROD, (p) => (1 / rate[p,s]) * Make[p]) <= avail[s]                
                  );            
            
```

Compare with original AMPL model:

```
set PROD;   # product
set STAGE;  # stages
param rate {PROD,STAGE} > 0; # tons per hour in each stage
param avail {STAGE} >= 0;    # hours available/week in each stage
param profit {PROD};         # profit per ton
param commit {PROD} >= 0;    # lower limit on tons sold in week
param market {PROD} >= 0;    # upper limit on tons sold in week

var Make {p in PROD} >= commit[p], <= market[p]; # tons produced

maximize Total_Profit: sum {p in PROD} profit[p] * Make[p];

subject to Time {s in STAGE}:
  sum {p in PROD} (1/rate[p,s]) * Make[p] <= avail[s];

```

using System;
using System.Collections.Generic;
using System.Linq;

namespace MILP.NET
{
    public class Model
    {
        List<Set> _sets = new List<Set>();
        List<Param> _params = new List<Param>();
        List<Var> _vars = new List<Var>();

        internal List<Constraint> _constraints = new List<Constraint>();

        internal Expression _objective;
        internal bool _maximize;

        internal int NumberOfVariables { get; private set; }
        
        Var GetVar(int absoluteIndex)
        {
            int cumulativeSize = 0;
            foreach(var v in _vars)
            {
                cumulativeSize += v.Count;
                if (absoluteIndex < cumulativeSize)
                    return v;
            }
            throw new IndexOutOfRangeException();
        }

        internal double? GetLowerBound(int absoluteIndex)
        {
            return GetVar(absoluteIndex).GetLowerBound(absoluteIndex);
        }

        internal double? GetUpperBound(int absoluteIndex)
        {
            return GetVar(absoluteIndex).GetUpperBound(absoluteIndex);
        }

        public void SealData()
        {
            NumberOfVariables = _vars.Sum((v) => v.Count);
        }

        public Set CreateSet()
        {
            var result = new Set();
            _sets.Add(result);
            return result;
        }

        public Param1 CreateParam(Set index)
        {
            var result = new Param1(index);
            _params.Add(result);
            return result;
        }

        public Param2 CreateParam(Set index1, Set index2)
        {
            var result = new Param2(index1, index2);
            _params.Add(result);
            return result;
        }

        public Param3 CreateParam(Set index1, Set index2, Set index3)

        {
            var result = new Param3(index1, index2, index3);
            _params.Add(result);
            return result;
        }
        public Var1 CreateVar(Set index)
        {
            var result = new Var1(index);
            _vars.Add(result);
            return result;
        }

        public Var2 CreateVar(Set index1, Set index2)
        {
            var result = new Var2(index1, index2);
            _vars.Add(result);
            return result;
        }

        public Var3 CreateVar(Set index1, Set index2, Set index3)
        {
            var result = new Var3(index1, index2, index3);
            _vars.Add(result);
            return result;
        }

        public static Expression Sum(Set index, Func<Index,Expression> sum)
        {
            var result = new Sum();
            for(int i = 0; i < index.Count; ++i)
            {
                result.Add(sum(new Index(i)));
            }
            return result;
        }

        public static Expression Sum(Set index1, Set index2, Func<Index, Index,  Expression> sum)
        {
            var result = new Sum();

            var indices = from i in index1._elements.Select((_s, _i) => _i)
                          from j in index2._elements.Select((_s, _i) => _i)
                          select (i, j);


            foreach (var (i, j) in indices)
            {
                result.Add(sum(new Index(i), new Index(j)));
            }
            return result;
        }

        public static Expression Sum(Set index1, Set index2, Set index3, Func<Index, Index, Index, Expression> sum)
        {
            var result = new Sum();

            var indices = from i in index1._elements.Select((_s, _i) => _i)
                          from j in index2._elements.Select((_s, _i) => _i)
                          from k in index3._elements.Select((_s, _i) => _i)
                          select (i, j, k);

            foreach (var (i, j, k) in indices)
            {
                result.Add(sum(new Index(i), new Index(j), new Index(k)));
            }
            return result;
        }

        public void Maximize(string name, Expression e)
        {
            _maximize = true;
            _objective = e;
        }

        public void Minimize(string name, Expression e)
        {
            _maximize = false;
            _objective = e;
        }

        public void SubjectTo(string name, Constraint c)
        {
            _constraints.Add(c);
        }

        public void SubjectTo(string name, Set index, Func<Index, Constraint> constraint)
        {
            var indices = from i in index._elements.Select((_s, _i) => _i)
                          select i;

            foreach (var i in indices)
            {
                var c = constraint(new Index(i));
                _constraints.Add(c);
            }
        }

        public void SubjectTo(string name, Set index1, Set index2, Func<Index, Index, Constraint> constraint)
        {
            var indices = from i in index1._elements.Select( (_s, _i) => _i)
                          from j in index2._elements.Select(( _s, _i) => _i)
                          select (i, j );

            foreach( var (i, j) in indices)
            {
                var c = constraint(new Index(i), new Index(j));
                _constraints.Add(c);
            }
        
        }


    }
}

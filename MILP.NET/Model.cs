using System;
using System.Collections.Generic;

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
            int cumulativeSize = 0;
            foreach(var v in _vars)
            {
                v._startIndex = cumulativeSize;
                cumulativeSize += v.Count;
            }
            NumberOfVariables = cumulativeSize;
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
            for (int i = 0; i < index1.Count; ++i)
            {
                for (int j = 0; j < index2.Count; ++j)
                {
                    result.Add(sum(new Index(i), new Index(j)));
                }
            }
            return result;
        }

        public void Maximize(Expression e)
        {
            _maximize = true;
            _objective = e;
        }

        public void Minimize(Expression e)
        {
            _maximize = false;
            _objective = e;
        }

        public void SubjectTo(Constraint c)
        {
            _constraints.Add(c);
        }

        public void SubjectTo(Set index, Func<Index, Constraint> constraint)
        {
            int size = index.Count;
            for (int i = 0; i < size; ++i)
            {
                var c = constraint(new Index(i));
                _constraints.Add(c);
            }
        }

    }
}

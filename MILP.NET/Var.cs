using System;

namespace MILP.NET
{
    public abstract class Var
    {
        internal int _startIndex = 0;
        abstract internal int Count { get; }
        abstract internal double? GetLowerBound(int absoluteIndex);
        abstract internal double? GetUpperBound(int absoluteIndex);
    }



    public class Var1 : Var
    {
        internal Func<Index, double> _lowerBound = null;
        internal Func<Index, double> _upperBound = null;

        internal Set _index;
        internal Var1(Set index) { _index = index; }
        internal override int Count { get { return _index.Count; } }
        public Expression this[Index index]
        {
            get
            {
                return new Variable(_startIndex + index._rawIndex);
            }
        }

        public Var1 LowerBound(double value)
        {
            _lowerBound = (i) => value;
            return this;
        }

        public Var1 LowerBound(Func<Index, double> expression)
        {
            _lowerBound = expression;
            return this;
        }

        public Var1 UpperBound(Func<Index, double> expression)
        {
            _upperBound = expression;
            return this;
        }

        internal override double? GetLowerBound(int absoluteIndex)
        {
            if (_lowerBound == null)
                return null;
            return _lowerBound(new Index(absoluteIndex - _startIndex));
        }

        internal override double? GetUpperBound(int absoluteIndex)
        {
            if (_upperBound == null)
                return null;
            return _upperBound(new Index(absoluteIndex - _startIndex));
        }


    }
}

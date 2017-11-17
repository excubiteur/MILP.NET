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

    public class Var2 : Var
    {
        internal Func<Index, Index, double> _lowerBound = null;
        internal Func<Index, Index, double> _upperBound = null;

        internal Set _index1;
        internal Set _index2;

        internal Var2(Set index1, Set index2) { _index1 = index1; _index2 = index2; }
        internal override int Count { get { return _index1.Count * _index2.Count; } }

        public Expression this[Index index1, Index index2]
        {
            get
            {
                return new Variable(_startIndex + index1._rawIndex * _index2.Count + index2._rawIndex);
            }
        }

        public Var2 LowerBound(double value)
        {
            _lowerBound = (i, j) => value;
            return this;
        }

        public Var2 LowerBound(Func<Index, Index, double> expression)
        {
            _lowerBound = expression;
            return this;
        }

        public Var2 UpperBound(Func<Index, Index, double> expression)
        {
            _upperBound = expression;
            return this;
        }

        internal override double? GetLowerBound(int absoluteIndex)
        {
            if (_lowerBound == null)
                return null;
            return _lowerBound(
                new Index((absoluteIndex - _startIndex) / _index2.Count),
                new Index((absoluteIndex - _startIndex) % _index2.Count));
        }

        internal override double? GetUpperBound(int absoluteIndex)
        {
            if (_upperBound == null)
                return null;
            return _upperBound(
                new Index((absoluteIndex - _startIndex) / _index2.Count),
                new Index((absoluteIndex - _startIndex) % _index2.Count));
        }


    }
}

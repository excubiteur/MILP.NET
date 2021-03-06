﻿using System;
using System.Collections.Generic;


namespace MILP.NET
{
    public class Param
    {
        protected double _default;
        public void SetDefault(double d) { _default = d; }
    }

    public class Param1 : Param
    {
        Set _index;
        List<double> _values = new List<double>();

        internal Param1(Set index) { _index = index; }
        public double this[Index index]
        {
            get
            {
                if (_values.Count <= 0)
                    return _default;
                return _values[index._rawIndex];
            }
        }

        public void Add(string name, double value)
        {
            if (_values.Count <= 0)
                _values.Capacity = _index.Count;
            _values.Insert(_index.IndexOf(name), value);

        }

        public Param1 Data(IList<double> vals)
        {
            if (vals.Count != _index.Count)
                throw new IndexOutOfRangeException();
            if (_values.Count <= 0)
                _values.Capacity = _index.Count;
            _values.AddRange(vals);
            return this;
        }

    }

    public class Param2 : Param
    {
        Set _index1;
        Set _index2;
        List<double> _values = new List<double>();

        internal Param2(Set index1, Set index2)
        {
            _index1 = index1;
            _index2 = index2;
        }

        public double this[Index index1, Index index2]
        {
            get
            {
                if (_values.Count <= 0)
                    return _default;
                return _values[index1._rawIndex*_index2.Count + index2._rawIndex];
            }
        }

        public void Add(string name1, string name2, double value)
        {
            if (_values.Count <= 0)
                _values.Capacity = _index1.Count * _index2.Count;
            _values.Insert(_index1.IndexOf(name1) * _index2.Count + _index2.IndexOf(name2), value);

        }

        public Param2 Data(double[,] vals)
        {
            if (vals.GetLength(0) != _index1.Count)
                throw new IndexOutOfRangeException();
            if (vals.GetLength(1) != _index2.Count)
                throw new IndexOutOfRangeException();

            if (_values.Count <= 0)
                _values.Capacity = _index1.Count * _index2.Count;

            foreach (var v in vals)
                _values.Add(v);

            return this;
        }
    }


    public class Param3 : Param
    {
        Set _index1;
        Set _index2;
        Set _index3;
        List<double> _values = new List<double>();

        internal Param3(Set index1, Set index2, Set index3)
        {
            _index1 = index1;
            _index2 = index2;
            _index3 = index3;
        }

        public double this[Index index1, Index index2, Index index3]
        {
            get
            {
                if (_values.Count <= 0)
                    return _default;
                return _values[index1._rawIndex * _index2.Count * _index3.Count 
                    + index2._rawIndex * _index3.Count 
                    + index3._rawIndex];
            }
        }

        public void Add(string name1, string name2, string name3, double value)
        {
            if (_values.Count <= 0)
                _values.Capacity = _index1.Count * _index2.Count;
            _values.Insert(_index1.IndexOf(name1) * _index2.Count * _index3.Count
                + _index2.IndexOf(name2) * _index3.Count
                + _index3.IndexOf(name3), 
                value);

        }

        public Param3 Data(double[,,] vals)
        {
            if (vals.GetLength(0) != _index1.Count)
                throw new IndexOutOfRangeException();
            if (vals.GetLength(1) != _index2.Count)
                throw new IndexOutOfRangeException();
            if (vals.GetLength(2) != _index3.Count)
                throw new IndexOutOfRangeException();

            if (_values.Count <= 0)
                _values.Capacity = _index1.Count * _index2.Count * _index3.Count;

            foreach (var v in vals)
                _values.Add(v);

            return this;
        }
    }
}

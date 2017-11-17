using System;
using System.Collections.Generic;

namespace MILP.NET
{
    public class Expression
    {
        static Expression Multiply(Expression e, double value)
        {
            switch (e)
            {
                case Variable v:
                    return new Term(v._index, value);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Expression operator *(Expression e, double value)
        {
            return Multiply(e, value);
        }
        public static Expression operator *(double value, Expression e)
        {
            return Multiply(e, value);
        }

        static Constraint GreaterEqual(Expression e, double value)
        {
            switch (e)
            {
                case Variable v:
                    {
                        var sum = new Sum();
                        sum.Add(v);
                        var constraint = new Constraint();
                        constraint._expression = sum;
                        constraint._lowerBound = value;
                        return constraint;
                    }
                case Sum s:
                    {
                        var constraint = new Constraint();
                        constraint._expression = s;
                        constraint._lowerBound = value;
                        return constraint;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        static Constraint LessEqual(Expression e, double value)
        {
            switch (e)
            {
                case Sum sum:
                    {
                        var constraint = new Constraint();
                        constraint._expression = sum;
                        constraint._upperBound = value;
                        return constraint;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static Constraint operator <=(double value, Expression e)
        {
            return GreaterEqual(e, value);
        }

        public static Constraint operator >=(Expression e, double value)
        {
            return GreaterEqual(e, value);
        }

        public static Constraint operator >=(double value, Expression e)
        {
            return LessEqual(e, value);
        }

        public static Constraint operator <=(Expression e, double value)
        {
            return LessEqual(e, value);
        }

    }

    internal class Term : Expression
    {
        internal int _variableIndex;
        internal double _coefficient;
        internal Term(int varIndex, double coefficient)
        {
            _variableIndex = varIndex;
            _coefficient = coefficient;
        }
    }

    internal class Variable : Expression
    {
        internal int _index;
        internal Variable(int index) { _index = index; }
    }

    internal class Sum : Expression
    {
        internal Dictionary<int, Term> _terms = new Dictionary<int, Term>();
        internal double _constant = 0;

        internal void Add(Expression e)
        {
            switch (e)
            {
                case Term term:
                    {
                        if (_terms.ContainsKey(term._variableIndex))
                        {
                            _terms[term._variableIndex]._coefficient += term._coefficient;
                        }
                        else
                        {
                            _terms[term._variableIndex] = term;
                        }
                    }
                    break;
                case Variable v:
                    {
                        if (_terms.ContainsKey(v._index))
                        {
                            _terms[v._index]._coefficient += 1;
                        }
                        else
                        {
                            _terms[v._index] = new Term(v._index, 1);
                        }

                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Constraint
    {
        internal Sum _expression;
        internal double? _lowerBound;
        internal double? _upperBound;


        static Constraint GreaterEqual(Constraint e, double value)
        {
            var result = new Constraint();
            result._expression = e._expression;
            if (e._lowerBound.HasValue)
                throw new Exception();
            result._lowerBound = value;
            if (!e._upperBound.HasValue)
                throw new Exception();
            result._upperBound = e._upperBound;
            return result;
        }

        static Constraint LessEqual(Constraint e, double value)
        {
            var result = new Constraint();
            result._expression = e._expression;
            if (e._upperBound.HasValue)
                throw new Exception();
            result._upperBound = value;
            if (!e._lowerBound.HasValue)
                throw new Exception();
            result._lowerBound = e._lowerBound;
            return result;
        }

        public static Constraint operator >=(double value, Constraint e)
        {
            return LessEqual(e, value);
        }

        public static Constraint operator <=(Constraint e, double value)
        {
            return LessEqual(e, value);
        }

        public static Constraint operator >=(Constraint e, double value)
        {
            return GreaterEqual(e, value);
        }

        public static Constraint operator <=(double value, Constraint e)
        {
            return GreaterEqual(e, value);
        }

    }
}

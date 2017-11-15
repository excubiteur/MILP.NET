﻿using System;
using System.Runtime.InteropServices;

namespace MILP.NET
{
    public class lp_solve: IDisposable
    {
        const string lp_solve_libname = "lpsolve55.dll";

        [DllImport(lp_solve_libname)]
        static extern IntPtr make_lp(int rows, int columns);

        [DllImport(lp_solve_libname)]
        static extern void delete_lp(IntPtr lp);

        const int FALSE = 0;
        const int TRUE = 0;
        [DllImport(lp_solve_libname)]
        static extern byte set_add_rowmode(IntPtr lp, byte turnon);

        [DllImport(lp_solve_libname)]
        static extern byte set_unbounded(IntPtr lp, int column);

        const int LE = 1;
        const int GE = 2;
        [DllImport(lp_solve_libname)]
        static extern byte add_constraintex(IntPtr lp, int count, double[] row, int[] colno, int constr_type, double rh);

        [DllImport(lp_solve_libname)]
        static extern byte set_obj_fnex(IntPtr lp, int count, double[] row, int[] colno);

        [DllImport(lp_solve_libname)]
        static extern void set_minim(IntPtr lp);

        [DllImport(lp_solve_libname)]
        static extern void set_maxim(IntPtr lp);

        [DllImport(lp_solve_libname)]
        static extern int solve(IntPtr lp);

        [DllImport(lp_solve_libname)]
        static extern double get_objective(IntPtr lp);

        const int NEUTRAL   = 0;
        const int CRITICAL  = 1;
        const int SEVERE    = 2;
        const int IMPORTANT = 3;
        const int NORMAL    = 4;
        const int DETAILED  = 5;
        const int FULL      = 6;
        [DllImport(lp_solve_libname)]
        static extern void set_verbose(IntPtr lp, int verbose);

        [DllImport(lp_solve_libname)]
        public static extern byte get_variables(IntPtr lp, double[] var);

        Model _model;
        IntPtr _lp;

        public lp_solve(Model m)
        {
            _model = m;
        }

        public void solve()
        {
            int ncols = _model.NumberOfVariables;

            _lp = make_lp(0, ncols);
            set_add_rowmode(_lp, TRUE);

            for (int i = 1; i <= ncols; ++i)
            {
                set_unbounded(_lp, i);
            }

            foreach (var c in _model._constraints)
            {
                var indices = new int[c._expression._terms.Count];
                var values = new double[c._expression._terms.Count];

                int count = 0;
                foreach (var t in c._expression._terms)
                {
                    indices[count] = t.Key + 1;
                    values[count] = t.Value._coefficient;
                    ++count;
                }

                if (c._lowerBound.HasValue)
                {
                    var lower = c._lowerBound.Value - c._expression._constant;
                    add_constraintex(_lp, c._expression._terms.Count, values, indices, GE, lower);
                }
                if(c._upperBound.HasValue)
                {
                    var upper = c._upperBound.Value - c._expression._constant;
                    add_constraintex(_lp, c._expression._terms.Count, values, indices, LE, upper);
                }
            }

            set_add_rowmode(_lp, FALSE);

            switch (_model._objective)
            {
                case Sum s:
                    {
                        var indices = new int[s._terms.Count];
                        var values = new double[s._terms.Count];

                        int count = 0;
                        foreach (var t in s._terms)
                        {
                            indices[count] = t.Key + 1;
                            values[count] = t.Value._coefficient;
                            ++count;
                        }
                        set_obj_fnex(_lp, s._terms.Count, values, indices);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (_model._maximize)
                set_maxim(_lp);
            else
                set_minim(_lp);

            set_verbose(_lp, CRITICAL);
            solve(_lp);
        }


        public double ObjectiveValue
        {
            get
            {
                return get_objective(_lp);
            }
        }

        public void GetValues(Var1 variables, Action<string, double> iterator)
        {
            var values = new double[_model.NumberOfVariables];
            get_variables(_lp, values);

            int index = 0;
            foreach (var i in variables._index._elements)
            {
                var value = values[ variables._startIndex + index];
                iterator(i, value);
                ++index;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (_lp != IntPtr.Zero)
                    delete_lp(_lp);
            }
        }

        ~lp_solve()
        {
           Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

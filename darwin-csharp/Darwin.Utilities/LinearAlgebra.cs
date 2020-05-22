﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin.Utilities
{
    public static class LinearAlgebra
    {
		private static void swapf(int x1, int x2, int y1, int y2, ref float[,] array)
		{
			float temp = array[x1, x2];
			array[x1, x2] = array[y1, y2];
			array[y1, y2] = temp;
		}

		//*******************************************************************
		// gaussj()
		//
		// Based on the function presented in Numerical Recipes in C, pages 39-40.
		//
		// Linear equation solution by Gauss-Jordan elmination.  a[0..n-1][0..n-1]
		// is the input matrix.  b[0..n-1][0..m-1] is the input containing the m
		// right-hand side vectors.  On output, a is replaced by its matrix inverse,
		// and b is replaced by the corresponding set of solution vectors.
		//
		public static bool GaussJ(ref float[,] a, int n, ref float[,] b, int m)
		{
			int i, icol = 0, irow = 0, j, k, l, ll;
			float big, dum, pivinv;

			// These arrays are used for bookkeeping on the pivoting
			int[] indxc = new int[n];
			int[] indxr = new int[n];
			int[] ipiv = new int[n];

			for (j = 0; j < n; j++)
				ipiv[j] = 0;

			// Main loop over the columns to be reduced
			for (i = 0; i < n; i++)
			{
				big = 0.0f;

				// Outer loop of the search for a pivot element
				for (j = 0; j < n; j++)
					if (ipiv[j] != 1)
						for (k = 0; k < n; k++)
						{
							if (ipiv[k] == 0)
							{
								if (Math.Abs(a[j, k]) >= big)
								{
									big = (float)Math.Abs(a[j, k]);
									irow = j;
									icol = k;
								}
							}
							else if (ipiv[k] > 1)
							{
								Trace.WriteLine("gaussj: Singular Matrix-1");
								return false;
							}
						}

				++(ipiv[icol]);

				// We now have the pivot element, so we interchange rows,
				// if needed, to put the pivot element on the diagonal.
				// The columns are not physically interchanged, only
				// relabeled: indxc[i], the column of the ith pivot element,
				// is the ith column that is reduced, while indxr[i] is the row
				// in which that pivot element was originally located.  If indxr[i]
				// != indxc[i] there is an implied column interchange.  With
				// this form of bookkeeping, the solution b's will end up in
				// the correct order, and the inverse matrix will be scrambled
				// by columns.
				if (irow != icol)
				{
					for (l = 0; l < n; l++)
						swapf(irow, l, icol, l, ref a);

					for (l = 0; l < m; l++)
						swapf(irow, 1, icol, 1, ref b);
				}

				// We are now ready to divide the pivot row by the pivot element,
				// located at irow and icol
				indxr[i] = irow;
				indxc[i] = icol;

				if (a[icol, icol] == 0.0)
				{
					Trace.WriteLine("gaussj: Singular Matrix-2");
					return false;
				}

				pivinv = 1.0f / a[icol, icol];
				a[icol, icol] = 1.0f;

				for (l = 0; l < n; l++)
					a[icol, l] *= pivinv;

				for (l = 0; l < m; l++)
					b[icol, l] *= pivinv;

				// Now, we reduce the rows...
				for (ll = 0; ll < n; ll++)
					// ... except for the pivot one, of course.
					if (ll != icol)
					{
						dum = a[ll, icol];
						a[ll, icol] = 0.0f;

						for (l = 0; l < n; l++)
							a[ll, l] -= a[icol, l] * dum;

						for (l = 0; l < m; l++)
							b[ll, l] -= b[icol, l] * dum;
					}
			}

			// This is the end of the main loop over columns of reduction.
			// It only remains to unscramble the solution in view of the
			// column interchanges.  We do this by interchanging pairs
			// of columns in the reverse order that the permutation was
			// built up.
			for (l = n - 1; l >= 0; l--)
			{
				if (indxr[l] != indxc[l])
					for (k = 0; k < n; k++)
						swapf(k, indxr[l], k, indxc[l], ref a);
			}

			return true;
		}
	}
}

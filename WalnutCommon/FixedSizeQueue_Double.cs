using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace WalnutCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to implement a fixed size queue for doubles 
    /// 
    /// </summary>
    public class FixedSizeQueue_Double : FixedSizeQueue_Generic<double>
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public FixedSizeQueue_Double() : base()
        { }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueSizeIn">the starting queue size</param>
        public FixedSizeQueue_Double(int queueSizeIn) : base(queueSizeIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the average
        /// </summary>
        /// <returns>the average of all the values in the queue</returns>
        public double Average()
        {
            if (this.Count == 0) return 0;
            // loop through
            double sumVal = 0;
            // just sum them up
            foreach (double doubleVal in this)
            {
                sumVal += doubleVal;
            }
            return sumVal / this.Count;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates a slope of the queue of data. Assumes equally spaced points!
        /// This is straight out of the Leibovici comment on the link below
        /// 
        /// https://math.stackexchange.com/questions/582029/regression-with-equally-spaced-set
        /// </summary>
        public double SlopeOfData()
        {
            double s1 = 0;
            double s2 = 0;
            double s3 = 0;
            double s4 = 0;
            double n;

            // From the normal equations, you have b = (S1 * S2 - N * S3) / (S2 * S2 - N * S4) where
            // N is the number of data points,
            // S1 the sum of the Y(i),
            // S2 the sum of the X(i),
            // S3 the sum of the X(i) * Y(i) and
            // S4 the sum of the X(i) * X(i). 

            // using the fact that the X(i) are in arithmetic progression we could pre-compute
            // the value of sums S2 and S4 and pass them in (assuming the size of the list does
            // not change between calls) but it just adds complexity to a simple algorythm and
            // it is not that much extra work

            n = (double)this.Count;
            int i = 0;
            foreach(double dVal in this)
            {
                s1 += dVal;
                s2 += i;
                s3 += (i * dVal);
                s4 += (i * i);
                i++;
            }
            double numerator = (s1 * s2) - (n * s3);
            double denominator = (s2 * s2) - (n * s4);
            return numerator / denominator;
        }
    }
}

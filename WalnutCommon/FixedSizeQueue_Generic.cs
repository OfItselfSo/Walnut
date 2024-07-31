using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

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
    /// A class to implement a generic fixed size queue
    /// 
    /// Credit: https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enqueues
    /// </summary>
    public class FixedSizeQueue_Generic<T> : IEnumerable<T>
    {
        private const int DEFAULT_QUEUE_SIZE = 2;
        readonly ConcurrentQueue<T> fixedQueue = new ConcurrentQueue<T>();
        private object lockObject = new object();

        public int QueueSize { get; set; } = DEFAULT_QUEUE_SIZE;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public FixedSizeQueue_Generic()
        {}

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueSizeIn">the starting queue size</param>
        public FixedSizeQueue_Generic(int queueSizeIn)
        {
            QueueSize = queueSizeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the count
        /// </summary>
        public int Count
        {
            get
            {
                return fixedQueue.Count;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the Queue is full
        /// </summary>
        public bool IsFull()
        {
            if (fixedQueue.Count != QueueSize) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Flushes the queue except the specified number of newest entries
        /// </summary>
        /// <param name="numValuesToPreserve">the number of values to preserve</param>
        public void ClearOld(int numValuesToPreserve)
        {
            lock (lockObject)
            {
                T overflow;
                while (fixedQueue.Count > numValuesToPreserve && fixedQueue.TryDequeue(out overflow)) ;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Enqueues an object
        /// 
        /// </summary>
        public void Enqueue(T obj)
        {
            fixedQueue.Enqueue(obj);
            lock (lockObject)
            {
                T overflow;
                while (fixedQueue.Count > QueueSize && fixedQueue.TryDequeue(out overflow)) ;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the generic enumerator
        /// 
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            // note according to the docs 
            // The enumeration represents a moment-in-time snapshot of the contents 
            // of the queue. It does not reflect any updates to the collection after 
            // GetEnumerator was called. The enumerator is safe to use concurrently 
            // with reads from and writes to the queue.
            return ((IEnumerable<T>)fixedQueue).GetEnumerator();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the enumerator
        /// 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // note according to the docs 
            // The enumeration represents a moment-in-time snapshot of the contents 
            // of the queue. It does not reflect any updates to the collection after 
            // GetEnumerator was called. The enumerator is safe to use concurrently 
            // with reads from and writes to the queue.
            return ((IEnumerable)fixedQueue).GetEnumerator();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dumps the queue to the console
        /// 
        /// </summary>
        public virtual void DumpQueueToConsole()
        {
            foreach(T tObj in this)
            {
                Console.WriteLine(tObj.ToString());
            }
        }
    }
}

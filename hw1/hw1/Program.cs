﻿using System;
using System.Diagnostics;

namespace Homework
{
    public class Program
    {
        public static void Main(string[] args) 
        {
            ILongProgressByTime a = new LongProgressByTime();
            new Thread
                (
                    () =>
                    {
                        Console.WriteLine("A Start: "+(a.Start(2000)).ToString());
                        Thread.Sleep(1000);
                        Console.WriteLine("A TrySet0: "+(a.TrySet0()).ToString());
                        Thread.Sleep(500);
                        Console.WriteLine("A Start: "+(a.Start(1000)).ToString() +" Now: "+Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("A Progress: "+(a.GetProgress()).ToString() +" Now: "+Environment.TickCount64);
                        Thread.Sleep(1003);
                        Console.WriteLine("A TrySet0: "+(a.TrySet0()).ToString());
                    }
                ).Start();

            new Thread
                (
                    () =>
                    {
                        Console.WriteLine("B Start: "+(a.Start(2000)).ToString());
                        Thread.Sleep(1500);
                        Console.WriteLine("B Start: " +(a.Start(1000)).ToString() + " Now: " + Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("B Progress: " +(a.GetProgress()).ToString() + " Now: " + Environment.TickCount64);
                    }
                ).Start();
        }
    }

    public interface ILongProgressByTime
    {
        /// <summary>
        /// 尝试加载下一次进度条，needTime指再次加载进度条所需时间，单位毫秒
        /// 如果之前进度条处于就绪态，则将进度开始下一次加载，返回true
        /// 如果之前进度条不处于就绪态，返回false
        /// </summary>
        public bool Start(long needTime);

        /// <summary>
        /// 使未完成的进度条清零并终止变为就绪态，返回值代表是否成功终止
        /// </summary>
        public bool TrySet0();

        /// <summary>
        /// 使进度条强制清零并终止变为就绪态
        /// </summary>
        public void Set0();

        /// <summary>
        ///     ElapsedTime指其中已过去的时间，NeedTime指当前Progress完成所需时间，单位毫秒
        /// </summary>
        public (long ElapsedTime, long NeedTime) GetProgress();
    }

    public class LongProgressByTime: ILongProgressByTime
    {
        // 根据时间推算Start后完成多少进度的进度条（long）。

        // 只允许修改LongProgressByTime类中的代码
        // 要求实现ILongProgressByTime中的要求
        // 可利用Environment.TickCount64获取当前时间（单位ms）

        //挑战：利用原子操作
        //long.MaxValue非常久
        private long needTime = 0;
        private long finishedTime = 0;
        private long startTime = 0;
        private bool readystate = true;
        private static readonly object lockobject  = new();

        public bool Start(long x)
        {
            lock(lockobject){
                if (readystate)
                {
                    readystate = false;
                    startTime = Environment.TickCount64;
                    needTime = x;
                    return true;
                }
                else
                {
                    return false;
                } 
            }
        }
        public bool TrySet0()
        {
            lock (lockobject)
            {
                finishedTime = Environment.TickCount64 - startTime;
                if (finishedTime < needTime)
                {
                    readystate = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void Set0()
        {
            lock (lockobject)
            {
                readystate = true;
            }
        }
        public (long ElapsedTime, long NeedTime) GetProgress()
        {
            lock (lockobject)
            {
                finishedTime = Environment.TickCount64 - startTime;
                if (readystate == false)
                {
                    return (finishedTime, needTime);
                }
                else
                {
                    return (0, needTime);
                }
            }
        }

    }

/*输出示例（仅供参考）：
 * A Start: False
B Start: True
A TrySet0: True
B Start: True Now: 14536562
A Start: False Now: 14536578
B Progress: (516, 1000) Now: 14537078
A Progress: (516, 1000) Now: 14537078
A TrySet0: False
*/
}
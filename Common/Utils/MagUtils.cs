using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.NetModules;

namespace ItemMagnetPro
{
    public static class Num
    {
        public static bool Within(this int i, int min, int max)
        {
            return i >= min && i <= max;
        }
    }
    public class EnumNum<T> where T : Enum
    {
        public enum Limit
        {
            Cap,
            Cycle,
            First,
            Last,
            Bounce
        }

        public T EType;
        public Limit Limiting = Limit.Cycle;
        private int limit;
        public int Index
        {
            get
            {
                return limit;
            }
            set
            {
                limit = Math.Clamp(value, 0, Count);
            }
        }
        public T Value
        {
            get
            {
                return (T)Enum.GetValues(typeof(T)).GetValue(Index);
            }
        }
        public int Count
        {
            get
            {
                return Enum.GetValues(typeof(T)).Length;
            }
        }
        public T[] Values
        {
            get
            {
                return (T[])Enum.GetValues(typeof(T));
            }
        }
        public EnumNum(int index = 0, Limit limiting = Limit.Cycle)
        {
            Index = index;
            Limiting = limiting;
        }
        public EnumNum(T type, int index = 0, Limit limiting = Limit.Cycle)
        {
            EType = type;
            Index = index;
            Limiting = limiting;
        }

        private static int changeIndex(int index, int change, int count, Limit limiting)
        {
            int result = index + change;
            bool high = result >= count, low = result < 0;
            if (!high && !low)
            {
                return result;
            }

            if (limiting == Limit.Cap)
            {
                if (high)
                {
                    return count - 1;
                }
                return 0;
            }
            if (limiting == Limit.Cycle)
            {
                if (high)
                {
                    return result % count;
                }
                return count - 1 + result % count;
            }
            if (limiting == Limit.First)
            {
                return 0;
            }
            if (limiting == Limit.Last)
            {
                return count - 1;
            }
            if (limiting == Limit.Bounce)
            {
                change %= ((count - 1) * 2);
                int dir = high ? 1 : -1;
                result = index;
                for (int i = 0; i < change; i++, result += dir)
                {
                    if (result == count - 1 || result == 0)
                    {
                        dir *= -1;
                    }
                }
                return result;
            }
            return result;
        }

        public static EnumNum<T> operator +(EnumNum<T> num, int addition)
        {
            num.Index = changeIndex(num.Index, addition, num.Count, num.Limiting);
            return num;
        }
        public static EnumNum<T> operator -(EnumNum<T> num, int subtraction)
        {
            num.Index = changeIndex(num.Index, subtraction, num.Count, num.Limiting);
            return num;
        }
        public static EnumNum<T> operator *(EnumNum<T> num, int multiply)
        {
            num.Index = changeIndex(num.Index, num.Index * multiply, num.Count, num.Limiting);
            return num;
        }
        public static EnumNum<T> operator /(EnumNum<T> num, int divide)
        {
            num.Index = changeIndex(num.Index, num.Index / divide, num.Count, num.Limiting);
            return num;
        }
        public static EnumNum<T> operator %(EnumNum<T> num, int module)
        {
            num.Index = changeIndex(num.Index, num.Index % module, num.Count, num.Limiting);
            return num;
        }
        public static bool operator ==(EnumNum<T> num, T @enum)
        {
            if (!num.Values.Contains(@enum))
            {
                return false;
            }
            if (num.Values.ToList().IndexOf(@enum) == num.Index)
            {
                return true;
            }
            return false;
        }
        public static bool operator !=(EnumNum<T> num, T @enum)
        {
            if (!num.Values.Contains(@enum))
            {
                return true;
            }
            if (num.Values.ToList().IndexOf(@enum) == num.Index)
            {
                return false;
            }
            return true;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public int ToInt()
        {
            return Index;
        }
    }
}

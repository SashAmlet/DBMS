using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class Constants
    {
        public static string BasePath = @""; // C:\Users\ostre\OneDrive\Books\4th_course\IT\LAB1-DBMS\Storage
        public const string ApiUrl = "https://localhost:7133/";
        public enum DataType
        {
            Integer,
            Real,
            Char,
            String,
            Time,
            TimeLvl,
        }
    }
}

using System;
using UtilRepo;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            object obj = null;
            DateTime? dt = null;
            Student stu = null;
            Console.WriteLine(obj.IsNull());
            Console.WriteLine(dt.IsNull());
            Console.WriteLine(stu.IsNull());
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
    public class Student
    { 
    
    }
}

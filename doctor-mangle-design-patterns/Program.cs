using doctor_mangle_data.controllers;
using System;

namespace doctor_mangle_design_patterns
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var PC = ProgramControl.GetProgramControl();
            if (PC != null)
            {
                Console.WriteLine("successfully made PC");
            }
            Console.ReadLine();
        }
    }
}

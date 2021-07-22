using System;

namespace doctor_mangle_console_app
{
    public class StaticConsoleHelper
    {
        public static void TalkPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey(true);
        }

        public static int CheckInput(int low, int high)
        {
            string strInput = "";
            int input = 0;
            bool halt = true;
            bool formatError = false;
            while (halt)
            {
                formatError = false;
                strInput = Console.ReadLine();
                if (strInput == null)
                {
                    Console.WriteLine("Please choose a number " + low + "-" + high);
                }
                else
                {
                    try
                    {
                        input = Convert.ToInt32(strInput);
                    }
                    catch (System.FormatException)
                    {
                        Console.WriteLine("Please choose a number " + low + "-" + high);
                        formatError = true;
                    }
                    if ((input < low || input > high) && formatError == false)
                    {
                        Console.WriteLine("Please choose a number " + low + "-" + high);
                    }
                    else if (formatError == false)
                    {
                        halt = false;
                    }
                }


            }

            return input;
        }
    }
}

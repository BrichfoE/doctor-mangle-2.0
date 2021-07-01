using System;

namespace doctor_mangle_data.controllers
{
    // Singleton pattern - using this to ensure that the overall game management class 
    //     only has one instance which is universally accessible
    public sealed class ProgramControl
    {
        private static readonly ProgramControl instance = new ProgramControl();

        private ProgramControl()
        {

        }

        public static ProgramControl GetProgramControl() => instance;
        
    }
}

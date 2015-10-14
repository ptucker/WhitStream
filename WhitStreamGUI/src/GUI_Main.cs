using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
//using WhitStream;
//using WhitStream.Data;
//using WhitStream.QueryEngine;
//using WhitStream.QueryEngine.QueryOperators;
//using WhitStream.QueryEngine.Scheduler;
//using WhitStream.Utility;

namespace WhitStreamGUI
{
    static class GUI_Main
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OpeningScreen());
        }
    }
}
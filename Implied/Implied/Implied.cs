using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Implied
{
    static class Implied
    {
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new FormImplied());
        }
    }
}

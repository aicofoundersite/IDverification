Imports System.Windows.Forms
Imports CrossSetaDeduplicator.Forms

Module Program
    <STAThread()>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainDashboard())
    End Sub
End Module

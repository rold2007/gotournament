namespace GoTournament
   {
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;

   public class Program
      {
      static void Main(string[] args)
         {
         ProcessStartInfo processStartInfo = new ProcessStartInfo();

         processStartInfo.FileName = @"d:\GnuGo\gnugo-3.8\gnugo.exe";
         processStartInfo.Arguments = @"--mode gtp";
         processStartInfo.RedirectStandardInput = true;
         processStartInfo.RedirectStandardOutput = true;
         processStartInfo.UseShellExecute = false;

         using (Process process = Process.Start(processStartInfo))
            {
            Console.WriteLine(process.Id.ToString());

            process.OutputDataReceived += Process_OutputDataReceived;

            process.BeginOutputReadLine();

            process.StandardInput.WriteLine("showboard");
            process.StandardInput.WriteLine("quit");

            process.WaitForExit();
            }
         }

      private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
         {
         string receivedData;

         receivedData = e.Data;

         Console.WriteLine(receivedData);
         }
      }
   }

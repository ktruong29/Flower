/*******************************************************************************
 *Author: Kien Truong
 *Program: Flower
 ******************************************************************************/

using System;
using System.Windows.Forms;  //Needed for "Application" on next to last line of Main
public class FlowerMain
{
  static void Main(string[] args)
  {
    Flower flowerApp = new Flower();
    Application.Run(flowerApp);
    System.Console.WriteLine("Main method will now shutdown.");
  }//End of Main
}//End of FlowerMain

using System;
using Assessment2.Model;
using Assessment2.Model.Enums;
using Assessment2.Repository;
using Assessment2.Service;
using Assessment2.View;

namespace Assessment2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Boiler Controller";
            Console.OutputEncoding = System.Text.Encoding.UTF8;


            // Initialize layers
            var logRepository = new LogRepository();
            var boilerService = new BoilerService(logRepository);
            var boilerView = new BoilerView(boilerService);

            // Display initial screen
            boilerView.DisplayInitialScreen();
            Console.WriteLine("Initializing Boiler Controller...\n");

            // Start application
            boilerService.Init();

            bool exitRequested = false;
            while (!exitRequested)
            {
                try
                {
                    boilerView.ClearMenuArea();
                    boilerView.DisplayMenu();
                    string? input = boilerView.GetUserInput();
                    Console.WriteLine();

                    switch (input)
                    {
                        case "1":
                            boilerService.Start();
                            PauseScreen();
                            break;
                        case "2":
                            boilerService.Stop();
                            PauseScreen();
                            break;
                        case "3":
                            boilerService.SimulateError();
                            PauseScreen();
                            break;
                        case "4":
                            boilerService.ToggleInterlock();
                            PauseScreen();
                            break;
                        case "5":
                            boilerService.ResetLockout();
                            PauseScreen();
                            break;
                        case "6":
                            boilerView.DisplayEventLog(boilerService.GetLog());
                            boilerView.DisplayInitialScreen();
                            PauseScreen();
                            break;
                        case "7":
                            exitRequested = true;
                            Console.Clear();
                            Console.WriteLine("\n✓ Exiting application...");
                            System.Threading.Thread.Sleep(1000);
                            break;
                        default:
                            Console.WriteLine("✗ Invalid selection. Enter a number between 1 and 7.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Unhandled error: {ex.Message}");
                }
            }
        }
        public static void PauseScreen()
        {
            Console.WriteLine("Enter any key to continue.");
            Console.ReadKey();
        }
    }
}

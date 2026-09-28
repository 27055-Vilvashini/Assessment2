using System;
using Assessment2.Service;

namespace Assessment2.View
{
    internal class BoilerView
    {
        private readonly BoilerService _boilerService;
        private const int MenuStartRow = 5;

        public BoilerView(BoilerService boilerService)
        {
            _boilerService = boilerService ?? throw new ArgumentNullException(nameof(boilerService));
        }

        public void DisplayInitialScreen()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(@"
                            ╔════════════════════════════════════════════╗
                            ║      BOILER CONTROLLER SYSTEM v1.0         ║
                            ║     Industrial Boiler Startup Manager      ║
                            ╚════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        public void DisplayMenu()
        {
            Console.SetCursorPosition(0, MenuStartRow);
            Console.Write(@"
                            ┌────────────────────────────────────────────┐
                            │      BOILER CONTROLLER MENU                │
                            ├────────────────────────────────────────────┤
                            │  1. Start Boiler Sequence                  │
                            │  2. Stop Boiler Sequence                   │
                            │  3. Simulate Boiler Error (Operational)    │
                            │  4. Toggle Run Interlock Switch            │
                            │  5. Reset Lockout                          │
                            │  6. View Event Log                         │
                            │  7. Exit Application                       │
                            └────────────────────────────────────────────┘
                            Select option [1-7]: ");
        }

        public string GetUserInput()
        {
            return Console.ReadLine()?.Trim() ?? "";
        }

        public void DisplayEventLog(string[] logEntries)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(@"
                            ╔════════════════════════════════════════════╗
                            ║             EVENT LOG VIEWER               ║
                            ╚════════════════════════════════════════════╝");
            Console.WriteLine();

            if (logEntries.Length == 0)
            {
                Console.WriteLine("No log entries found.");
            }
            else
            {
                Console.WriteLine("Event Log Entries:");
                Console.WriteLine("──────────────────────────────────────────────");
                foreach (string entry in logEntries)
                {
                    Console.WriteLine(entry);
                }
                Console.WriteLine("──────────────────────────────────────────────");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        public void ClearMenuArea()
        {
            for (int i = MenuStartRow; i < Console.BufferHeight - 2; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.WriteLine(new string(' ', Console.BufferWidth));
            }
        }
    }
}
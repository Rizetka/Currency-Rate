namespace Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("Correct Exit: Ctrl + C ==> ex ==> Enter");
            Console.WriteLine("=======================================");

            Server server = new Server();

            server.Run();

            while (!server.isCloseReaquested)
            {
                server.handleInput();
            }
        }
    }
}

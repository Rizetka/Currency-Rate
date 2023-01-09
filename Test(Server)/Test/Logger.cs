namespace Test
{
    public class Logger
    {
        private readonly object writeLock = new object();
        private string pathTologFile;

        public Logger(string pathToLogFile)
        {
            if (pathToLogFile == null)
            {
                throw new ArgumentNullException();
            }

            this.pathTologFile = pathToLogFile;

            Console.WriteLine("Logger created");
        }

        public void LogInFile(string logMsg)
        {
            DateTime currentDate = DateTime.Now;

            logMsg = $"[{currentDate.ToString()}] {logMsg}";

            lock(writeLock)
            {
                using (FileStream fs = new FileStream(pathTologFile, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter fstream = new StreamWriter(fs))
                    {
                        fstream.WriteLine(logMsg);
                    }
                }
            }            
        }
    }
}

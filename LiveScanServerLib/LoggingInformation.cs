// Redirect Logging to output
// Ioannis Selinis 2019 (5GIC, University of Surrey)

using System;
using System.IO;

public class LoggingInformation
{
    private string filePath;
    private bool flag = false;

    string basePath = AppDomain.CurrentDomain.BaseDirectory;
    string directory;

    public void CreateDirectory()
    {
        try
        {
            directory = basePath + "logging_output";
            if (Directory.Exists(directory))
            {
                //Console.WriteLine("Directory " + directory + " exists");
            }
            else
            {
                // alternative: DirectoryInfo di = Directory.CreateDirectory(directory);
                // in case we want to delete directory later, i.e. di.delete()
                Directory.CreateDirectory(directory);
                Console.WriteLine("Directory was created successfully at {0}.", Directory.GetCreationTime(directory));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Process of creating folder (TransferServer) failed: {0}", e.ToString());
        }
    }

    public string GetDirectory()
    {
        return directory;
    }

    public void RedirectOutput(string msg)
    {
        // Not thread safe, causing exceptions for concurrent uses of same file
        // Will isolate when file stat dumping is required
        /*
        try
        {
            using (StreamWriter sw = (File.Exists(filePath)) ? File.AppendText(filePath) : File.CreateText(filePath))
            {
                sw.WriteLine((msg));
                sw.Flush();
                sw.Close();
            }
        }
        catch (IOException e)
        {
            if (e.Source != null)
                Console.WriteLine("IOException LoggingInformation source: {0} ", e.Source);
        }*/
    }

    public void SetFilePath(string path)
    {
        if (!flag)
        {
            filePath = path;
            flag = true;
        }
    }

    public void ResetFlag()
    {
        flag = !flag;
    }
}

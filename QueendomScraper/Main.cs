using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace QueendomScraper
{
    public class Main
    {
        HttpClient client;
        ResultFile resultFile;
        List<int> TimeOuts;
        int totalQuizzes;
        const int FirstId = 1;
        const int LastId = 5000;
        const string BaseUrl = "https://www.queendom.com/tests/access_page/index.htm?idRegTest=";
        const string FilePath = "QuizList.html";

        private void ParseFile()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            this.client = new HttpClient(httpClientHandler);
            resultFile = new ResultFile(FilePath);
            var counter = new Counter();
            totalQuizzes = 0;
            TimeOuts = new List<int>();

            for (var i = FirstId; i <= LastId; i++)
            {
                int current = i;
                Thread call = new Thread(() => this.DoCall(current, counter));
                call.Start();
                if (counter.ShouldWaitBeforeNextCall())
                {
                    Thread.Sleep(3000);
                }
            }

            while (!counter.IsFinished())
            {
                Thread.Sleep(100);
            }

            resultFile.WriteToFile(TimeOuts, totalQuizzes);
            Console.WriteLine("Finished parsing.");
        }

        public void Run()
        {
            Console.WriteLine("Available commands:\n");
            Console.WriteLine("parse : Write down all quizzes to html.");
            Console.WriteLine("open : Opens the html file.");

            var command = Console.ReadLine();
            while (true)
            {
                switch (command)
                {
                    case "parse": this.ParseFile(); break;
                    case "open": this.OpenFile(); break;
                    default: Console.WriteLine("Unrecognized command."); break;
                }

                command = Console.ReadLine();
            }
        }

        private void OpenFile()
        {
            var processInfo = new ProcessStartInfo();
            processInfo.UseShellExecute = true;
            processInfo.FileName = string.Format("file:///{0}/{1}", Directory.GetCurrentDirectory(), FilePath);
            Process.Start(processInfo);
        }

        private async void DoCall(int id, Counter counter)
        {
            lock(counter)
            {
                counter.StartCall();
                Console.WriteLine("Making call for id " + id);
            }

            HttpResponseMessage response;
            var url = BaseUrl + id;
            try
            {
                response = await client.GetAsync(url);
            }
            catch
            {
                TimeOuts.Add(id);
                Finish("timed out.");
                return;
            }
          
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Finish("returned an unsuccessfull response.");
                return;
            }

            var textContent = await response.Content.ReadAsStringAsync();
            var traverser = new StringTraverser(textContent);
            if (textContent == "" || traverser.MoveUntil("breadcrumb", true) != MoveResult.Found)
            {
                Finish("was empty.");
                return;
            }

            if (traverser.MoveUntil("Quizzes", true, "</h3") != MoveResult.Found)
            {
                Finish("was not a quiz.");
                return;
            }

            traverser.MoveUntil("card-header", true);
            traverser.MoveUntil("<h3", false);
            traverser.MoveUntil(">", true);

            string title = traverser.ReadUntil("</h3");

            traverser.MoveUntil("card-body", true);

            var description = new StringBuilder();
            while (traverser.MoveUntil("<p>", true, "</div") == MoveResult.Found)
            {
                traverser.Move(1);
                var partialDescription = traverser.ReadUntil("</p");
                description.Append(HttpUtility.HtmlDecode(partialDescription) + "\n");
            }

            var descriptionText = description.ToString();
            if (descriptionText == "" && traverser.MoveUntil("Let's get started", false, "JavaScript") != MoveResult.Found)
            {
                Finish("did not have a start button.");
                return;
            }
            resultFile.AddResult(title, url, descriptionText);
            Finish("finished successfully");
            totalQuizzes++;

            void Finish(string finishReason)
            {
                lock (counter)
                {
                    counter.FinishCall();
                    Console.WriteLine("Call for id " + id + " " +finishReason);
                }
            }
        }
    }
}

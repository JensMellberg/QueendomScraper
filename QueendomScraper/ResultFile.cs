using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QueendomScraper
{
    public class ResultFile
    {
        const string HtmlStart = "<div class=\"outer\">";
        const string HtmlEnd = "</div>";
        List<string> html;
        int counter;

        public ResultFile()
        {
            html = new List<string>();
            counter = 0;
        }

        public void AddResult(string title, string url, string description)
        {
            counter++;
            html.Add("<div class=\"wrapper\"><div class=\"inner\"><a href =\"" +
                 url + "\"><span>" + title + "</span></a><div class=\"arrowWrapper\" number=\"" + counter + "\"><div class=\"arrow\"></div></div></div><div id =\"t" +
                 counter + "\" style=\"display: none;\">" + SplitIntoParagraphs(description) + "</div></div>");
        }

        public void WriteToFile(List<int> timeOuts, int totalQuizzes)
        {
            using (var writer = new StreamWriter("QuizList.html"))
            {
                if (timeOuts.Any())
                {
                    writer.WriteAsync("<p>The following requests timed out, maybe you missed a quiz :) (" + string.Join(", ", timeOuts) + ")</p>");
                }
                
                writer.WriteAsync("<p>Total number of quizzes: " + totalQuizzes + "</p>");
                writer.WriteLine(HtmlStart);
                html.ForEach(writer.WriteLine);
                writer.WriteLine(HtmlEnd);
                writer.WriteLine(this.JavaScript);
                writer.WriteLine(this.Css);
            }
        }

        public string SplitIntoParagraphs(string text) => "<p>" + text.Replace("\n", "</p><p>") + "</p>";

        private string JavaScript => "<script> const arrows = document.querySelectorAll('.arrowWrapper'); for (const arrow of arrows){const id = arrow.attributes.number.value;const description = document.getElementById('t' + id); arrow.addEventListener('click', () => {if (arrow.classList.contains('open')) {arrow.classList.remove('open');description.style = 'display: none;';} else {arrow.classList.add('open');description.style = 'display: visible;';}});} </script> ";

        private string Css => "<style> .outer {display: flex;gap: 15px;flex-direction: column;}.wrapper {	border: 2px solid black; width: fit-content; border-radius: 12px; display: flex; flex-direction: column;}.inner{display: flex;gap: 8px;}.arrow{    margin-left: 0.255em;vertical-align: 0.255em;content: \"\";   border-top: 0.6em solid;   border-right: 0.6em solid transparent;   border-bottom: 0;   border-left: 0.6em solid transparent;}.arrowWrapper{    margin-top: 17px;   margin-right: 5px;}.arrowWrapper.open.arrow {transform: rotate(180deg);}a{text-decoration: none;color: #45a5b2;	padding: 15px;}p{padding-left: 15px;padding-right: 15px;}</ style >";
    }
}

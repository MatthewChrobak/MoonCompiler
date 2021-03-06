﻿using System.Collections.Generic;

namespace ReportGenerator
{
    public class Report
    {
        public List<Section> Sections = new List<Section>();

        public string GenerateReport()
        {
            string basePage = "<!doctype html><html lang='en'><head><!-- Required meta tags --><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'><!-- Bootstrap CSS --><link rel='stylesheet' href='xml2tree.css'><link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css' integrity='sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm' crossorigin='anonymous'></head><body>";
            basePage += "<script src=\"xml2tree.min.js\"></script>";

            // Generate the table of contents.
            var tableOfContents = new Section("Table of Contents");
            tableOfContents.Add("<div class='row'><div class='col-sm-12'><ul>");
            foreach (var section in Sections) {
                tableOfContents.Add($"<li><a href='#{section.Header.Replace(' ', '_')}'>{section.Header}</a></li>");
            }
            tableOfContents.Add("</ul></div></div>");

            basePage += tableOfContents.Generate();

            // Add the actual content in.
            foreach (var section in Sections) {
                basePage += section.Generate();
            }

            string js = @"<!-- Optional JavaScript -->
    <!-- jQuery first, then Popper.js, then Bootstrap JS -->
    <script src='https://code.jquery.com/jquery-3.2.1.slim.min.js' integrity='sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN' crossorigin='anonymous'></script>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js' integrity='sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q' crossorigin='anonymous'></script>
    <script src='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js' integrity='sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl' crossorigin='anonymous'></script>";

            return basePage + js + "</body></html>";
        }
    }
}

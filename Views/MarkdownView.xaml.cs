using System.Windows;
using System.Windows.Controls;
using Playnite.SDK.Data;
using Playnite.SDK;
using TheArtOfDev.HtmlRenderer.WPF;
using System.Text.RegularExpressions;
using System.Linq;

namespace ESMetadata.Views
{
    public partial class MarkdownView : UserControl
    {
        public string HtmlContent { get; set; }
        public MarkdownView()
        {
            InitializeComponent();
        }

        public void LoadMarkdown(string markdownFilePath)
        {
            // Read the markdown file
            string markdownContent = System.IO.File.ReadAllText(markdownFilePath);

            Regex regex = new Regex(@"\[help_content\]: # \(start\)(.*?)\[help_content\]: # \(end\)",  RegexOptions.Singleline);

            MatchCollection matches = regex.Matches(markdownContent);

            if (matches.Count > 0)
            {
                markdownContent = string.Join("\n", matches.Cast<Match>().Select(m => m.Groups[0]));
            }
            // Convert markdown to HTML
            var htmlContent = Markup.MarkdownToHtml(markdownContent);

            HtmlContent = Regex.Replace(htmlContent, @"<li>\s*<p>(.*?)<\/p>", @"<li>$1");


            System.Windows.Window window = ESMetadata.PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions{ ShowMinimizeButton = false });
            window.Title =  ResourceProvider.GetString("LOC_ESMETADATA_HelpMeTitle");
            window.Content = this;
            window.DataContext = this;
            window.Owner = ESMetadata.PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            window.Height = 800;
            window.Width = 650;

            window.Show();

        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the UserControl or perform any other close action
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}


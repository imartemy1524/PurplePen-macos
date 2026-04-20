// ReportDialog.axaml.cs
//
// Code-behind for the report display dialog.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PurplePen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for ReportDialog.
    /// Parses HTML content and builds formatted UI controls.
    /// </summary>
    public partial class ReportDialog : Window
    {
        public ReportDialog()
        {
            InitializeComponent();
            DataContextChanged += ReportDialog_DataContextChanged;
        }

        private void ReportDialog_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is ReportDialogViewModel vm)
            {
                Loaded += (s, args) => RenderReport(vm.HtmlContent_Property);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RenderReport(string html)
        {
            var mainPanel = new StackPanel
            {
                Spacing = 0,
                Margin = new Thickness(0)
            };

            // Add title
            var titleBorder = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                Padding = new Thickness(20, 16),
                Child = new TextBlock
                {
                    Text = Title,
                    FontSize = 22,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#1F1F1F"))
                }
            };
            mainPanel.Children.Add(titleBorder);

            // Add content
            var contentPanel = new StackPanel
            {
                Spacing = 0,
                Margin = new Thickness(20, 16),
                Children = { ParseHtmlContent(html) }
            };
            mainPanel.Children.Add(contentPanel);

            // Set as scroll viewer content
            reportScroller.Content = mainPanel;
        }

        private Control ParseHtmlContent(string html)
        {
            var panel = new StackPanel { Spacing = 0 };

            // Remove script and style tags
            html = Regex.Replace(html, "<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, "<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Split into blocks
            var blocks = SplitIntoBlocks(html);

            foreach (var block in blocks)
            {
                var control = CreateBlockControl(block);
                if (control != null)
                {
                    panel.Children.Add(control);
                }
            }

            return panel;
        }

        private List<(string type, string content)> SplitIntoBlocks(string html)
        {
            var blocks = new List<(string, string)>();
            var pos = 0;

            while (pos < html.Length)
            {
                var h1Match = Regex.Match(html.Substring(pos), "^<h1[^>]*>(.*?)</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var h2Match = Regex.Match(html.Substring(pos), "^<h2[^>]*>(.*?)</h2>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var h3Match = Regex.Match(html.Substring(pos), "^<h3[^>]*>(.*?)</h3>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var pMatch = Regex.Match(html.Substring(pos), "^<p[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var tableMatch = Regex.Match(html.Substring(pos), "^<table[^>]*>(.*?)</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var ulMatch = Regex.Match(html.Substring(pos), "^<ul[^>]*>(.*?)</ul>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var olMatch = Regex.Match(html.Substring(pos), "^<ol[^>]*>(.*?)</ol>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var matchList = new[] {
                    (match: h1Match, type: "h1"), (match: h2Match, type: "h2"), (match: h3Match, type: "h3"),
                    (match: pMatch, type: "p"), (match: tableMatch, type: "table"), (match: ulMatch, type: "ul"), (match: olMatch, type: "ol")
                }
                .Where(x => x.match.Success)
                .OrderBy(x => x.match.Index)
                .ToList();

                if (matchList.Count == 0)
                    break;

                var match = matchList.First();

                if (match.match.Index > 0)
                {
                    pos += match.match.Index;
                }

                blocks.Add((match.type, match.match.Groups[1].Value));
                pos += match.match.Value.Length;
            }

            return blocks;
        }

        private Control CreateBlockControl((string type, string content) block)
        {
            return block.type switch
            {
                "h1" => CreateHeading(1, block.content),
                "h2" => CreateHeading(2, block.content),
                "h3" => CreateHeading(3, block.content),
                "p" => CreateParagraph(block.content),
                "table" => CreateTable(block.content),
                "ul" or "ol" => CreateList(block.content),
                _ => null
            };
        }

        private Control CreateHeading(int level, string content)
        {
            var fontSizes = new[] { 0, 28, 22, 18 };
            var fontSize = level < fontSizes.Length ? fontSizes[level] : 14;

            return new TextBlock
            {
                Text = StripHtmlTags(content),
                FontSize = fontSize,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#1F1F1F")),
                Margin = new Thickness(0, fontSize == 28 ? 0 : 12, 0, 8),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private Control CreateParagraph(string content)
        {
            return new TextBlock
            {
                Text = StripHtmlTags(content),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse("#323232")),
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 1.4
            };
        }

        private Control CreateTable(string content)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 12, 0, 16)
            };

            var rows = Regex.Matches(content, "<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (rows.Count == 0) return grid;

            int maxCols = 0;
            var tableRows = new List<List<string>>();

            foreach (Match rowMatch in rows)
            {
                var row = new List<string>();
                var cells = Regex.Matches(rowMatch.Groups[1].Value, "<t[dh][^>]*>(.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match cellMatch in cells)
                {
                    row.Add(StripHtmlTags(cellMatch.Groups[1].Value).Trim());
                }

                if (row.Count > 0)
                {
                    tableRows.Add(row);
                    maxCols = Math.Max(maxCols, row.Count);
                }
            }

            // Setup columns
            for (int i = 0; i < maxCols; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }

            // Add rows
            for (int rowIdx = 0; rowIdx < tableRows.Count; rowIdx++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                bool isHeader = rowIdx == 0;

                for (int colIdx = 0; colIdx < maxCols; colIdx++)
                {
                    var cell = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.Parse("#B4B4B4")),
                        Background = isHeader ? new SolidColorBrush(Color.Parse("#F0F0F0")) : new SolidColorBrush(Colors.White),
                        Padding = new Thickness(8, 6),
                        Child = new TextBlock
                        {
                            Text = colIdx < tableRows[rowIdx].Count ? tableRows[rowIdx][colIdx] : "",
                            FontSize = 11,
                            FontWeight = isHeader ? FontWeight.SemiBold : FontWeight.Normal,
                            TextWrapping = TextWrapping.Wrap
                        }
                    };

                    Grid.SetRow(cell, rowIdx);
                    Grid.SetColumn(cell, colIdx);
                    grid.Children.Add(cell);
                }
            }

            return grid;
        }

        private Control CreateList(string content)
        {
            var panel = new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 8, 0, 12)
            };

            var items = Regex.Matches(content, "<li[^>]*>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match itemMatch in items)
            {
                var itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(16, 2)
                };

                itemPanel.Children.Add(new TextBlock
                {
                    Text = "•",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#646464"))
                });

                itemPanel.Children.Add(new TextBlock
                {
                    Text = StripHtmlTags(itemMatch.Groups[1].Value),
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.Parse("#323232"))
                });

                panel.Children.Add(itemPanel);
            }

            return panel;
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            var text = Regex.Replace(html, "<[^>]+>", "");

            // Decode entities
            text = text.Replace("&nbsp;", " ");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("&apos;", "'");
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&amp;", "&");

            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }
    }
}

﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Roslynator.Metadata;
using Roslynator.Utilities;
using Roslynator.Utilities.Markdown;

namespace Roslynator.CodeGeneration.Markdown
{
    public static class MarkdownGenerator
    {
        public static string CreateReadMe(IEnumerable<AnalyzerDescriptor> analyzers, IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendLine(File.ReadAllText(@"..\text\ReadMe.txt", Encoding.UTF8));

            sb.AppendHeader3("List of Analyzers");
            sb.AppendLine();

            foreach (AnalyzerDescriptor info in analyzers.OrderBy(f => f.Id, comparer))
            {
                sb.AppendUnorderedListItem();
                sb.Append(info.Id);
                sb.Append(" - ");
                sb.AppendLink(info.Title.TrimEnd('.'), $"docs/analyzers/{info.Id}.md");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendHeader3("List of Refactorings");
            sb.AppendLine();

            foreach (RefactoringDescriptor info in refactorings.OrderBy(f => f.Title, comparer))
            {
                sb.AppendUnorderedListItem();
                sb.AppendLink(info.Title.TrimEnd('.'), $"docs/refactorings/{info.Id}.md");
            }

            return sb.ToString();
        }

        public static string CreateRefactoringsMarkdown(IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Refactorings");

            foreach (RefactoringDescriptor info in refactorings
                .OrderBy(f => f.Title, comparer))
            {
                sb.AppendLine();
                sb.AppendHeader4($"{info.Title} ({info.Id})");
                sb.AppendLine();
                sb.AppendUnorderedListItem();
                sb.AppendBold("Syntax");
                sb.Append(": ");

                bool isFirst = true;

                foreach (SyntaxDescriptor syntax in info.Syntaxes)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                    sb.Append(syntax.Name.EscapeMarkdown());
                }

                sb.AppendLine();

                if (!string.IsNullOrEmpty(info.Span))
                {
                    sb.AppendUnorderedListItem();
                    sb.AppendBold("Span");
                    sb.Append(": ");
                    sb.AppendLine(info.Span.EscapeMarkdown());
                }

                sb.AppendLine();

                WriteRefactoringSamples(sb, info);
            }

            return sb.ToString();
        }

        private static void WriteRefactoringSamples(StringBuilder sb, RefactoringDescriptor refactoring)
        {
            if (refactoring.Samples.Count > 0)
            {
                WriteSamples(sb, refactoring.Samples, 4);
            }
            else if (refactoring.Images.Count > 0)
            {
                bool isFirst = true;

                foreach (ImageDescriptor image in refactoring.Images)
                {
                    if (!isFirst)
                        sb.AppendLine();

                    AppendRefactoringImage(sb, refactoring, image.Name);
                    isFirst = false;
                }
            }
            else
            {
                AppendRefactoringImage(sb, refactoring, refactoring.Identifier);
            }
        }

        private static void WriteSamples(StringBuilder sb, IEnumerable<SampleDescriptor> samples, int headerLevel)
        {
            bool isFirst = true;

            foreach (SampleDescriptor sample in samples)
            {
                if (!isFirst)
                {
                    sb.AppendHorizonalRule();
                }
                else
                {
                    isFirst = false;
                }

                sb.AppendHeader("Before", headerLevel);
                sb.AppendLine();
                sb.AppendCSharpCodeBlock(sample.Before);
                sb.AppendLine();

                sb.AppendHeader("After", headerLevel);
                sb.AppendLine();
                sb.AppendCSharpCodeBlock(sample.After);
            }
        }

        public static string CreateRefactoringMarkdown(RefactoringDescriptor refactoring)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2($"{refactoring.Title}");
            sb.AppendLine();

            sb.AppendTableHeader("Property", "Value");
            sb.AppendTableRow("Id", refactoring.Id);
            sb.AppendTableRow("Title", refactoring.Title);
            sb.AppendTableRow("Syntax", string.Join(", ", refactoring.Syntaxes.Select(f => f.Name)));

            if (!string.IsNullOrEmpty(refactoring.Span))
                sb.AppendTableRow("Span", refactoring.Span);

            sb.AppendTableRow("Enabled by Default", GetBooleanAsText(refactoring.IsEnabledByDefault));

            sb.AppendLine();
            sb.AppendHeader3("Usage");
            sb.AppendLine();

            WriteRefactoringSamples(sb, refactoring);

            sb.AppendLine();

            sb.AppendLink("full list of refactorings", "Refactorings.md");

            return sb.ToString();
        }

        public static string CreateAnalyzerMarkdown(AnalyzerDescriptor analyzer)
        {
            var sb = new StringBuilder();

            string title = analyzer.Title.TrimEnd('.');
            sb.AppendHeader($"{((analyzer.IsObsolete) ? "[deprecated] " : "")}{analyzer.Id}: {title}");
            sb.AppendLine();

            sb.AppendTableHeader("Property", "Value");
            sb.AppendTableRow("Id", analyzer.Id);
            sb.AppendTableRow("Category", analyzer.Category);
            sb.AppendTableRow("Default Severity", analyzer.DefaultSeverity);
            sb.AppendTableRow("Enabled by Default", GetBooleanAsText(analyzer.IsEnabledByDefault));
            sb.AppendTableRow("Supports Fade-Out", GetBooleanAsText(analyzer.SupportsFadeOut));
            sb.AppendTableRow("Supports Fade-Out Analyzer", GetBooleanAsText(analyzer.SupportsFadeOutAnalyzer));

            ReadOnlyCollection<SampleDescriptor> samples = analyzer.Samples;

            if (samples.Count > 0)
            {
                sb.AppendLine();
                sb.AppendHeader2("Examples");
                sb.AppendLine();

                WriteSamples(sb, samples, 3);
            }

            sb.AppendLine();
            sb.AppendHeader2("How to Suppress");
            sb.AppendLine();

            sb.AppendHeader3("SuppressMessageAttribute");
            sb.AppendLine();

            sb.AppendCSharpCodeBlock($"[assembly: SuppressMessage(\"{analyzer.Category}\", \"{analyzer.Id}:{analyzer.Title}\", Justification = \"<Pending>\")]");
            sb.AppendLine();

            sb.AppendHeader3("#pragma");
            sb.AppendLine();

            sb.AppendCSharpCodeBlock($@"#pragma warning disable {analyzer.Id} // {analyzer.Title}
#pragma warning restore {analyzer.Id} // {analyzer.Title}");

            sb.AppendLine();

            sb.AppendHeader3("Ruleset");
            sb.AppendLine();

            sb.AppendUnorderedListItem();
            sb.AppendLink("How to configure rule set", "../HowToConfigureAnalyzers.md");
            sb.AppendLine();

            return sb.ToString();
        }

        public static string CreateAnalyzersReadMe(IEnumerable<AnalyzerDescriptor> analyzers, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Analyzers");
            sb.AppendLine();

            sb.AppendTableHeader("Id", "Title", "Category", new ColumnInfo("Enabled by Default", Alignment.Center));

            foreach (AnalyzerDescriptor info in analyzers.OrderBy(f => f.Id, comparer))
            {
                sb.AppendTableRow(
                    info.Id,
                    new LinkInfo(info.Title.TrimEnd('.'), $"../../docs/analyzers/{info.Id}.md"),
                    info.Category,
                    (info.IsEnabledByDefault) ? "x" : "");
            }

            return sb.ToString();
        }

        public static string CreateRefactoringsReadMe(IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Refactorings");
            sb.AppendLine();

            sb.AppendTableHeader("Id", "Title", new ColumnInfo("Enabled by Default", Alignment.Center));

            foreach (RefactoringDescriptor info in refactorings.OrderBy(f => f.Title, comparer))
            {
                sb.AppendTableRow(
                    info.Id,
                    new LinkInfo(info.Title.TrimEnd('.'), $"../../docs/refactorings/{info.Id}.md"),
                    (info.IsEnabledByDefault) ? "x" : "");
            }

            return sb.ToString();
        }

        public static string CreateCodeFixesReadMe(IEnumerable<CodeFixDescriptor> codeFixes, IEnumerable<CompilerDiagnosticDescriptor> diagnostics, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Code Fixes");
            sb.AppendLine();

            sb.AppendTableHeader("Id", "Title", "Fixable Diagnostics", new ColumnInfo("Enabled by Default", Alignment.Center));

            foreach (CodeFixDescriptor codeFix in codeFixes.OrderBy(f => f.Title, comparer))
            {
                IEnumerable<LinkInfo> fixableDiagnostics = codeFix
                    .FixableDiagnosticIds
                    .Join(diagnostics, f => f, f => f.Id, (f, g) => new LinkInfo(g.Id, g.HelpUrl));

                sb.AppendTableRow(
                    codeFix.Id,
                    codeFix.Title.TrimEnd('.'),
                    new MarkdownText(string.Join(", ", fixableDiagnostics), shouldEscape: false),
                    (codeFix.IsEnabledByDefault) ? "x" : "");
            }

            return sb.ToString();
        }

        public static string CreateCodeFixesByDiagnosticId(IEnumerable<CodeFixDescriptor> codeFixes, IEnumerable<CompilerDiagnosticDescriptor> diagnostics)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Code Fixes by Diagnostic Id");
            sb.AppendLine();

            sb.AppendTableHeader("Diagnostic", "Code Fixes");

            foreach (var grouping in codeFixes
                .SelectMany(codeFix => codeFix.FixableDiagnosticIds.Select(diagnosticId => new { DiagnosticId = diagnosticId, CodeFixDescriptor = codeFix }))
                .OrderBy(f => f.DiagnosticId)
                .ThenBy(f => f.CodeFixDescriptor.Id)
                .GroupBy(f => f.DiagnosticId))
            {
                CompilerDiagnosticDescriptor diagnostic = diagnostics.FirstOrDefault(f => f.Id == grouping.Key);

                if (!string.IsNullOrEmpty(diagnostic?.HelpUrl))
                {
                    sb.AppendLink(diagnostic.Id, diagnostic.HelpUrl);
                }
                else
                {
                    sb.Append(grouping.Key);
                }

                sb.Append('|');
                sb.Append(string.Join(", ", grouping.Select(f => f.CodeFixDescriptor.Id)));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string CreateAnalyzersByCategoryMarkdown(IEnumerable<AnalyzerDescriptor> analyzers, IComparer<string> comparer)
        {
            var sb = new StringBuilder();

            sb.AppendHeader2("Roslynator Analyzers by Category");
            sb.AppendLine();

            sb.AppendTableHeader("Category", "Title", "Id", new ColumnInfo("Enabled by Default", Alignment.Center));

            foreach (IGrouping<string, AnalyzerDescriptor> grouping in analyzers
                .GroupBy(f => f.Category.EscapeMarkdown())
                .OrderBy(f => f.Key, comparer))
            {
                foreach (AnalyzerDescriptor info in grouping.OrderBy(f => f.Title, comparer))
                {
                    sb.AppendTableRow(
                        grouping.Key,
                        new LinkInfo(info.Title.TrimEnd('.'), $"../../docs/analyzers/{info.Id}.md"),
                        info.Id,
                        (info.IsEnabledByDefault) ? "x" : "");
                }
            }

            return sb.ToString();
        }

        private static StringBuilder AppendRefactoringImage(StringBuilder sb, RefactoringDescriptor refactoring, string fileName)
        {
            return sb
                .AppendImage(refactoring.Title, $"../../images/refactorings/{fileName}.png")
                .AppendLine();
        }

        private static string GetBooleanAsText(bool value)
        {
            return (value) ? "yes" : "no";
        }
    }
}

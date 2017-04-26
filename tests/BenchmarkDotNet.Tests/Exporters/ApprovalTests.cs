﻿#if CLASSIC
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Tests.Mocks;
using Xunit;
namespace BenchmarkDotNet.Tests.Exporters
{
    [UseReporter(typeof(XUnit2Reporter))]
    [UseApprovalSubdirectory("ApprovedFiles")]
    public class ApprovalTest : IDisposable
    {
        private readonly CultureInfo initCulture;

        public ApprovalTest()
        {
            initCulture = Thread.CurrentThread.CurrentCulture;
        }

        [Theory]
        [MemberData(nameof(GetCultureInfos))]
        public void Exporters(CultureInfo cultureInfo)
        {
            NamerFactory.AdditionalInformation = $"{GetName(cultureInfo)}";
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            var logger = new AccumulationLogger();

            var exporters = GetExporters();
            foreach (var exporter in exporters)
            {
                PrintTitle(logger, exporter);
                exporter.ExportToLog(MockFactory.CreateSummary(config), logger);
            }

            Approvals.Verify(logger.GetLog());
        }

        private static void PrintTitle(AccumulationLogger logger, IExporter exporter)
        {
            logger.WriteLine("############################################");
            logger.WriteLine($"{exporter.Name}");
            logger.WriteLine("############################################");
        }

        private static TheoryData<CultureInfo> GetCultureInfos()
        {
            var cultures = new List<CultureInfo>()
            {
                CultureInfo.InvariantCulture,
                new CultureInfo("ru-RU"),
                new CultureInfo("en-US")
            };

            var theoryData = new TheoryData<CultureInfo>();
            foreach (var cultureInfo in cultures)
                theoryData.Add(cultureInfo);
            return theoryData;
        }

        private static string GetName(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name == string.Empty)
                return "Invariant";
            return cultureInfo.Name;
        }

        private static IEnumerable<IExporter> GetExporters()
        {
            //todo add CsvExporter and CsvMeasurementsExporter (need to mock RuntimeInformation)
            yield return AsciiDocExporter.Default;
            yield return HtmlExporter.Default;
            yield return JsonExporter.Brief;
            yield return JsonExporter.BriefCompressed;
            yield return JsonExporter.Full;
            yield return JsonExporter.FullCompressed;
            yield return MarkdownExporter.Default;
            yield return MarkdownExporter.Atlassian;
            yield return MarkdownExporter.Console;
            yield return MarkdownExporter.GitHub;
            yield return MarkdownExporter.StackOverflow;
            yield return PlainExporter.Default;
        }

        private static readonly IConfig config = ManualConfig.Create(DefaultConfig.Instance)
            .With(StatisticColumn.Mean)
            .With(StatisticColumn.StdDev)
            .With(StatisticColumn.P67);

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = initCulture;
        }
    }
}
#endif
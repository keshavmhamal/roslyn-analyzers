﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer.Utilities
{
    public abstract class AbstractVersionCheckAnalyzer : DiagnosticAnalyzer
    {
        private const string RuleId = "CA9999";
        private const string AnalyzerPackageVersion = "2.0.*";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(AnalyzerUtilitiesResources.VersionCheckTitle), AnalyzerUtilitiesResources.ResourceManager, typeof(AnalyzerUtilitiesResources));
        private static readonly LocalizableString s_localizableMessageFormat = new LocalizableResourceString(nameof(AnalyzerUtilitiesResources.VersionCheckMessage), AnalyzerUtilitiesResources.ResourceManager, typeof(AnalyzerUtilitiesResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(AnalyzerUtilitiesResources.VersionCheckDescription), AnalyzerUtilitiesResources.ResourceManager, typeof(AnalyzerUtilitiesResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                                                        RuleId,
                                                        s_localizableTitle,
                                                        s_localizableMessageFormat,
                                                        DiagnosticCategory.Reliability,
                                                        DiagnosticSeverity.Warning,
                                                        isEnabledByDefault: true,
                                                        description: s_localizableDescription);

        protected abstract string AnalyzerPackageName { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Suppress RS1013 as CompilationAction is only executed with FSA on and we want the analyzer to run even with FSA off.
#pragma warning disable RS1013 // Start action has no registered non-end actions.
            context.RegisterCompilationStartAction(compilationStartContext =>
#pragma warning restore RS1013 // Start action has no registered non-end actions.
            {
                compilationStartContext.RegisterCompilationEndAction(compilationContext =>
                {
                    if (!AnalysisContextExtensions.ShouldExecuteOperationAnalyzers)
                    {
                        // Version mismatch between the analyzer package '{0}' and Microsoft.CodeAnalysis '{1}'. Certain analyzers in this package will not run until the version mismatch is fixed.
                        var arg1 = AnalyzerPackageName + "-" + AnalyzerPackageVersion;
                        var arg2 = AnalysisContextExtensions.s_MicrosoftCodeAnalysisVersion;
                        var diagnostic = Diagnostic.Create(Rule, Location.None, arg1, arg2);
                        compilationContext.ReportDiagnostic(diagnostic);
                    }
                });
            });
        }
    }
}

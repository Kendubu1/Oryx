﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    [OutputType("go")]
    internal class GoOutput : IOutputFile
    {
        private static Template outputTemplate = CreateOutputTemplate();

        private ConstantCollection _collection;
        private string _directory;
        private string _package;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _directory = typeInfo["directory"];
            _package = typeInfo.GetValueOrDefault("package") ?? Path.GetFileName(_directory);
        }

        public string GetPath()
        {
            return Path.Combine(_directory, _collection.Name.Replace(ConstantCollection.NameSeparator, "_") + ".go");
        }

        public string GetContent()
        {
            var model = new ConstantCollectionTemplateModel
            {
                AutogenDisclaimer = Program.BuildAutogenDisclaimer(_collection.SourcePath),
                Namespace = _package,
                Constants = _collection.Constants.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value),
            };
            return outputTemplate.Render(model, member => member.Name);
        }

        private static Template CreateOutputTemplate()
        {
            var projectOutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var templateReader = new StreamReader(Path.Combine(projectOutputDir, "Outputs", "GoConstants.go.tpl")))
            {
                return Template.Parse(templateReader.ReadToEnd());
            }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents configuration options for the Razor Host
    /// </summary>
    public class MvcRazorHostOptions
    {
        private readonly List<InjectDescriptor> _defaultInjectedProperties = new List<InjectDescriptor>();
        private readonly HashSet<string> _defaultNamespaces = new HashSet<string>(StringComparer.Ordinal);

        public MvcRazorHostOptions()
        {
            DefaultNamespace = "ASP";
            DefaultBaseClass = "Microsoft.AspNet.Mvc.Razor.RazorPage";
            DefaultModel = "dynamic";
            ActivateAttributeName = "Microsoft.AspNet.Mvc.ActivateAttribute";

            DefaultInjectedProperties.Add(new InjectDescriptor("Microsoft.AspNet.Mvc.Rendering.IHtmlHelper<TModel>", "Html"));
            DefaultInjectedProperties.Add(new InjectDescriptor("Microsoft.AspNet.Mvc.IViewComponentHelper", "Component"));

            DefaultImportedNamespaces.Add("System");
            DefaultImportedNamespaces.Add("System.Linq");
            DefaultImportedNamespaces.Add("System.Collections.Generic");
            DefaultImportedNamespaces.Add("Microsoft.AspNet.Mvc");
            DefaultImportedNamespaces.Add("Microsoft.AspNet.Mvc.Razor");
            DefaultImportedNamespaces.Add("Microsoft.AspNet.Mvc.Rendering");
        }

        /// <summary>
        /// Gets or sets the namespace that will contain generated classes.
        /// </summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        /// Gets or sets the base class for generated pages.
        /// </summary>
        public string DefaultBaseClass { get; set; }

        /// <summary>
        /// Gets or sets the model that is used by default for generated views
        /// when no model is explicily specified.
        /// </summary>
        public string DefaultModel { get; set; }

        /// <summary>
        /// Gets or sets the attribute that is used to decorate properties that are injected and need to
        /// be activated.
        /// </summary>
        public string ActivateAttributeName { get; set; }

        /// <summary>
        /// Gets the list of properties that are injected by default.
        /// </summary>
        public IList<InjectDescriptor> DefaultInjectedProperties
        {
            get { return _defaultInjectedProperties; }
        }

        /// <summary>
        /// Gets the list of namespaces that are imported in views by default.
        /// </summary>
        public ISet<string> DefaultImportedNamespaces
        {
            get { return _defaultNamespaces; }
        }
    }
}
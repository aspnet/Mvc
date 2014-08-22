﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Internal.Routing
{
    public class LinkGenerationDecisionTreeTest
    {
        [Fact]
        public void SelectSingleEntry_NoCriteria()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry, Assert.Single(matches));
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry, Assert.Single(matches));
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria_AmbientValues()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(values: null, ambientValues: new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry, Assert.Single(matches));
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria_Replaced()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(
                values: new { action = "Buy" },
                ambientValues: new { controller = "Store", action = "Cart" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry, Assert.Single(matches));
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria_AmbientValue_Ignored()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = (string)null });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(
                values: new { controller = "Store" },
                ambientValues: new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry, Assert.Single(matches));
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria_NoMatch()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "AddToCart" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public void SelectSingleEntry_MultipleCriteria_AmbientValue_NoMatch()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(
                values: new { controller = "Store" },
                ambientValues: new { controller = "Store", action = "Cart" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public void SelectMultipleEntries_OneDoesntMatch()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry1);

            var entry2 = CreateEntry(new { controller = "Store", action = "Cart" });
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(
                values: new { controller = "Store" },
                ambientValues: new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Same(entry1, Assert.Single(matches));
        }

        [Fact]
        public void SelectMultipleEntries_BothMatch_CriteriaSubset()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry1);

            var entry2 = CreateEntry(new { controller = "Store" });
            entry2.Order = 1;
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(
                values: new { controller = "Store" },
                ambientValues: new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Equal(entries, matches);
        }

        [Fact]
        public void SelectMultipleEntries_BothMatch_NonOverlappingCriteria()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entries.Add(entry1);

            var entry2 = CreateEntry(new { slug = "1234" });
            entry2.Order = 1;
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "Buy", slug = "1234" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Equal(entries, matches);
        }

        // Precedence is ignored for sorting because they have different order
        [Fact]
        public void SelectMultipleEntries_BothMatch_OrderedByOrder()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry1.Precedence = 1;
            entries.Add(entry1);

            var entry2 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry2.Order = 1;
            entry2.Precedence = 0;
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Equal(entries, matches);
        }

        // Precedence is used for sorting because they have the same order
        [Fact]
        public void SelectMultipleEntries_BothMatch_OrderedByPrecedence()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry1.Precedence = 0;
            entries.Add(entry1);

            var entry2 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry2.Precedence = 1;
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Equal(entries, matches);
        }

        // Template is used for sorting because they have the same order
        [Fact]
        public void SelectMultipleEntries_BothMatch_OrderedByTemplate()
        {
            // Arrange
            var entries = new List<AttributeRouteLinkGenerationEntry>();

            var entry1 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry1.TemplateText = "a";
            entries.Add(entry1);

            var entry2 = CreateEntry(new { controller = "Store", action = "Buy" });
            entry2.TemplateText = "b";
            entries.Add(entry2);

            var tree = new LinkGenerationDecisionTree(entries);

            var context = CreateContext(new { controller = "Store", action = "Buy" });

            // Act
            var matches = tree.GetMatches(context);

            // Assert
            Assert.Equal(entries, matches);
        }

        private AttributeRouteLinkGenerationEntry CreateEntry(object requiredValues)
        {
            var entry = new AttributeRouteLinkGenerationEntry();
            entry.RequiredLinkValues = new RouteValueDictionary(requiredValues);
            return entry;
        }

        private VirtualPathContext CreateContext(object values, object ambientValues = null)
        {
            var context = new VirtualPathContext(
                new DefaultHttpContext(),
                new RouteValueDictionary(ambientValues),
                new RouteValueDictionary(values));

            return context;
        }
    }
}
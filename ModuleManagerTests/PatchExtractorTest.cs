﻿using System;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using TestUtils;
using ModuleManager;
using ModuleManager.Progress;

namespace ModuleManagerTests
{
    public class PatchExtractorTest
    {
        private UrlDir root;
        private UrlDir.UrlFile file;

        private IPatchProgress progress;

        public PatchExtractorTest()
        {
            root = UrlBuilder.CreateRoot();
            file = UrlBuilder.CreateFile("abc/def.cfg", root);

            progress = Substitute.For<IPatchProgress>();
        }

        [Fact]
        public void TestSortAndExtractPatches()
        {
            UrlDir.UrlConfig[] insertConfigs =
            {
                CreateConfig("NODE"),
                CreateConfig("NADE"),
            };

            UrlDir.UrlConfig[] legacyConfigs =
            {
                CreateConfig("@NODE"),
                CreateConfig("@NADE[foo]:HAS[#bar]"),
            };

            UrlDir.UrlConfig[] firstConfigs =
            {
                CreateConfig("@NODE:FIRST"),
                CreateConfig("@NODE[foo]:HAS[#bar]:FIRST"),
                CreateConfig("@NADE:First"),
                CreateConfig("@NADE:first"),
            };

            UrlDir.UrlConfig[] finalConfigs =
            {
                CreateConfig("@NODE:FINAL"),
                CreateConfig("@NODE[foo]:HAS[#bar]:FINAL"),
                CreateConfig("@NADE:Final"),
                CreateConfig("@NADE:final"),
            };

            UrlDir.UrlConfig[] beforeMod1Configs =
            {
                CreateConfig("@NODE:BEFORE[mod1]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:BEFORE[mod1]"),
                CreateConfig("@NADE:before[mod1]"),
                CreateConfig("@NADE:BEFORE[MOD1]"),
            };

            UrlDir.UrlConfig[] forMod1Configs =
            {
                CreateConfig("@NODE:FOR[mod1]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:FOR[mod1]"),
                CreateConfig("@NADE:for[mod1]"),
                CreateConfig("@NADE:FOR[MOD1]"),
            };

            UrlDir.UrlConfig[] afterMod1Configs =
            {
                CreateConfig("@NODE:AFTER[mod1]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:AFTER[mod1]"),
                CreateConfig("@NADE:after[mod1]"),
                CreateConfig("@NADE:AFTER[MOD1]"),
            };

            UrlDir.UrlConfig[] lastMod1Configs =
            {
                CreateConfig("@NODE:LAST[mod1]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:LAST[mod1]"),
                CreateConfig("@NADE:last[mod1]"),
                CreateConfig("@NADE:LAST[MOD1]"),
            };

            UrlDir.UrlConfig[] beforeMod2Configs =
            {
                CreateConfig("@NODE:BEFORE[mod2]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:BEFORE[mod2]"),
                CreateConfig("@NADE:before[mod2]"),
                CreateConfig("@NADE:BEFORE[MOD2]"),
            };

            UrlDir.UrlConfig[] forMod2Configs =
            {
                CreateConfig("@NODE:FOR[mod2]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:FOR[mod2]"),
                CreateConfig("@NADE:for[mod2]"),
                CreateConfig("@NADE:FOR[MOD2]"),
            };

            UrlDir.UrlConfig[] afterMod2Configs =
            {
                CreateConfig("@NODE:AFTER[mod2]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:AFTER[mod2]"),
                CreateConfig("@NADE:after[mod2]"),
                CreateConfig("@NADE:AFTER[MOD2]"),
            };

            UrlDir.UrlConfig[] lastMod2Configs =
            {
                CreateConfig("@NODE:LAST[mod2]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:LAST[mod2]"),
                CreateConfig("@NADE:last[mod2]"),
                CreateConfig("@NADE:LAST[MOD2]"),
            };

            UrlDir.UrlConfig[] beforeMod3Configs =
            {
                CreateConfig("@NODE:BEFORE[mod3]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:BEFORE[mod3]"),
                CreateConfig("@NADE:before[mod3]"),
                CreateConfig("@NADE:BEFORE[MOD3]"),
            };

            UrlDir.UrlConfig[] forMod3Configs =
            {
                CreateConfig("@NODE:FOR[mod3]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:FOR[mod3]"),
                CreateConfig("@NADE:for[mod3]"),
                CreateConfig("@NADE:FOR[MOD3]"),
            };

            UrlDir.UrlConfig[] afterMod3Configs =
            {
                CreateConfig("@NODE:AFTER[mod3]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:AFTER[mod3]"),
                CreateConfig("@NADE:after[mod3]"),
                CreateConfig("@NADE:AFTER[MOD3]"),
            };

            UrlDir.UrlConfig[] lastMod3Configs =
            {
                CreateConfig("@NODE:LAST[mod3]"),
                CreateConfig("@NODE[foo]:HAS[#bar]:LAST[mod3]"),
                CreateConfig("@NADE:last[mod3]"),
                CreateConfig("@NADE:LAST[MOD3]"),
            };

            string[] modList = { "mod1", "mod2" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            progress.DidNotReceiveWithAnyArgs().Error(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null, null);

            Assert.True(list.modPasses.HasMod("mod1"));
            Assert.True(list.modPasses.HasMod("mod2"));
            Assert.False(list.modPasses.HasMod("mod3"));

            Assert.Equal(insertConfigs, root.AllConfigs);

            Assert.Equal(legacyConfigs, list.legacyPatches);

            List<UrlDir.UrlConfig> currentPatches;

            currentPatches = list.firstPatches;
            Assert.Equal(firstConfigs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                firstConfigs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", firstConfigs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                firstConfigs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                firstConfigs[3], currentPatches[3]);

            currentPatches = list.finalPatches;
            Assert.Equal(finalConfigs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                finalConfigs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", finalConfigs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                finalConfigs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                finalConfigs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod1"].beforePatches;
            Assert.Equal(beforeMod1Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                beforeMod1Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", beforeMod1Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                beforeMod1Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                beforeMod1Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod1"].forPatches;
            Assert.Equal(forMod1Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                forMod1Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", forMod1Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                forMod1Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                forMod1Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod1"].afterPatches;
            Assert.Equal(afterMod1Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                afterMod1Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", afterMod1Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                afterMod1Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                afterMod1Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod1"].lastPatches;
            Assert.Equal(lastMod1Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE", lastMod1Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", lastMod1Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE", lastMod1Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE", lastMod1Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod2"].beforePatches;
            Assert.Equal(beforeMod2Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                beforeMod2Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", beforeMod2Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                beforeMod2Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                beforeMod2Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod2"].forPatches;
            Assert.Equal(forMod2Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                forMod2Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", forMod2Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                forMod2Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                forMod2Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod2"].afterPatches;
            Assert.Equal(afterMod2Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE",                afterMod2Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", afterMod2Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE",                afterMod2Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE",                afterMod2Configs[3], currentPatches[3]);

            currentPatches = list.modPasses["mod2"].lastPatches;
            Assert.Equal(lastMod2Configs.Length, currentPatches.Count);
            AssertUrlCorrect("@NODE", lastMod2Configs[0], currentPatches[0]);
            AssertUrlCorrect("@NODE[foo]:HAS[#bar]", lastMod2Configs[1], currentPatches[1]);
            AssertUrlCorrect("@NADE", lastMod2Configs[2], currentPatches[2]);
            AssertUrlCorrect("@NADE", lastMod2Configs[3], currentPatches[3]);

            progress.Received(42).PatchAdded();

            progress.Received().NeedsUnsatisfiedBefore(beforeMod3Configs[0]);
            progress.Received().NeedsUnsatisfiedBefore(beforeMod3Configs[1]);
            progress.Received().NeedsUnsatisfiedBefore(beforeMod3Configs[2]);
            progress.Received().NeedsUnsatisfiedBefore(beforeMod3Configs[3]);

            progress.Received().NeedsUnsatisfiedFor(forMod3Configs[0]);
            progress.Received().NeedsUnsatisfiedFor(forMod3Configs[1]);
            progress.Received().NeedsUnsatisfiedFor(forMod3Configs[2]);
            progress.Received().NeedsUnsatisfiedFor(forMod3Configs[3]);

            progress.Received().NeedsUnsatisfiedAfter(afterMod3Configs[0]);
            progress.Received().NeedsUnsatisfiedAfter(afterMod3Configs[1]);
            progress.Received().NeedsUnsatisfiedAfter(afterMod3Configs[2]);
            progress.Received().NeedsUnsatisfiedAfter(afterMod3Configs[3]);

            progress.Received().NeedsUnsatisfiedLast(lastMod3Configs[0]);
            progress.Received().NeedsUnsatisfiedLast(lastMod3Configs[1]);
            progress.Received().NeedsUnsatisfiedLast(lastMod3Configs[2]);
            progress.Received().NeedsUnsatisfiedLast(lastMod3Configs[3]);
        }

        [Fact]
        public void TestSortAndExtractPatches__InsertWithPass()
        {
            UrlDir.UrlConfig config1 = CreateConfig("NODE");
            UrlDir.UrlConfig config2 = CreateConfig("NODE:FOR[mod1]");
            UrlDir.UrlConfig config3 = CreateConfig("NODE:FOR[mod2]");
            UrlDir.UrlConfig config4 = CreateConfig("NODE:FINAL");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            Assert.Equal(new[] { config1 }, root.AllConfigs);

            progress.Received().Error(config2, "Error - pass specifier detected on an insert node (not a patch): abc/def/NODE:FOR[mod1]");
            progress.Received().Error(config3, "Error - pass specifier detected on an insert node (not a patch): abc/def/NODE:FOR[mod2]");
            progress.Received().Error(config4, "Error - pass specifier detected on an insert node (not a patch): abc/def/NODE:FINAL");

            Assert.Empty(list.firstPatches);
            Assert.Empty(list.legacyPatches);
            Assert.Empty(list.finalPatches);
            Assert.Empty(list.modPasses["mod1"].beforePatches);
            Assert.Empty(list.modPasses["mod1"].forPatches);
            Assert.Empty(list.modPasses["mod1"].afterPatches);

            progress.DidNotReceive().PatchAdded();
        }

        [Fact]
        public void TestSortAndExtractPatches__MoreThanOnePass()
        {
            UrlDir.UrlConfig config1 = CreateConfig("@NODE:FIRST");
            UrlDir.UrlConfig config2 = CreateConfig("@NODE:FIRST:FIRST");
            UrlDir.UrlConfig config3 = CreateConfig("@NODE:FIRST:FOR[mod1]");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            Assert.Empty(root.AllConfigs);

            progress.Received().Error(config2, "Error - more than one pass specifier on a node: abc/def/@NODE:FIRST:FIRST");
            progress.Received().Error(config3, "Error - more than one pass specifier on a node: abc/def/@NODE:FIRST:FOR[mod1]");

            Assert.Equal(1, list.firstPatches.Count);
            AssertUrlCorrect("@NODE", config1, list.firstPatches[0]);
            Assert.Empty(list.legacyPatches);
            Assert.Empty(list.finalPatches);
            Assert.Empty(list.modPasses["mod1"].beforePatches);
            Assert.Empty(list.modPasses["mod1"].forPatches);
            Assert.Empty(list.modPasses["mod1"].afterPatches);

            progress.Received(1).PatchAdded();
        }

        [Fact]
        public void TestSortAndExtractPatches__Exception()
        {
            Exception e = new Exception("an exception was thrown");
            progress.WhenForAnyArgs(p => p.Error(null, null)).Throw(e);

            UrlDir.UrlConfig config1 = CreateConfig("@NODE");
            UrlDir.UrlConfig config2 = CreateConfig("@NODE:FIRST:FIRST");
            UrlDir.UrlConfig config3 = CreateConfig("@NADE:FIRST");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            progress.Received().Exception(config2, "Exception while parsing pass for config: abc/def/@NODE:FIRST:FIRST", e);

            Assert.Equal(new[] { config1 }, list.legacyPatches);
            Assert.Equal(1, list.firstPatches.Count);
            AssertUrlCorrect("@NADE", config3, list.firstPatches[0]);

            progress.Received(2).PatchAdded();
        }

        [Fact]
        public void TestSortAndExtractPatches__NotBracketBalanced()
        {
            UrlDir.UrlConfig config1 = CreateConfig("@NODE:FOR[mod1]");
            UrlDir.UrlConfig config2 = CreateConfig("@NODE:FOR[");
            UrlDir.UrlConfig config3 = CreateConfig("NODE:HAS[#foo[]");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            Assert.Empty(root.AllConfigs);

            progress.Received().Error(config2, "Error - node name does not have balanced brackets (or a space - if so replace with ?):\nabc/def/@NODE:FOR[");
            progress.Received().Error(config3, "Error - node name does not have balanced brackets (or a space - if so replace with ?):\nabc/def/NODE:HAS[#foo[]");
            
            Assert.Empty(list.firstPatches);
            Assert.Empty(list.legacyPatches);
            Assert.Empty(list.finalPatches);
            Assert.Empty(list.modPasses["mod1"].beforePatches);
            Assert.Equal(1, list.modPasses["mod1"].forPatches.Count);
            AssertUrlCorrect("@NODE", config1, list.modPasses["mod1"].forPatches[0]);
            Assert.Empty(list.modPasses["mod1"].afterPatches);

            progress.Received(1).PatchAdded();
        }

        [Fact]
        public void TestSortAndExtractPatches__BadlyFormed()
        {
            UrlDir.UrlConfig config1 = CreateConfig("@NODE:FOR[mod1]");
            UrlDir.UrlConfig config2 = CreateConfig("@NODE:FOR[]");
            UrlDir.UrlConfig config3 = CreateConfig("@NADE:FIRST:BEFORE");
            UrlDir.UrlConfig config4 = CreateConfig("@NADE:AFTER");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            Assert.Empty(root.AllConfigs);

            progress.Received().Error(config2, "Error - malformed :FOR patch specifier detected: abc/def/@NODE:FOR[]");
            progress.Received().Error(config3, "Error - more than one pass specifier on a node: abc/def/@NADE:FIRST:BEFORE");
            progress.Received().Error(config3, "Error - malformed :BEFORE patch specifier detected: abc/def/@NADE:FIRST:BEFORE");
            progress.Received().Error(config4, "Error - malformed :AFTER patch specifier detected: abc/def/@NADE:AFTER");

            Assert.Empty(list.firstPatches);
            Assert.Empty(list.legacyPatches);
            Assert.Empty(list.finalPatches);
            Assert.Empty(list.modPasses["mod1"].beforePatches);
            Assert.Equal(1, list.modPasses["mod1"].forPatches.Count);
            AssertUrlCorrect("@NODE", config1, list.modPasses["mod1"].forPatches[0]);
            Assert.Empty(list.modPasses["mod1"].afterPatches);

            progress.Received(1).PatchAdded();
        }

        [Fact]
        public void TestSortAndExtractPatches__InvalidCommand()
        {
            UrlDir.UrlConfig config1 = CreateConfig("@NODE:FOR[mod1]");
            UrlDir.UrlConfig config2 = CreateConfig("%NODE:FOR[mod1]");
            UrlDir.UrlConfig config3 = CreateConfig("&NODE:FOR[mod1]");
            UrlDir.UrlConfig config4 = CreateConfig("|NODE:FOR[mod1]");
            UrlDir.UrlConfig config5 = CreateConfig("#NODE:FOR[mod1]");
            UrlDir.UrlConfig config6 = CreateConfig("*NODE:FOR[mod1]");

            string[] modList = { "mod1" };
            PatchList list = PatchExtractor.SortAndExtractPatches(root, modList, progress);

            Assert.Empty(root.AllConfigs);

            progress.Received().Error(config2, "Error - replace command (%) is not valid on a root node: abc/def/%NODE:FOR[mod1]");
            progress.Received().Error(config3, "Error - create command (&) is not valid on a root node: abc/def/&NODE:FOR[mod1]");
            progress.Received().Error(config4, "Error - rename command (|) is not valid on a root node: abc/def/|NODE:FOR[mod1]");
            progress.Received().Error(config5, "Error - paste command (#) is not valid on a root node: abc/def/#NODE:FOR[mod1]");
            progress.Received().Error(config6, "Error - special command (*) is not valid on a root node: abc/def/*NODE:FOR[mod1]");

            Assert.Empty(list.firstPatches);
            Assert.Empty(list.legacyPatches);
            Assert.Empty(list.finalPatches);
            Assert.Empty(list.modPasses["mod1"].beforePatches);
            Assert.Equal(1, list.modPasses["mod1"].forPatches.Count);
            AssertUrlCorrect("@NODE", config1, list.modPasses["mod1"].forPatches[0]);
            Assert.Empty(list.modPasses["mod1"].afterPatches);

            progress.Received(1).PatchAdded();
        }

        private UrlDir.UrlConfig CreateConfig(string name)
        {
            ConfigNode node = new TestConfigNode(name)
            {
                { "name", "snack" },
                { "cheese", "gouda" },
                { "bread", "sourdough" },
                new ConfigNode("wine"),
                new ConfigNode("fruit"),
            };

            node.id = "hungry?";

            return UrlBuilder.CreateConfig(node, file);
        }

        private void AssertUrlCorrect(string expectedNodeName, UrlDir.UrlConfig originalUrl, UrlDir.UrlConfig observedUrl)
        {
            Assert.Equal(expectedNodeName, observedUrl.type);

            ConfigNode originalNode = originalUrl.config;
            ConfigNode observedNode = observedUrl.config;

            Assert.Equal(expectedNodeName, observedNode.name);

            if (originalNode.HasValue("name")) Assert.Equal(originalNode.GetValue("name"), observedUrl.name);

            Assert.Same(originalUrl.parent, observedUrl.parent);

            Assert.Equal(originalNode.id, observedNode.id);
            Assert.Equal(originalNode.values.Count, observedNode.values.Count);
            Assert.Equal(originalNode.nodes.Count, observedNode.nodes.Count);

            for (int i = 0; i < originalNode.values.Count; i++)
            {
                Assert.Same(originalNode.values[i], observedNode.values[i]);
            }

            for (int i = 0; i < originalNode.nodes.Count; i++)
            {
                Assert.Same(originalNode.nodes[i], observedNode.nodes[i]);
            }
        }
    }
}

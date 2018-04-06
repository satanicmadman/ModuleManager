using System;
using System.Linq;
using Xunit;
using NSubstitute;
using UnityEngine;
using TestUtils;
using ModuleManager;
using ModuleManager.Logging;
using ModuleManager.Progress;

namespace ModuleManagerTests
{
    public class PatchApplierTest
    {
        private readonly IBasicLogger logger;
        private readonly IPatchProgress progress;
        private readonly string[] modList = new[] { "mod1", "mod2" };
        private UrlDir databaseRoot;
        private UrlDir.UrlFile file;
        private readonly PatchList patchList;
        private readonly PatchApplier patchApplier;

        public PatchApplierTest()
        {
            logger = Substitute.For<IBasicLogger>();
            progress = Substitute.For<IPatchProgress>();
            databaseRoot = UrlBuilder.CreateRoot();
            file = UrlBuilder.CreateFile("abc/def.cfg", databaseRoot);
            patchList = new PatchList(modList);
            patchApplier = new PatchApplier(patchList, databaseRoot, progress, logger);
        }

        [Fact]
        public void TestApplyPatches__Edit()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "abc" },
                { "foo", "bar" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "def" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART")
            {
                { "@foo", "baz" },
                { "pqr", "stw" },
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patch1);
            progress.Received().ApplyingUpdate(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(3, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "abc" },
                { "foo", "baz" },
                { "pqr", "stw" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "def" },
                { "pqr", "stw" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, allConfigs[2].config);
        }

        [Fact]
        public void TestApplyPatches__Copy()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "003" },
                { "bbb", "004" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("+PART")
            {
                { "@name ^", ":^00:01:" },
                { "@aaa", "011" },
                { "ccc", "005" },
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingCopy(config1, patch1);
            progress.Received().ApplyingCopy(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(5, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "003" },
                { "bbb", "004" },
            }, allConfigs[2].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "010" },
                { "aaa", "011" },
                { "ccc", "005" },
            }, allConfigs[3].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "012" },
                { "ccc", "005" },
            }, allConfigs[4].config);
        }

        [Fact]
        public void TestApplyPatches__Copy__AlternateCommand()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "003" },
                { "bbb", "004" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("$PART")
            {
                { "@name ^", ":^00:01:" },
                { "@aaa", "011" },
                { "ccc", "005" },
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingCopy(config1, patch1);
            progress.Received().ApplyingCopy(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(5, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "003" },
                { "bbb", "004" },
            }, allConfigs[2].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "010" },
                { "aaa", "011" },
                { "ccc", "005" },
            }, allConfigs[3].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "012" },
                { "ccc", "005" },
            }, allConfigs[4].config);
        }

        [Fact]
        public void TestApplyPatches__Delete()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "abc" },
                { "foo", "bar" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "def" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("-PART"));

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingDelete(config1, patch1);
            progress.Received().ApplyingDelete(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, allConfigs[0].config);
        }

        [Fact]
        public void TestApplyPatches__Delete__AlternateCommand()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "abc" },
                { "foo", "bar" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "def" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("-PART"));

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingDelete(config1, patch1);
            progress.Received().ApplyingDelete(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "ghi" },
                { "jkl", "mno" },
            }, allConfigs[0].config);
        }

        [Fact]
        public void TestApplyPatches__Name()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "004" },
                { "ccc", "005" },
            }, file);

            UrlDir.UrlConfig config4 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000]")
            {
                { "@aaa", "011" },
                { "eee", "012" },
            });

            UrlDir.UrlConfig patch2 = new UrlDir.UrlConfig(file, new TestConfigNode("+PART[002]")
            {
                { "@name", "022" },
                { "@bbb", "013" },
                { "fff", "014" },
            });

            UrlDir.UrlConfig patch3 = new UrlDir.UrlConfig(file, new TestConfigNode("!PART[004]"));

            patchList.firstPatches.Add(patch1);
            patchList.firstPatches.Add(patch2);
            patchList.firstPatches.Add(patch3);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(3).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patch1);
            progress.Received().ApplyingCopy(config2, patch2);
            progress.Received().ApplyingDelete(config3, patch3);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(4, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "011" },
                { "eee", "012" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, allConfigs[2].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "022" },
                { "bbb", "013" },
                { "fff", "014" },
            }, allConfigs[3].config);
        }

        [Fact]
        public void TestApplyPatches__Name__Wildcard()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "004" },
                { "ccc", "005" },
            }, file);

            UrlDir.UrlConfig config4 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART[0*0]")
            {
                { "@aaa", "011" },
                { "eee", "012" },
            });

            UrlDir.UrlConfig patch2 = new UrlDir.UrlConfig(file, new TestConfigNode("+PART[0*2]")
            {
                { "@name", "022" },
                { "@bbb", "013" },
                { "fff", "014" },
            });

            UrlDir.UrlConfig patch3 = new UrlDir.UrlConfig(file, new TestConfigNode("!PART[0*4]"));

            patchList.firstPatches.Add(patch1);
            patchList.firstPatches.Add(patch2);
            patchList.firstPatches.Add(patch3);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(3).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patch1);
            progress.Received().ApplyingCopy(config2, patch2);
            progress.Received().ApplyingDelete(config3, patch3);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(4, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "011" },
                { "eee", "012" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, allConfigs[2].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "022" },
                { "bbb", "013" },
                { "fff", "014" },
            }, allConfigs[3].config);
        }

        [Fact]
        public void TestApplyPatches__Name__Or()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "004" },
                { "ccc", "005" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
            {
                { "@aaa", "011" },
                { "ddd", "006" },
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patch1);
            progress.Received().ApplyingUpdate(config2, patch1);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(3, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "011" },
                { "ddd", "006" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
                { "ddd", "006" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "004" },
                { "ccc", "005" },
            }, allConfigs[2].config);
        }

        [Fact]
        public void TestApplyPatches__Order()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aa", "001" },
            }, file);
            
            UrlDir.UrlConfig[] patches = {
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "bb", "002" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "cc", "003" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "dd", "004" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "ee", "005" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "ff", "006" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "gg", "007" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "hh", "008" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "ii", "009" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "jj", "010" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "kk", "011" },
                }),
                new UrlDir.UrlConfig(file, new TestConfigNode("@PART[000|0*2]")
                {
                    { "ll", "012" },
                }),
            };

            patchList.firstPatches.Add(patches[0]);
            patchList.legacyPatches.Add(patches[1]);
            patchList.modPasses["mod1"].beforePatches.Add(patches[2]);
            patchList.modPasses["mod1"].forPatches.Add(patches[3]);
            patchList.modPasses["mod1"].afterPatches.Add(patches[4]);
            patchList.modPasses["mod2"].beforePatches.Add(patches[5]);
            patchList.modPasses["mod2"].forPatches.Add(patches[6]);
            patchList.modPasses["mod2"].afterPatches.Add(patches[7]);
            patchList.modPasses["mod1"].lastPatches.Add(patches[8]);
            patchList.modPasses["mod2"].lastPatches.Add(patches[9]);
            patchList.finalPatches.Add(patches[10]);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(patches.Length).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patches[0]);
            progress.Received().ApplyingUpdate(config1, patches[1]);
            progress.Received().ApplyingUpdate(config1, patches[2]);
            progress.Received().ApplyingUpdate(config1, patches[3]);
            progress.Received().ApplyingUpdate(config1, patches[4]);
            progress.Received().ApplyingUpdate(config1, patches[5]);
            progress.Received().ApplyingUpdate(config1, patches[6]);
            progress.Received().ApplyingUpdate(config1, patches[7]);
            progress.Received().ApplyingUpdate(config1, patches[8]);
            progress.Received().ApplyingUpdate(config1, patches[9]);
            progress.Received().ApplyingUpdate(config1, patches[10]);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aa", "001" },
                { "bb", "002" },
                { "cc", "003" },
                { "dd", "004" },
                { "ee", "005" },
                { "ff", "006" },
                { "gg", "007" },
                { "hh", "008" },
                { "ii", "009" },
                { "jj", "010" },
                { "kk", "011" },
                { "ll", "012" },
            }, allConfigs[0].config);
        }

        [Fact]
        public void TestApplyPatches__Constraints()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig config2 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, file);

            UrlDir.UrlConfig config3 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "004" },
                { "ccc", "005" },
            }, file);

            UrlDir.UrlConfig config4 = UrlBuilder.CreateConfig(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART:HAS[#aaa[001]]")
            {
                { "@aaa", "011" },
                { "eee", "012" },
            });

            UrlDir.UrlConfig patch2 = new UrlDir.UrlConfig(file, new TestConfigNode("+PART:HAS[#bbb[003]]")
            {
                { "@name", "012" },
                { "@bbb", "013" },
                { "fff", "014" },
            });

            UrlDir.UrlConfig patch3 = new UrlDir.UrlConfig(file, new TestConfigNode("!PART:HAS[#ccc[005]]"));

            patchList.firstPatches.Add(patch1);
            patchList.firstPatches.Add(patch2);
            patchList.firstPatches.Add(patch3);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(3).PatchApplied();
            progress.Received().ApplyingUpdate(config1, patch1);
            progress.Received().ApplyingCopy(config2, patch2);
            progress.Received().ApplyingDelete(config3, patch3);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(4, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "011" },
                { "eee", "012" },
            }, allConfigs[0].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "002" },
                { "bbb", "003" },
            }, allConfigs[1].config);

            AssertNodesEqual(new TestConfigNode("PORT")
            {
                { "name", "006" },
                { "ddd", "007" },
            }, allConfigs[2].config);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "012" },
                { "bbb", "013" },
                { "fff", "014" },
            }, allConfigs[3].config);
        }

        [Fact]
        public void TestApplyPatches__Loop()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "1" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("@PART:HAS[~aaa[>10]]")
            {
                { "@aaa *", "2" },
                { "bbb", "002" },
                new ConfigNode("MM_PATCH_LOOP"),
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();

            EnsureNoErrors();

            progress.Received(1).PatchApplied();
            progress.Received(4).ApplyingUpdate(config1, patch1);

            logger.Received().Log(LogType.Log, "Looping on abc/def/@PART:HAS[~aaa[>10]] to abc/def/PART");

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "16" },
                { "bbb", "002" },
                { "bbb", "002" },
                { "bbb", "002" },
                { "bbb", "002" },
            }, allConfigs[0].config);
        }

        [Fact]
        public void TestApplyPatches__InvalidOperator()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "1" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new ConfigNode("%PART"));
            UrlDir.UrlConfig patch2 = new UrlDir.UrlConfig(file, new ConfigNode("|PART"));
            UrlDir.UrlConfig patch3 = new UrlDir.UrlConfig(file, new ConfigNode("#PART"));
            UrlDir.UrlConfig patch4 = new UrlDir.UrlConfig(file, new ConfigNode("*PART"));
            UrlDir.UrlConfig patch5 = new UrlDir.UrlConfig(file, new ConfigNode("&PART"));

            patchList.firstPatches.Add(patch1);
            patchList.firstPatches.Add(patch2);
            patchList.firstPatches.Add(patch3);
            patchList.firstPatches.Add(patch4);
            patchList.firstPatches.Add(patch5);

            patchApplier.ApplyPatches();

            progress.DidNotReceiveWithAnyArgs().Error(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null, null);

            logger.DidNotReceive().Log(LogType.Error, Arg.Any<string>());
            logger.DidNotReceiveWithAnyArgs().Exception(null, null);

            logger.Received().Log(LogType.Warning, "Invalid command encountered on a patch: abc/def/%PART");
            logger.Received().Log(LogType.Warning, "Invalid command encountered on a patch: abc/def/|PART");
            logger.Received().Log(LogType.Warning, "Invalid command encountered on a patch: abc/def/#PART");
            logger.Received().Log(LogType.Warning, "Invalid command encountered on a patch: abc/def/*PART");
            logger.Received().Log(LogType.Warning, "Invalid command encountered on a patch: abc/def/&PART");

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "1" },
            }, allConfigs[0].config);

        }

        [Fact]
        public void TestApplyPatches__Copy__NameNotChanged()
        {
            UrlDir.UrlConfig config1 = UrlBuilder.CreateConfig(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, file);

            UrlDir.UrlConfig patch1 = new UrlDir.UrlConfig(file, new TestConfigNode("+PART")
            {
                { "@aaa", "011" },
                { "bbb", "012" },
            });

            patchList.firstPatches.Add(patch1);

            patchApplier.ApplyPatches();
            
            progress.DidNotReceiveWithAnyArgs().Exception(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null, null);

            progress.Received().Error(patch1, "Error - when applying copy abc/def/+PART to abc/def/PART - the copy needs to have a different name than the parent (use @name = xxx)");

            logger.DidNotReceive().Log(LogType.Warning, Arg.Any<string>());
            logger.DidNotReceive().Log(LogType.Error, Arg.Any<string>());
            logger.DidNotReceiveWithAnyArgs().Exception(null, null);

            progress.Received(1).PatchApplied();
            progress.DidNotReceiveWithAnyArgs().ApplyingCopy(null, null);

            UrlDir.UrlConfig[] allConfigs = databaseRoot.AllConfigs.ToArray();
            Assert.Equal(1, allConfigs.Length);

            AssertNodesEqual(new TestConfigNode("PART")
            {
                { "name", "000" },
                { "aaa", "001" },
            }, allConfigs[0].config);
        }

        private void EnsureNoErrors()
        {
            progress.DidNotReceiveWithAnyArgs().Error(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null);
            progress.DidNotReceiveWithAnyArgs().Exception(null, null, null);

            logger.DidNotReceive().Log(LogType.Warning, Arg.Any<string>());
            logger.DidNotReceive().Log(LogType.Error, Arg.Any<string>());
            logger.DidNotReceiveWithAnyArgs().Exception(null, null);
        }

        private void AssertNodesEqual(ConfigNode expected, ConfigNode actual)
        {
            Assert.Equal(expected.ToString(), actual.ToString());
        }
    }
}

﻿namespace Nodexr.Tests.RegexParserTests;
using NUnit.Framework;
using Pidgin;
using Nodexr.RegexParsers;

internal class IfElseParserTests
{
    [TestCase(@"(1)a|b", "1", "a", "b")]
    [TestCase(@"(abc)a|b", "abc", "a", "b")]
    [TestCase(@"(abc)(a)|(b)", "abc", "(a)", "(b)")]
    public void VariousGroups_ReturnsLookaroundWithContentsAndType(string input, string expectedCondition, string expectedContents1, string expectedContents2)
    {
        var result = IfElseParser.ParseIfElse.ParseOrThrow(input);
        var lookaround = result;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);

            Assert.AreEqual(expectedCondition, lookaround.InputCondition.GetValue());
            Assert.AreEqual(expectedContents1, lookaround.InputThen.Value.Expression);
            Assert.AreEqual(expectedContents2, lookaround.InputElse.Value.Expression);
        });
    }
}

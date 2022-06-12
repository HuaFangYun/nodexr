﻿namespace Nodexr.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

public class RegexReplaceHandler
{
    private readonly INodeHandler nodeHandler;
    private RegexOptions options = RegexOptions.None;
    private string searchText = DefaultSearchText;
    public const string DefaultReplacementRegex = "'$1'";

    public string ReplacementRegex { get; set; } = DefaultReplacementRegex;

    public const string DefaultSearchText = "Some people, when confronted with a problem, think \"I know, I'll use regular expressions.\" Now they have two problems.";

    public string SearchText
    {
        get => searchText;
        set
        {
            searchText = value;
            SearchTextChanged?.Invoke();
        }
    }

    public RegexOptions Options
    {
        get => options;
        set
        {
            options = value;
            RegexOptionsChanged?.Invoke();
        }
    }

    public event Action? RegexOptionsChanged;
    public event Action? SearchTextChanged;

    public RegexReplaceHandler(INodeHandler NodeHandler, NavigationManager navManager)
    {
        nodeHandler = NodeHandler;

        var uriParams = QueryHelpers.ParseQuery(navManager.ToAbsoluteUri(navManager.Uri).Query);
        if (uriParams.TryGetValue("search", out var searchString))
        {
            SearchText = searchString[0];
        }
        if (uriParams.TryGetValue("replace", out var replaceString))
        {
            ReplacementRegex = replaceString[0];
        }
    }

    public MatchCollection? GetAllMatches()
    {
        try
        {
            return Regex.Matches("" + SearchText, nodeHandler.CachedOutput.Expression, Options, TimeSpan.FromSeconds(0.5));
        }
        catch (RegexMatchTimeoutException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public string GetReplaceResult()
    {
        if (!IsRegexOptionsValid(Options))
        {
            return "ECMAScript mode must only be used with Multiline and Ignore Case flags";
        }

        string result;
        try
        {
            result = Regex.Replace(SearchText, nodeHandler.CachedOutput.Expression, ReplacementRegex, Options, TimeSpan.FromSeconds(0.5));
        }
        catch (RegexMatchTimeoutException ex)
        {
            result = "Regex replace timed out: " + ex.Message;
        }
        catch (Exception ex)
        {
            result = "Error: " + ex.Message;
        }
        return result;
    }

    private static bool IsRegexOptionsValid(RegexOptions options)
    {
        //Options can only be invalid in ECMAScript mode
        if (!options.HasFlag(RegexOptions.ECMAScript)) return true;

        const RegexOptions disallowedFlags = ~(
            RegexOptions.Multiline |
            RegexOptions.IgnoreCase);

        //Regex is only allowed to have multiline or ignoreCase flags when in ECMAScript mode
        return (options & disallowedFlags & ~RegexOptions.ECMAScript) == 0;
    }
}

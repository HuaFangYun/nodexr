﻿namespace Nodexr.Api.Contracts.NodeTrees;

public class NodeTreePreviewDto
{
    public string Id { get; set; } = null!;

    public string? Title { get; set; } = null!;

    public string? Expression { get; set; } = null!;

    public string? Description { get; set; }
}

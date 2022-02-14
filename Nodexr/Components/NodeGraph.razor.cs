﻿namespace Nodexr.Components;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Nodexr.Utils;
using Nodexr.NodeInputs;
using BlazorNodes.Core;
using Microsoft.AspNetCore.Components;
using Nodexr.Serialization;
using Nodexr.Services;
using System.Net.Http.Json;
using Nodexr.Api.Contracts.NodeTrees;
using System.Text.Json;

public partial class NodeGraph
{
    [Inject] protected INodeDragService NodeDragService { get; set; } = null!;
    [Inject] protected INoodleDragService NoodleDragService { get; set; } = null!;
    [Inject] protected INodeHandler NodeHandler { get; set; } = null!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] protected IConfiguration Config { get; set; } = null!;
    [Inject] protected HttpClient Http { get; set; } = null!;

    [Parameter]
    public string? NodeTreeId { get; set; }

    private bool shouldRender = false;
    protected override bool ShouldRender() => shouldRender;

    private Vector2 clickStartPos; //Used to check whether an onclick event was actually a drag

    private bool isLoadingNodeTree = false;

    private static Type InputViewModelProvider(INodeInput input)
    {
        return input switch
        {
            NodeInputs.InputCheckbox => typeof(InputCheckboxView),
            InputCheckboxNullable => typeof(InputCheckboxNullableView),
            InputCollection => typeof(InputCollectionView),
            InputDropdown => typeof(InputDropdownView),
            InputNumber => typeof(InputNumberView),
            InputProcedural => typeof(InputProceduralView),
            InputRange => typeof(InputRangeView),
            InputString => typeof(InputStringView),
            _ => throw new ArgumentOutOfRangeException($"No view class defined for type {input.GetType()}")
        };
    }

    protected override async Task OnInitializedAsync()
    {
        NodeHandler.OnRequireNodeGraphRefresh += (sender, e) => Refresh();
        if (!string.IsNullOrEmpty(NodeTreeId))
        {
            await LoadNodeTreeFromServer();
        }
    }

    private async Task LoadNodeTreeFromServer()
    {
        isLoadingNodeTree = true;
        var nodeTree = await Http.GetFromJsonAsync<NodeTreePreviewDto>($"{Config["apiAddress"]}/nodetree/{NodeTreeId}");

        var jsonOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = new CachePreservingReferenceHandler(),
            WriteIndented = true,
            Converters = { new RegexNodeJsonConverter() },
        };

        var nodes = nodeTree?.Nodes.Deserialize<List<INodeViewModel>>(jsonOptions);
        NodeHandler.Tree = new NodeTree(nodes);
        isLoadingNodeTree = false;
        Refresh();
    }

    private async Task StartPan(MouseEventArgs e)
    {
        clickStartPos = e.GetClientPos();
        await JSRuntime.InvokeVoidAsync("panzoom.startPan");
    }

    private void Refresh()
    {
        shouldRender = true;
        StateHasChanged();
        shouldRender = false;
    }

    private void OnDrop(DragEventArgs e)
    {
        NodeDragService.OnDrop(e);
        Refresh();
    }

    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Delete" || e.Key == "Backspace")
        {
            NodeHandler.DeleteSelectedNodes();
        }
        else if (e.Key == "Escape")
        {
            DeselectNode();
        }
    }

    private void OnClick(MouseEventArgs e)
    {
        const int dragThreshold = 4; //Length in px to consider a drag (instead of a click)
        var mouseOffset = e.GetClientPos() - clickStartPos;
        if (mouseOffset.GetLength() <= dragThreshold)
        {
            DeselectNode();
        }
    }

    private void DeselectNode()
    {
        NodeHandler.Tree.DeselectAllNodes();
    }
}
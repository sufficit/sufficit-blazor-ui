using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class AsyncTableTests
{
    [Fact]
    public async Task SupersededResultsCannotReplaceTheNewScope()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var pending = new List<TaskCompletionSource<SUITablePage<string>>>();
        var tokens = new List<CancellationToken>();
        Task<SUITablePage<string>> Load(SUITableQuery query, CancellationToken token)
        {
            tokens.Add(token);
            var source = new TaskCompletionSource<SUITablePage<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
            pending.Add(source);
            return source.Task;
        }
        var table = context.Render<SUIDataTable<string>>(p => p.Add(c => c.LoadPage, Load)
            .Add(c => c.QueryKey, "first").Add(c => c.RowTemplate, text => builder => builder.AddContent(0, text)));
        Assert.Single(pending);
        Assert.Empty(table.FindAll(".sui-table__empty-row .sui-empty"));
        Assert.Single(table.FindComponents<SUISkeletonLoader>());
        table.Render(p => p.Add(c => c.QueryKey, "second"));
        Assert.True(tokens[0].IsCancellationRequested);
        Assert.Equal(2, pending.Count);
        pending[1].SetResult(new() { Items = ["new row"], TotalItems = 1 });
        table.WaitForAssertion(() => Assert.Contains("new row", table.Markup));
        pending[0].SetResult(new() { Items = ["old row"], TotalItems = 1 });
        await table.InvokeAsync(() => Task.CompletedTask);
        Assert.DoesNotContain("old row", table.Markup);
    }

    [Fact]
    public async Task ExplicitFailedRefreshKeepsRowsAndShowsFailureInsteadOfEmptySuccess()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var calls = 0;
        Task<SUITablePage<string>> Load(SUITableQuery query, CancellationToken token)
        {
            calls++;
            return calls == 1 ? Task.FromResult(new SUITablePage<string> { Items = ["saved row"], TotalItems = 1 })
                : Task.FromException<SUITablePage<string>>(new InvalidOperationException("private server detail"));
        }
        var table = context.Render<SUIDataTable<string>>(p => p.Add(c => c.LoadPage, Load)
            .Add(c => c.RowTemplate, text => builder => builder.AddContent(0, text)));
        table.WaitForAssertion(() => Assert.Contains("saved row", table.Markup));
        await table.InvokeAsync(() => table.Instance.ReloadAsync());
        Assert.Contains("saved row", table.Markup);
        Assert.Single(table.FindAll("[role=alert]"));
        Assert.Contains("A lista anterior foi mantida", table.Markup);
        Assert.DoesNotContain("private server detail", table.Markup);
    }

    [Fact]
    public async Task ParentRendersAndControlledPaginationDoNotRepeatTheSameQuery()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var queries = new List<SUITableQuery>();
        Task<SUITablePage<string>> Load(SUITableQuery query, CancellationToken token)
        {
            queries.Add(query);
            return Task.FromResult(new SUITablePage<string> { Items = ["row"], TotalItems = 50 });
        }
        var table = context.Render<SUIDataTable<string>>(p => p.Add(c => c.LoadPage, Load).Add(c => c.ShowPagination, true));
        table.WaitForAssertion(() => Assert.Single(queries));
        table.Render(p => p.Add(c => c.Class, "new-class"));
        Assert.Single(queries);
        var pager = table.FindComponent<SUIPagination>();
        await table.InvokeAsync(() => pager.Instance.PageIndexChanged.InvokeAsync(1));
        Assert.Equal(2, queries.Count);
        Assert.Equal(1, queries[1].PageIndex);
        table.Render(p => p.Add(c => c.PageIndex, 1));
        Assert.Equal(2, queries.Count);
    }
    [Fact]
    public async Task SwitchingToLocalRowsCancelsTheServerAndReloadsOnReturn()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var requests = new List<TaskCompletionSource<SUITablePage<string>>>();
        var tokens = new List<CancellationToken>();
        Task<SUITablePage<string>> Load(SUITableQuery query, CancellationToken token)
        {
            tokens.Add(token);
            var request = new TaskCompletionSource<SUITablePage<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
            requests.Add(request);
            return request.Task;
        }
        var table = context.Render<SUIDataTable<string>>(p => p.Add(c => c.LoadPage, Load)
            .Add(c => c.RowTemplate, text => builder => builder.AddContent(0, text)));
        table.Render(p => p.Add(c => c.LoadPage, null).Add(c => c.Items, new[] { "local row" }));
        Assert.True(tokens[0].IsCancellationRequested);
        requests[0].SetResult(new() { Items = ["obsolete server row"], TotalItems = 1 });
        await table.InvokeAsync(() => Task.CompletedTask);
        Assert.Contains("local row", table.Markup);
        Assert.DoesNotContain("obsolete server row", table.Markup);
        table.Render(p => p.Add(c => c.LoadPage, Load));
        Assert.Equal(2, requests.Count);
        requests[1].SetResult(new() { Items = ["fresh server row"], TotalItems = 1 });
        table.WaitForAssertion(() => Assert.Contains("fresh server row", table.Markup));
        Assert.DoesNotContain("local row", table.Markup);
    }

}

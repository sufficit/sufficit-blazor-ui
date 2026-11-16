using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class MediaComponentsTests
{
    [Fact]
    public async Task FileSelectionReportsTheFileWithoutOpeningItsStreamAndExternalClearResetsInput()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var file = new TestFile();
        IBrowserFile? selected = null;
        var cut = context.Render<SUIFileUpload>(p => p.Add(c => c.Id, "audio")
            .Add(c => c.Label, "Arquivo de áudio").Add(c => c.HelperText, "Até 20 MB")
            .Add(c => c.ValueChanged, value => selected = value));
        Assert.Equal("audio", cut.Find("label").GetAttribute("for"));
        Assert.Contains("audio-helper", cut.Find("input").GetAttribute("aria-describedby"));
        await cut.InvokeAsync(() => cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([file])));
        Assert.Same(file, selected);
        Assert.Contains(file.Name, cut.Find("[role=status]").TextContent);
        Assert.Equal(0, file.Reads);
        var input = cut.Find("input");
        cut.Render(p => p.Add(c => c.Value, (IBrowserFile?)null));
        Assert.NotSame(input, cut.Find("input"));
        Assert.Contains("Nenhum arquivo", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public async Task OversizedSelectionClearsTheOldFileAndAnnouncesValidation()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        IBrowserFile? selected = new TestFile();
        var cut = context.Render<SUIFileUpload>(p => p.Add(c => c.MaxFileSize, 4L)
            .Add(c => c.ValueChanged, value => selected = value));
        await cut.InvokeAsync(() => cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([new TestFile()])));
        Assert.Null(selected);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        Assert.Contains(cut.Find("[role=alert]").Id!, cut.Find("input").GetAttribute("aria-describedby"));
        Assert.Contains("Nenhum arquivo", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public async Task DisabledChooserDoesNotReportASelection()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var called = false;
        var cut = context.Render<SUIFileUpload>(p => p.Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, _ => called = true));
        Assert.True(cut.Find("input").HasAttribute("disabled"));
        await cut.InvokeAsync(() => cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([new TestFile()])));
        Assert.False(called);
    }

    [Fact]
    public void PlayerRetainsNativeControlsDownloadAndClearsFailureOnSourceChange()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIAudioPlayer>(p => p.Add(c => c.Source, "/first.wav")
            .Add(c => c.Label, "Prévia do atendimento").Add(c => c.DownloadName, "atendimento.wav"));
        Assert.True(cut.Find("audio").HasAttribute("controls"));
        Assert.False(cut.Find("audio").HasAttribute("autoplay"));
        Assert.Equal("Prévia do atendimento", cut.Find("audio").GetAttribute("aria-label"));
        Assert.Equal("atendimento.wav", cut.Find("a").GetAttribute("download"));
        cut.Find("audio").TriggerEvent("onerror", EventArgs.Empty);
        Assert.Single(cut.FindAll("[role=alert]"));
        cut.Render(p => p.Add(c => c.Source, "/second.wav"));
        Assert.Empty(cut.FindAll("[role=alert]"));
        Assert.Equal("/second.wav", cut.Find("audio").GetAttribute("src"));
        cut.Render(p => p.Add(c => c.Source, (string?)null));
        Assert.Empty(cut.FindAll("audio"));
        Assert.Empty(cut.FindAll("a"));
    }

    private sealed class TestFile : IBrowserFile
    {
        public string Name => "atendimento-com-nome-longo.wav";
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public long Size => 8;
        public string ContentType => "audio/wav";
        public int Reads { get; private set; }
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        { Reads++; return new MemoryStream(new byte[8]); }
    }
}

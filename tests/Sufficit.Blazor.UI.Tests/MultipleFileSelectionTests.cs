using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class MultipleFileSelectionTests
{
    [Fact]
    public async Task SelectionReportsAllFilesWithoutReadingAndNextSelectionReplacesThem()
    {
        using var context=new BunitContext();context.JSInterop.Mode=JSRuntimeMode.Loose;
        IReadOnlyList<IBrowserFile> selected=[];var first=new File("first.wav");var second=new File("second.mp3");var third=new File("third.wav");
        var cut=context.Render<SUIFileUpload>(p=>p.Add(c=>c.Multiple,true).Add(c=>c.MaxFiles,2).Add(c=>c.FilesChanged,files=>selected=files));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([first,second])));
        Assert.Equal(new IBrowserFile[]{first,second},selected);Assert.Equal(0,first.Reads+second.Reads);Assert.True(cut.Find("input").HasAttribute("multiple"));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([third])));
        Assert.Same(third,Assert.Single(selected));Assert.Equal(0,third.Reads);
    }
    [Fact]
    public async Task CountOverflowRejectsTheWholeSelectionAndAnnouncesAnAccessibleError()
    {
        using var context=new BunitContext();context.JSInterop.Mode=JSRuntimeMode.Loose;
        IReadOnlyList<IBrowserFile>? selected=null;var files=new[]{new File("a.wav"),new File("b.wav"),new File("c.wav")};
        var cut=context.Render<SUIFileUpload>(p=>p.Add(c=>c.Multiple,true).Add(c=>c.MaxFiles,2).Add(c=>c.FilesChanged,value=>selected=value));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs(files)));
        Assert.NotNull(selected);Assert.Empty(selected);Assert.Equal("true",cut.Find("input").GetAttribute("aria-invalid"));Assert.Contains(cut.Find("[role=alert]").Id!,cut.Find("input").GetAttribute("aria-describedby"));Assert.All(files,file=>Assert.Equal(0,file.Reads));
    }
    [Fact]
    public async Task SizeRejectionKeepsAllowedFilesWithoutOpeningAnyOfTheirStreams()
    {
        using var context=new BunitContext();context.JSInterop.Mode=JSRuntimeMode.Loose;
        IReadOnlyList<IBrowserFile> selected=[];var small=new File("small.wav",8);var large=new File("large.wav",16);
        var cut=context.Render<SUIFileUpload>(p=>p.Add(c=>c.Multiple,true).Add(c=>c.MaxFileSize,10).Add(c=>c.FilesChanged,files=>selected=files));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([small,large])));
        Assert.Same(small,Assert.Single(selected));Assert.Single(cut.FindAll("[role=alert]"));Assert.Equal(0,small.Reads+large.Reads);
    }
    [Fact]
    public async Task ClearReplacesTheNativeInputAndDisabledSelectionDoesNotNotifyTheOwner()
    {
        using var context=new BunitContext();context.JSInterop.Mode=JSRuntimeMode.Loose;var called=0;var file=new File("audio.wav");
        var cut=context.Render<SUIFileUpload>(p=>p.Add(c=>c.Multiple,true).Add(c=>c.FilesChanged,_=>called++));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([file])));var input=cut.Find("input");
        cut.Render(p=>p.Add(c=>c.Files,Array.Empty<IBrowserFile>()).Add(c=>c.Disabled,true));Assert.NotSame(input,cut.Find("input"));
        await cut.InvokeAsync(()=>cut.FindComponent<InputFile>().Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([file])));Assert.Equal(1,called);Assert.Equal(0,file.Reads);
    }
    private sealed class File(string name,long size=8):IBrowserFile
    {
        public string Name=>name;public DateTimeOffset LastModified=>DateTimeOffset.UnixEpoch;public long Size=>size;public string ContentType=>"audio/wav";public int Reads {get;private set;}
        public Stream OpenReadStream(long maxAllowedSize=512000,CancellationToken cancellationToken=default){Reads++;throw new InvalidOperationException("Selection must not read files.");}
    }
}

using PhotoArchivingTools.Views;
using System;

namespace PhotoArchivingTools.ViewModels;

public class ToolPanelInfo
{
    private ToolPanelInfo()
    {
    }

    public static ToolPanelInfo Create<T>(string title, string description, string iconUri = null) where T : PanelBase
    {
        return new ToolPanelInfo
        {
            Title = title,
            Description = description,
            IconUri = iconUri,
            PanelType = typeof(T)
        };
    }

    public string Description { get; private set; }
    public string IconUri { get; private set; }
    public string Title { get; private set; }
    public Type PanelType { get; private set; }
    public PanelBase PanelInstance { get; set; }
}

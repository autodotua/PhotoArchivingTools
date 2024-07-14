using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PhotoArchivingTools.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static PhotoArchivingTools.ViewModels.MainViewModel;

namespace PhotoArchivingTools.ViewModels;
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object mainContent;

    public List<ToolPanelInfo> Panels { get; } = new List<ToolPanelInfo>()
    {
         ToolPanelInfo.Create<TimeClassifyPanel>("根据时间段归档","识别目录中相同时间段的文件，将它们移动到相同的新目录中","/Assets/archive.svg"),
         ToolPanelInfo.Create<UselessJpgCleanerPanel>("删除多余JPG","删除目录中存在同名RAW文件的JPG文件","/Assets/jpg.svg"),
         ToolPanelInfo.Create<RepairModifiedTimePanel>("修复文件修改时间","寻找EXIF信息中的拍摄时间与照片修改时间不同的文件，将修改时间更新闻EXIF时间","/Assets/time.svg"),
         ToolPanelInfo.Create<PhotoSlimmingPanel>("创建照片集合副本","复制或压缩照片，用于生成更小的照片集副本","/Assets/zip.svg"),
         ToolPanelInfo.Create<EncryptorPanel>("文件加密解密","使用AES加密方法，对文件进行加密或解密","/Assets/encrypt.svg"),
    };

    [ObservableProperty]
    private bool isToolOpened;

    [RelayCommand]
    private void EnterTool(ToolPanelInfo panelInfo)
    {
        if (panelInfo.PanelInstance == null)
        {
            panelInfo.PanelInstance = Activator.CreateInstance(panelInfo.PanelType) as PanelBase;
            panelInfo.PanelInstance.RequestClosing += (s, e) =>
            {
                IsToolOpened = false;
            };
        }
        PanelBase panel = panelInfo.PanelInstance;
        panel.Title = panelInfo.Title;
        panel.Description = panelInfo.Description;
        MainContent = panel;
        IsToolOpened = true;
    }
}

# 照片归档工具

## 简介

一套用来解决照片（以及部分文件）在预处理、整理、存档、浏览等环节存在问题的自动化工具

| 工具名                       | 用途                                                         | 期望解决的问题                                               | 英文名                    |
| ---------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------- |
| 相同时间段照片归档           | 识别目录中相同时间段的文件，将它们移动到相同的新目录中       | 例如对于航拍照片，多次起飞的照片和视频会混在一起。通过该工具，可以通过拍摄时间来自动将不同航次的照片和视频进行归类。 | `TimeClassifyPanel`       |
| 删除存在RAW的JPG             | 删除目录中存在同名RAW文件的JPG文件                           | 当拍摄了RAW+JPG格式的照片时，若决定处理RAW文件，那么JPG格式是多余的，需要删除。 | `UselessJpgCleanerPanel`  |
| 使用EXIF时间更新文件修改时间 | 寻找EXIF信息中的拍摄时间与照片修改时间不同的文件，将修改时间更新闻EXIF时间 | 对照片进行处理后，文件修改时间会更新，不利于部分软件的照片排序，需要从EXIF中读取实际拍摄时间，对文件修改时间进行更新。 | `RepairModifiedTimePanel` |
| 创建照片集合副本             | 复制或压缩照片，用于生成更小的照片集副本                     | 需要将硬盘中整理后的部分照片复制到手机中以便随时查看，但可能存在文件过大放不进手机、只需要部分目录中的文件、只需要部分类型文件等需求。 | `PhotoSlimmingPanel`      |

## 截图

![主界面1](imgs/P1.png)

![主界面2](imgs/P2.png)

![“创建照片集合副本”工具配置界面](imgs/P3.png)

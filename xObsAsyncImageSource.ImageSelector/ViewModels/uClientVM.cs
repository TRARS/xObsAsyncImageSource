using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using xObsAsyncImageSource.ImageSelector.Interfaces;
using xObsAsyncImageSource.ImageSelector.Models;
using xObsAsyncImageSource.ImageSelector.Wrapper;
using xObsAsyncImageSource.SocketUtils.Helper;

namespace xObsAsyncImageSource.ImageSelector.ViewModels
{
    public partial class uClientVM
    {
        [ObservableProperty]
        private string serverIP;

        [ObservableProperty]
        private int serverPort;

        [ObservableProperty]
        private bool serverIsOffLine;

        [ObservableProperty]
        private List<string> onlineClientList;

        [ObservableProperty]
        private string currentTargetClientName;

        [ObservableProperty]
        private string currentMessage;

        public ObservableCollection<ImageInfo> ImageList { get; private set; }

        [ObservableProperty]
        private ImageInfo currentImage;

        public uClientVM()
        {
            ServerIsOffLine = true;

            PrepareServerInfo();
            PrepareImageList();

            ServerManager.OnOnlineListUpdated += (s, e) =>
            {
                OnlineClientList = new List<string>([((xObsAsyncImageSource.SocketUtils.Role.ChatServer)s!).Name, .. e]);
            };
        }
    }

    public partial class uClientVM : ObservableObject, IuClientVM
    {
        [RelayCommand]
        void OnStartServer()
        {
            _ = ServerManager.StartServer(ServerIP, ServerPort, (port) => { ServerPort = port; }, () => { ServerIsOffLine = false; });
        }

        [RelayCommand]
        async Task OnPushPng2ObsAsync()
        {
            await ServerManager.PushPng2Obs(CurrentTargetClientName, CurrentMessage);
        }

        [RelayCommand]
        async Task OnImageChangedAsync()
        {
            if (CurrentImage is null) { return; }

            var uri = CurrentImage.ImageSource;
            var tempFolder = CreateTempDirectory();
            var savePath = Path.Combine(tempFolder, "temp.png");

            SaveToTempDirectory(uri, savePath);

            CurrentMessage = string.Empty;
            await ServerManager.PushPng2Obs(CurrentTargetClientName, CurrentMessage = savePath, false);
        }
    }

    public partial class uClientVM
    {
        private void PrepareServerInfo()
        {
            ServerIP = LocalAddressRepository.LocalAddress.ToString();
            ServerPort = 0;
        }

        private void PrepareImageList()
        {
            ImageList = new();

            int[] ImageId = Shuffle(Enumerable.Range(0, 40).ToArray());

            for (int i = 0; i < ImageId.Length; i++)
            {
                ImageList.Add(new()
                {
                    ImageName = $"{ImageId[i] + 1:00}.png",
                    ImageSource = $"./Resources/Icon/Bot/{ImageId[i] + 1:00}.png", // start with 1
                    IsChecked = i % 2 == 0
                });
            }
        }

        private T[] Shuffle<T>(T[] input)
        {
            Action<T[], int, int> swapAction = (array, i, j) =>
            {
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            };

            var rand = new Random(new Random().Next());

            for (int i = input.Length - 1; i > 0; i--)
            {
                swapAction(input, i, rand.Next(0, i + 1));
            }

            return input;
        }

        private string CreateTempDirectory()
        {
            // 获取程序当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();

            // 定义要创建的 Temp 文件夹路径
            string tempFolderPath = Path.Combine(currentDirectory, "Temp");

            // 检查文件夹是否已经存在，如果不存在则创建
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            return tempFolderPath;
        }
        private void SaveToTempDirectory(string relativeUri, string localFilePath)
        {
            try
            {
                // 获取资源流
                var uri = new Uri(relativeUri, UriKind.Relative);
                var resourceStream = Application.GetResourceStream(uri);

                if (resourceStream != null)
                {
                    // 使用 FileStream 写入文件
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        // 将资源流复制到本地文件
                        resourceStream.Stream.CopyTo(fileStream);
                    }
                }
                else
                {
                    MessageBox.Show("无法找到指定的资源。");
                }
            }
            catch (IOException ex)
            {
                // 处理 IOException（例如，文件被占用）
                MessageBox.Show($"文件操作失败:\n{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                // 处理文件权限问题
                MessageBox.Show($"文件权限错误:\n{ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常
                MessageBox.Show($"发生错误:\n{ex.Message}");
            }
        }
    }
}

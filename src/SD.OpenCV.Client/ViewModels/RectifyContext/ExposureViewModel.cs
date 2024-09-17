using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.RectifyContext
{
    /// <summary>
    /// 曝光融合视图模型
    /// </summary>
    public class ExposureViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ExposureViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 已选图像源 —— Wrap<BitmapSource> SelectedBitmapSource
        /// <summary>
        /// 已选图像源
        /// </summary>
        [DependencyProperty]
        public Wrap<BitmapSource> SelectedBitmapSource { get; set; }
        #endregion

        #region 图像源列表 —— ObservableCollection<Wrap<BitmapSource>> BitmapSources
        /// <summary>
        /// 图像源列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Wrap<BitmapSource>> BitmapSources { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            this.BitmapSources = new ObservableCollection<Wrap<BitmapSource>>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 打开图像 —— async void OpenImages()
        /// <summary>
        /// 打开图像
        /// </summary>
        public async void OpenImages()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                Multiselect = true,
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                #region # 验证

                if (!openFileDialog.FileNames.Any())
                {
                    MessageBox.Show("未选择图像！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.Busy();

                foreach (string fileName in openFileDialog.FileNames)
                {
                    using Mat image = await Task.Run(() => Cv2.ImRead(fileName));
                    BitmapSource bitmapSource = image.ToBitmapSource();
                    this.BitmapSources.Add(bitmapSource.Wrap());
                }

                //默认预览第一张
                this.SelectedBitmapSource = this.BitmapSources.First();

                this.Idle();
            }
        }
        #endregion

        #region 曝光融合 —— async void ExposureFusion()
        /// <summary>
        /// 曝光融合
        /// </summary>
        public async void ExposureFusion()
        {
            #region # 验证

            if (this.BitmapSources.All(x => x.IsChecked == false))
            {
                MessageBox.Show("图像未选择！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            Mat[] images = this.BitmapSources.Where(x => x.IsChecked == true).Select(x => x.Model.ToMat()).ToArray();
            using Mat mergedImage = await Task.Run(() => images.ExposureFusion());

            BitmapSource bitmapSource = mergedImage.ToBitmapSource();
            Wrap<BitmapSource> wrapModel = bitmapSource.Wrap();
            this.SelectedBitmapSource = wrapModel;
            this.BitmapSources.Add(wrapModel);

            //释放资源
            foreach (Mat image in images)
            {
                image.Dispose();
            }

            this.Idle();
        }
        #endregion

        #region 另存为图像 —— async void SaveAsImage()
        /// <summary>
        /// 另存为图像
        /// </summary>
        public async void SaveAsImage()
        {
            #region # 验证

            if (this.SelectedBitmapSource == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                this.Busy();
                using Mat image = this.SelectedBitmapSource.Model.ToMat();
                await Task.Run(() => image.SaveImage(saveFileDialog.FileName));

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
        }
        #endregion

        #region 删除图像 —— void RemoveImage()
        /// <summary>
        /// 删除图像
        /// </summary>
        public void RemoveImage()
        {
            if (this.SelectedBitmapSource != null)
            {
                this.BitmapSources.Remove(this.SelectedBitmapSource);
            }
        }
        #endregion

        #region 全选图像 —— void CheckAll()
        /// <summary>
        /// 全选图像
        /// </summary>
        public void CheckAll()
        {
            if (this.BitmapSources.All(x => x.IsChecked == true))
            {
                this.BitmapSources.ForEach(x => x.IsChecked = false);
            }
            else
            {
                this.BitmapSources.ForEach(x => x.IsChecked = true);
            }
        }
        #endregion

        #endregion
    }
}

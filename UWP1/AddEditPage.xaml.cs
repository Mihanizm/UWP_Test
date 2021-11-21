using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.Graphics.Imaging;

namespace UWP1
{
    public sealed partial class AddEditPage : Page
    {
        private Agent agent;
        private BitmapImage bitmapImage;
        private SoftwareBitmap softwareBitmap;
        private Brush brushBorderTextBoxDefault;
        private Brush brushBorderTextBoxWrong;
        private byte[] byteArray;
        int pixelH;
        int pixelW;

        private string add_Mode = "Добавление агента";
        private string edit_Mode = "Изменение агента";
        public AddEditPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            brushBorderTextBoxDefault = nameTextBox.BorderBrush;
            brushBorderTextBoxWrong = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));

            if (e.Parameter != null)
            {
                int id = (int)e.Parameter;
                using (AgentContext db = new AgentContext())
                {
                    agent = db.Agents.FirstOrDefault(c => c.Id == id);
                }

                headerTextBlock.Text = edit_Mode;
            }
            else
            {
                headerTextBlock.Text = add_Mode;
            }

            if (agent != null)
            {
                byteArray = agent.Photo;
                pixelW = agent.PhotoWidth;
                pixelH = agent.PhotoHeiht;
                nameTextBox.Text = agent.Name;
                phoneTextBox.Text = agent.Phone;

                if (agent.Email == null)
                {
                    emailTextBox.Text = "";
                }
                else
                {
                    emailTextBox.Text = agent.Email;
                }

                WriteableBitmap bitmap = new WriteableBitmap(agent.PhotoWidth, agent.PhotoHeiht);
                await bitmap.PixelBuffer.AsStream().WriteAsync(agent.Photo, 0, agent.Photo.Length);
                overviewImage.Source = bitmap;
            }
            else
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/place_holder_img.png"));

                IRandomAccessStream stream1 = await file.OpenAsync(FileAccessMode.Read);

                BitmapDecoder decoder1 = await BitmapDecoder.CreateAsync(stream1);
                softwareBitmap = await decoder1.GetSoftwareBitmapAsync();

                StorageFile outputFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tmp.png", CreationCollisionOption.ReplaceExisting);

                if (outputFile == null)
                {
                    return;
                }

                SaveSoftwareBitmapToFile(softwareBitmap, outputFile);

                using (var inputStream = await file.OpenSequentialReadAsync())
                {
                    var readStream = inputStream.AsStreamForRead();

                    byteArray = new byte[readStream.Length];
                    await readStream.ReadAsync(byteArray, 0, byteArray.Length);

                    bitmapImage = new BitmapImage();
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    await bitmapImage.SetSourceAsync(stream);

                    Windows.Graphics.Imaging.BitmapDecoder decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                    Windows.Graphics.Imaging.PixelDataProvider pixelData = await decoder.GetPixelDataAsync();
                    byteArray = pixelData.DetachPixelData();
                    pixelH = bitmapImage.PixelHeight;
                    pixelW = bitmapImage.PixelWidth;

                    WriteableBitmap bitmap = new WriteableBitmap(pixelW, pixelH);
                    await bitmap.PixelBuffer.AsStream().WriteAsync(byteArray, 0, byteArray.Length);
                    overviewImage.Source = bitmap;
                }
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            GoToMainPage();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool checkFlag = true;

            Regex regex = new Regex(@"^[А-Я][а-я]* [А-Я].[А-Я].$");
            if (regex.Match(nameTextBox.Text).Success)
            {
                nameTextBox.BorderBrush = brushBorderTextBoxDefault;
            }
            else
            {
                nameTextBox.BorderBrush = brushBorderTextBoxWrong;
                checkFlag = false;
            }

            regex = new Regex(@"^8[0-9]{10}$");
            if (regex.Match(phoneTextBox.Text).Success)
            {
                phoneTextBox.BorderBrush = brushBorderTextBoxDefault;
            }
            else
            {
                phoneTextBox.BorderBrush = brushBorderTextBoxWrong;
                checkFlag = false;
            }

            regex = new Regex(@"(_*[A-Za-z0-9]+([\.-].[A-Za-z0-9_]+)*@[A-Za-z0-9]+([-.][A-Za-z0-9]+)*\.[A-Za-z0-9]+([-.][A-Za-z0-9]+)*)");
            if (regex.Match(emailTextBox.Text).Success || emailTextBox.Text == "")
            {
                emailTextBox.BorderBrush = brushBorderTextBoxDefault;
            }
            else
            {
                emailTextBox.BorderBrush = brushBorderTextBoxWrong;
                checkFlag = false;
            }

            if (checkFlag)
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("tmp.png");

                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

                bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);

                BitmapDecoder decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync();
                byteArray = pixelData.DetachPixelData();
                pixelH = bitmapImage.PixelHeight;
                pixelW = bitmapImage.PixelWidth;

                using (AgentContext db = new AgentContext())
                {
                    string emailAgent;

                    if (emailTextBox.Text == "")
                    {
                        emailAgent = null;
                    }
                    else
                    {
                        emailAgent = emailTextBox.Text;
                    }

                    if (agent != null)
                    {
                        agent.Name = nameTextBox.Text;
                        agent.Phone = phoneTextBox.Text;
                        agent.Email = emailAgent;
                        agent.Photo = byteArray;
                        agent.PhotoWidth = pixelW;
                        agent.PhotoHeiht = pixelH;
                        db.Agents.Update(agent);
                    }
                    else
                    {
                        db.Agents.Add(new Agent
                        {
                            Name = nameTextBox.Text,
                            Phone = phoneTextBox.Text,
                            Email = emailAgent,
                            Photo = byteArray,
                            PhotoWidth = pixelW,
                            PhotoHeiht = pixelH
                        });
                    }
                    db.SaveChanges();
                }
                GoToMainPage();
            }
            else
            {

            }
        }
        private void GoToMainPage()
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(MainPage));
        }

        private async void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            StorageFile outputFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tmp.png", CreationCollisionOption.ReplaceExisting);

            if (outputFile == null)
            {
                return;
            }

            SaveSoftwareBitmapToFile(softwareBitmap, outputFile);

            bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);

            decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            PixelDataProvider pixelData = await decoder.GetPixelDataAsync();
            byteArray = pixelData.DetachPixelData();
            pixelH = bitmapImage.PixelHeight;
            pixelW = bitmapImage.PixelWidth;

            WriteableBitmap bitmap = new WriteableBitmap(pixelW, pixelH);
            await bitmap.PixelBuffer.AsStream().WriteAsync(byteArray, 0, byteArray.Length);
            overviewImage.Source = bitmap;
        }
        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);


                encoder.BitmapTransform.ScaledWidth = (uint)Math.Round(overviewImage.Width);
                encoder.BitmapTransform.ScaledHeight = (uint)Math.Round(overviewImage.Height);
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }
    }
}

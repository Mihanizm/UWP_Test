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
using Microsoft.EntityFrameworkCore;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace UWP1
{
    public sealed partial class MainPage : Page
    {
        private string empty_mainListBox = "Данные отсутствуют";
        private string fill_mainListBox = "Выберите агента из списка";
        private string empty_emailAgent = "Не указана";
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMainPage();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddEditPage));
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem != null)
            {
                Agent agent = mainListBox.SelectedItem as Agent;
                if (agent != null)
                    Frame.Navigate(typeof(AddEditPage), agent.Id);
            }
            else
            {

            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem != null)
            {
                Agent agent = mainListBox.SelectedItem as Agent;
                if (agent != null)
                {
                    using (AgentContext db = new AgentContext())
                    {
                        db.Agents.Remove(agent);
                        db.SaveChanges();
                        UpdateMainPage();
                    }
                }
                else
                {

                }
            }
        }

        private async void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            nTextBlock.Visibility =
                nameTextBlock.Visibility =
                pTextBlock.Visibility =
                phoneTextBlock.Visibility =
                eTextBlock.Visibility =
                emailTextBlock.Visibility =
                phTextBlock.Visibility =
                photoImage.Visibility = Visibility.Visible;

            helpTextBlock.Visibility = Visibility.Collapsed;

            if (mainListBox.SelectedItem != null)
            {
                ListBoxItem lbi2 = (ListBoxItem)mainListBox.ContainerFromItem(mainListBox.SelectedItem);
                lbi2.Background = mainListBox.Background;

                Agent agent = mainListBox.SelectedItem as Agent;
                if (agent != null)
                {
                    nameTextBlock.Text = agent.Name;
                    phoneTextBlock.Text = agent.Phone;

                    if (agent.Email == null)
                    {
                        emailTextBlock.Text = empty_emailAgent;
                    }
                    else
                    {
                        emailTextBlock.Text = agent.Email;
                    }

                    WriteableBitmap bitmap = new WriteableBitmap(agent.PhotoWidth, agent.PhotoHeiht);
                    await bitmap.PixelBuffer.AsStream().WriteAsync(agent.Photo, 0, agent.Photo.Length);
                    photoImage.Source = bitmap;
                }
            }
            else
            {

            }
        }
        private void UpdateMainPage()
        {
            using (AgentContext db = new AgentContext())
            {
                mainListBox.ItemsSource = db.Agents.FromSql("SELECT * FROM Agents ORDER BY Name").ToList();
            }

            infoTextBlock.Text = ApplicationData.Current.LocalFolder.Path;

            if (mainListBox.SelectedItem == null)
            {
                nTextBlock.Visibility =
                    nameTextBlock.Visibility =
                    pTextBlock.Visibility =
                    phoneTextBlock.Visibility =
                    eTextBlock.Visibility =
                    emailTextBlock.Visibility =
                    phTextBlock.Visibility =
                    photoImage.Visibility = Visibility.Collapsed;

                helpTextBlock.Visibility = Visibility.Visible;

                if (mainListBox.Items.Count == 0)
                {
                    helpTextBlock.Text = empty_mainListBox;
                }
                else
                {
                    helpTextBlock.Text = fill_mainListBox;
                }
            }
        }
    }
}
